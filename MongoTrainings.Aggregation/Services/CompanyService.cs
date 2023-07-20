using Bogus;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoTrainings.Aggregation.Entities;

namespace MongoTrainings.Aggregation.Services;

public interface ICompanyService
{
    Task<IEnumerable<CompanyDto>> GetCompanies(CancellationToken token, int skip = 0, int take = 100);
    Task FillDataBase(int quantity, CancellationToken token);
    Task CleanCollections(CancellationToken token);
}

public class CompanyService : ICompanyService
{
    private readonly IMongoCollection<Company> _companyCollection;
    private readonly IMongoCollection<Site> _siteCollection;

    public CompanyService(IOptions<MongoConfig> options)
    {
        var mongoDatabase = new MongoClient(options.Value.GetMongoConnectionString())
            .GetDatabase(MongoConfig.MasDbName);

        _companyCollection = mongoDatabase.GetCollection<Company>(MongoConfig.CompanyCollection);
        _siteCollection = mongoDatabase.GetCollection<Site>(MongoConfig.SiteCollection);
    }

    public async Task FillDataBase(int quantity, CancellationToken token)
    {
        var fakeSites = new Faker<Site>()
            .RuleFor(site => site.SiteName, faker => faker.Company.CompanyName())
            .RuleFor(site => site.SiteAddress, faker => faker.Address.FullAddress())
            .RuleFor(site => site.Id, ObjectId.GenerateNewId)
            .Generate(quantity * quantity);

        var fakeCompanies = new Faker<Company>()
            .RuleFor(company => company.CompanyName, faker => faker.Company.CompanyName())
            .RuleFor(company => company.CompanyDescription, faker => faker.Company.Random.Words(10))
            .Generate(quantity)
            .Select((company, i) =>
            {
                company.Sites = fakeSites.Select(site => site.Id).Skip(i * quantity).Take(quantity);
                return company;
            });

        await _siteCollection.InsertManyAsync(fakeSites, new InsertManyOptions(), token);

        await _companyCollection.InsertManyAsync(fakeCompanies, new InsertManyOptions(), token);
    }

    public async Task CleanCollections(CancellationToken token) => await Task.WhenAll(
        _companyCollection.DeleteManyAsync(FilterDefinition<Company>.Empty, token),
        _siteCollection.DeleteManyAsync(FilterDefinition<Site>.Empty, token)
    );

    public async Task<IEnumerable<CompanyDto>> GetCompanies(CancellationToken token, int skip = 0, int take = 100)
    {
        var result = await _companyCollection
            .Aggregate()
            .Match(FilterDefinition<Company>.Empty)
            .Skip(skip)
            .Limit(take)
            .Lookup(
                foreignCollection: _siteCollection,
                foreignField: site => site.Id, localField: (Company company) => company.Sites,
                options: new AggregateLookupOptions<Site, CompanyDto>(),
                @as: (CompanyDto company) => company.Sites
            ).ToListAsync(token);

        return result;
    }
}