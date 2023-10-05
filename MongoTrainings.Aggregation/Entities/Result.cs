namespace MongoTrainings.Aggregation.Entities;

public record Result(long Count, IEnumerable<CompanyDto> Companies);