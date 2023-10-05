using MongoDB.Bson;

namespace MongoTrainings.Aggregation.Entities;

public abstract record SiteBase
{
    public ObjectId Id { get; set; }
    public string SiteName { get; set; }
    public string SiteAddress { get; set; }
}

public record Site : SiteBase
{
    public IEnumerable<ObjectId> EmployeeIds { get; set; }
}

public record SiteDto : SiteBase
{
    public IEnumerable<Employee> Employees { get; set; }
}