using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoTrainings.Aggregation.Entities;

[BsonIgnoreExtraElements] 
public record Employee
{
    public ObjectId Id { get; set; }
    public string FullName { get; set; }
}