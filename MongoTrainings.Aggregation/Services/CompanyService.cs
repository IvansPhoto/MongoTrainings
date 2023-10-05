using Bogus;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoTrainings.Aggregation.Entities;

namespace MongoTrainings.Aggregation.Services;

public interface ICompanyService
{
    Task<IEnumerable<CompanyDto>> GetCompanies(CancellationToken token, int skip = 0, int take = 100);
    Task<Result> FullAggregation(CancellationToken token, int skip = 0, int take = 100);
    Task FillDataBase(int quantity, CancellationToken token);
    Task CleanCollections(CancellationToken token);
}

public class CompanyService : ICompanyService
{
    private readonly IMongoCollection<Company> _companyCollection;
    private readonly IMongoCollection<Site> _siteCollection;
    private readonly IMongoCollection<Employee> _employeeCollection;

    public CompanyService(IOptions<MongoConfig> options)
    {
        var mongoDatabase = new MongoClient(options.Value.GetMongoConnectionString())
            .GetDatabase(MongoConfig.MasDbName);

        _companyCollection = mongoDatabase.GetCollection<Company>(MongoConfig.CompanyCollection);
        _siteCollection = mongoDatabase.GetCollection<Site>(MongoConfig.SiteCollection);
        _employeeCollection = mongoDatabase.GetCollection<Employee>(MongoConfig.EmployeeCollection);
    }

    public async Task FillDataBase(int quantity, CancellationToken token)
    {
        var fakeEmployee = new Faker<Employee>()
            .RuleFor(employee => employee.FullName, faker => faker.Name.FullName())
            .Generate(quantity * quantity);

        var fakeSites = new Faker<Site>()
            .RuleFor(site => site.SiteName, faker => faker.Company.CompanyName())
            .RuleFor(site => site.SiteAddress, faker => faker.Address.FullAddress())
            .RuleFor(site => site.Id, ObjectId.GenerateNewId)
            .RuleFor(site => site.EmployeeIds, faker => faker.Random.Shuffle(fakeEmployee.Take(quantity).Select(employee => employee.Id)))
            .Generate(quantity * quantity);

        var fakeCompanies = new Faker<Company>()
            .RuleFor(company => company.CompanyName, faker => faker.Company.CompanyName())
            .RuleFor(company => company.CompanyDescription, faker => faker.Company.Random.Words(10))
            .RuleFor(company => company.HqSiteId, ObjectId.Empty)
            .Generate(quantity)
            .Select((company, i) =>
            {
                company.SiteIds = fakeSites.Select(site => site.Id).Skip(i * quantity).Take(quantity);
                return company;
            });

        await _siteCollection.InsertManyAsync(fakeSites, new InsertManyOptions(), token);

        await _companyCollection.InsertManyAsync(fakeCompanies, new InsertManyOptions(), token);
    }

    public async Task<IEnumerable<CompanyDto>> GetCompanies(CancellationToken token, int skip = 0, int take = 100)
    {
        return await _companyCollection
            .Aggregate()
            .Match(FilterDefinition<Company>.Empty)
            .Skip(skip)
            .Limit(take)
            .Lookup(
                foreignCollection: _siteCollection,
                foreignField: site => site.Id, 
                localField: (Company company) => company.SiteIds,
                options: new AggregateLookupOptions<Site, CompanyDto>(),
                @as: (CompanyDto company) => company.Sites
            )
            .ToListAsync(token);
    }

    public async Task<Result> FullAggregation(CancellationToken token, int skip = 0, int take = 100)
    {
        const string facetNameHq = "Hq";
        const string facetNameCount = "Count";

        var facetHq = AggregateFacet.Create(facetNameHq,
            PipelineDefinition<Company, CompanyDto>.Create(new IPipelineStageDefinition[]
                {
                    PipelineStageDefinitionBuilder.Sort(Builders<Company>.Sort.Ascending(company => company.CompanyName)),
                    PipelineStageDefinitionBuilder.Skip<Company>(skip),
                    PipelineStageDefinitionBuilder.Limit<Company>(take),
                    PipelineStageDefinitionBuilder.Lookup<Company, Site, CompanyDto>(
                        foreignCollection: _siteCollection,
                        foreignField: site => site.Id,
                        localField: company => company.SiteIds,
                        @as: company => company.Sites),
                    PipelineStageDefinitionBuilder.Lookup<Company, Site, CompanyDto>(
                        foreignCollection: _siteCollection,
                        foreignField: site => site.Id,
                        localField: company => company.HqSiteId,
                        @as: company => company.HqSite)
                }
            ));

        var facetCount = AggregateFacet.Create(facetNameCount,
            PipelineDefinition<Company, AggregateCountResult>.Create(new[]
                {
                    PipelineStageDefinitionBuilder.Count<Company>()
                }
            ));
        
        var aggregateFacets = await _companyCollection
            .Aggregate()
            .Match(FilterDefinition<Company>.Empty)
            .Facet(facetHq, facetCount)
            .ToListAsync(token);

        var companies = aggregateFacets.First().Facets
            .First(facet => facet.Name == facetNameHq)
            .Output<CompanyDto>();
        var count = aggregateFacets.First().Facets
            .First(facet => facet.Name == facetNameCount)
            .Output<AggregateCountResult>()[0].Count;
        return new Result(count, companies);
    }

    public async Task CleanCollections(CancellationToken token) => await Task.WhenAll(
        _companyCollection.DeleteManyAsync(FilterDefinition<Company>.Empty, token),
        _siteCollection.DeleteManyAsync(FilterDefinition<Site>.Empty, token)
    );
}