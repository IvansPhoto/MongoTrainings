using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoTrainings.Aggregation.Entities;

public abstract record CompanyBase
{
    public ObjectId Id { get; set; }
    public string CompanyName { get; set; }
    public string CompanyDescription { get; set; }
}

public record Company : CompanyBase
{
    public ObjectId HqSiteId { get; set; } = ObjectId.Empty;
    public IEnumerable<ObjectId> SiteIds { get; set; } = ArraySegment<ObjectId>.Empty;
}

[BsonIgnoreExtraElements] 
public record CompanyDto : CompanyBase
{
    public SiteDto HqSite { get; set; }
    public IEnumerable<SiteDto> Sites { get; set; }
}