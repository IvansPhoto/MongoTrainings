using MongoDB.Bson;

namespace MongoTrainings.Aggregation.Entities;

public abstract record CompanyBase
{
    public ObjectId Id { get; set; }
    public string CompanyName { get; set; }
    public string CompanyDescription { get; set; }
}

public record Company : CompanyBase
{
    public IEnumerable<ObjectId> Sites { get; set; }
}

public record CompanyDto : CompanyBase
{
    public IEnumerable<Site> Sites { get; set; }
}