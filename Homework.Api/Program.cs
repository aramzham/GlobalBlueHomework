using FluentValidation;
using Homework.Api.Configuration;
using Homework.Api.Middlewares;
using Homework.Api.Models;
using Homework.Api.Services;
using Homework.Api.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;

var builder = WebApplication.CreateBuilder(args);
// services
builder.Services.AddTransient<IVatService, VatService>();
// validation
builder.Services.AddValidatorsFromAssemblyContaining(typeof(Program));
// configure options
builder.Services.Configure<AppConfig>(builder.Configuration);
// swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapPost("/vat-calculator", (IVatService vatService, [FromBody] VatRequestInput request) => vatService.Calculate(request).Match(
    Results.Ok,
    f => Results.BadRequest(f.ToResponseModel())
));

app.Run();

// Make the implicit Program class public so test projects can access it
public partial class Program { }