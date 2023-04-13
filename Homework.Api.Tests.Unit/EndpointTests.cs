using System.Net;
using System.Text;
using System.Text.Json;
using Bogus;
using FluentAssertions;
using FluentValidation;
using Homework.Api.Configuration;
using Homework.Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using NSubstitute;

namespace Homework.Api.Tests.Unit;

public class EndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _app;
    private readonly HttpClient _httpClient;

    public EndpointTests(WebApplicationFactory<Program> app)
    {
        _app = app;
        _httpClient = _app.CreateClient();
    }

    [Fact]
    public async Task Calculate_WhenInputWithGrossReceived_ReturnsOutput()
    {
        // arrange
        var configuration = _app.Services.GetRequiredService<IOptions<AppConfig>>();

        var index = new Faker().Random.Int(0, 2);
        var vatRate = configuration.Value.AustrianVatRates[index];
        var input = new Faker<VatRequestInput>().CustomInstantiator(f =>
            new VatRequestInput(f.Random.Decimal(), null, null, vatRate)).Generate();
        var content = JsonSerializer.Serialize(input);

        // act
        var response = await _httpClient.PostAsync("/vat-calculator",
            new StringContent(content, Encoding.UTF8, "application/json"));
        var responseText = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<VatCalculationResponse>(responseText);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        result.Gross.Should().Be(input.Gross);
        result.Vat.Should()
            .Be(Math.Round(input.Gross.Value - result.Net, configuration.Value.ResponseFloatingPointRound));
        result.Net.Should().Be(Math.Round(input.Gross.Value / (decimal)(1 + input.VatRate / 100),
            configuration.Value.ResponseFloatingPointRound));
    }

    private AppConfig InitConfig()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Test.json")
            .AddEnvironmentVariables()
            .Build();
        return config.Get<AppConfig>();
    }
}