using System.ComponentModel.DataAnnotations;
using MongoDB.Driver;

namespace MongoTrainings.Aggregation
{
    public sealed class MongoConfig
    {
        private const string ExceptionMessage = "The field is required to make a connection to the MongoDB";

        public const string SectionName = "MongoSettings";
        [Required] public string BaseUrl { get; set; }
        [Required] public string UserName { get; set; }
        [Required] public string Password { get; set; }

        public const string MasDbName = "Aggregations";

        public const string CompanyCollection = "Companies";
        public const string SiteCollection = "Sites";

        public MongoUrl GetMongoConnectionString() =>
            new MongoUrlBuilder(BaseUrl ?? throw new Exception(ExceptionMessage))
            {
                Username = UserName ?? throw new Exception(ExceptionMessage),
                Password = Password ?? throw new Exception(ExceptionMessage),
            }.ToMongoUrl();
    }
}