using Microsoft.AspNetCore.Mvc;
using MongoTrainings.Aggregation;
using MongoTrainings.Aggregation.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOptions<MongoConfig>()
    .Bind(builder.Configuration.GetSection(MongoConfig.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddScoped<ICompanyService, CompanyService>();

var app = builder.Build();


app.MapGet("/get-companies",
    (ICompanyService companyService, CancellationToken token) => companyService.GetCompanies(token));

app.MapGet("/fill-database/{quantity:int}",
    (int quantity, ICompanyService companyService, CancellationToken token) => companyService.FillDataBase(quantity, token));

app.MapGet("/clean-collections",
    (ICompanyService companyService, CancellationToken token) => companyService.CleanCollections(token));

app.Run();