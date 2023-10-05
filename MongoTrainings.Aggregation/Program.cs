using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;
using MongoTrainings.Aggregation;
using MongoTrainings.Aggregation.Entities;
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
    (int skip, int take, ICompanyService companyService, CancellationToken token) => companyService.GetCompanies(skip: skip, take:take, token: token));

app.MapGet("/full-aggregation", async (int skip, int take, ICompanyService companyService, CancellationToken token) => Results.Ok(await companyService.FullAggregation(token: token, skip: skip, take: take)));

app.MapGet("/fill-database/{quantity:int}",
    (int quantity, ICompanyService companyService, CancellationToken token) => companyService.FillDataBase(quantity, token));

app.MapGet("/clean-collections",
    (ICompanyService companyService, CancellationToken token) => companyService.CleanCollections(token));

app.Run();