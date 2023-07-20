using MongoDB.Bson;

namespace MongoTrainings.Aggregation.Entities;

public record Site
{
    public ObjectId Id { get; set; }
    public string SiteName { get; set; }
    public string SiteAddress { get; set; }
}