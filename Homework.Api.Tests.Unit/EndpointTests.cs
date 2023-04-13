using System.Net;
using System.Text;
using System.Text.Json;
using Bogus;
using FluentAssertions;
using Homework.Api.Configuration;
using Homework.Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
        var input = new Faker<VatRequestInput>()
            .CustomInstantiator(f => new VatRequestInput(f.Random.Decimal(), null, null, vatRate))
            .Generate();
        var content = JsonSerializer.Serialize(input);

        // act
        var response = await _httpClient.PostAsync("/vat-calculator",
            new StringContent(content, Encoding.UTF8, "application/json"));
        var responseText = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<VatCalculationResponse>(responseText, new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        });

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        result.Gross.Should().Be(input.Gross.Value);
        result.Vat.Should()
            .Be(Math.Round(input.Gross.Value - result.Net, configuration.Value.ResponseDecimalPlaces));
        result.Net.Should().Be(Math.Round(input.Gross.Value / (decimal)(1 + input.VatRate / 100),
            configuration.Value.ResponseDecimalPlaces));
    }

    [Fact]
    public async Task Calculate_WhenInputWithNetReceived_ReturnsOutput()
    {
        // arrange
        var configuration = _app.Services.GetRequiredService<IOptions<AppConfig>>();

        var index = new Faker().Random.Int(0, 2);
        var vatRate = configuration.Value.AustrianVatRates[index];
        var input = new Faker<VatRequestInput>()
            .CustomInstantiator(f => new VatRequestInput(null, f.Random.Decimal(), null, vatRate))
            .Generate();
        var content = JsonSerializer.Serialize(input);

        // act
        var response = await _httpClient.PostAsync("/vat-calculator",
            new StringContent(content, Encoding.UTF8, "application/json"));
        var responseText = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<VatCalculationResponse>(responseText, new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        });

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        result.Net.Should().Be(input.Net.Value);
        result.Vat.Should()
            .Be(Math.Round(input.Net.Value * (decimal)input.VatRate / 100, configuration.Value.ResponseDecimalPlaces));
        result.Gross.Should().Be(Math.Round(result.Vat + input.Net.Value, configuration.Value.ResponseDecimalPlaces));
    }

    [Fact]
    public async Task Calculate_WhenInputWithVatReceived_ReturnsOutput()
    {
        // arrange
        var configuration = _app.Services.GetRequiredService<IOptions<AppConfig>>();

        var index = new Faker().Random.Int(0, 2);
        var vatRate = configuration.Value.AustrianVatRates[index];
        var input = new Faker<VatRequestInput>()
            .CustomInstantiator(f => new VatRequestInput(null, null, f.Random.Decimal(), vatRate))
            .Generate();
        var content = JsonSerializer.Serialize(input);

        // act
        var response = await _httpClient.PostAsync("/vat-calculator",
            new StringContent(content, Encoding.UTF8, "application/json"));
        var responseText = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<VatCalculationResponse>(responseText, new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        });

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        result.Vat.Should().Be(input.Vat.Value);
        result.Net.Should()
            .Be(Math.Round(input.Vat.Value / (decimal)input.VatRate * 100, configuration.Value.ResponseDecimalPlaces));
        result.Gross.Should().Be(Math.Round(result.Net + input.Vat.Value, configuration.Value.ResponseDecimalPlaces));
    }

    [Fact]
    public async Task Calculate_WhenMoreThanOneInput_ReturnsError()
    {
        // arrange
        var input = new Faker<VatRequestInput>()
            .CustomInstantiator(f => new VatRequestInput(null, f.Random.Decimal(), f.Random.Decimal(), f.Random.Int()))
            .Generate();
        var content = JsonSerializer.Serialize(input);

        // act
        var response = await _httpClient.PostAsync("/vat-calculator",
            new StringContent(content, Encoding.UTF8, "application/json"));
        var responseText = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ValidationFailureResponse>(responseText, new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        });

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        result.Errors.Should().HaveCountGreaterOrEqualTo(1);
        result.Errors.First().Messages.Should().NotBeEmpty();
        result.Errors.First().Messages.Any(x => x == "Please only one input (either gross, net or vat amount).")
            .Should().BeTrue();
    }

    [Fact]
    public async Task Calculate_WhenInvalidVatRate_ReturnsError()
    {
        // arrange
        var configuration = _app.Services.GetRequiredService<IOptions<AppConfig>>();
        var input = new Faker<VatRequestInput>()
            .CustomInstantiator(f => new VatRequestInput(null, f.Random.Decimal(), f.Random.Decimal(),
                GetInvalidVatRate(f.Random, configuration.Value.AustrianVatRates)))
            .Generate();
        var content = JsonSerializer.Serialize(input);

        // act
        var response = await _httpClient.PostAsync("/vat-calculator",
            new StringContent(content, Encoding.UTF8, "application/json"));
        var responseText = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ValidationFailureResponse>(responseText, new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        });

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        result.Errors.Should().HaveCountGreaterOrEqualTo(1);
        var vatRateError = result.Errors.FirstOrDefault(x => x.PropertyName == nameof(input.VatRate));
        vatRateError.Should().NotBeNull();
        vatRateError.Messages.Should().NotBeEmpty();
        vatRateError.Messages.Any(x => x == $"Vat Rate {input.VatRate} is not applicable for Austria")
            .Should().BeTrue();
    }

    [Fact]
    public async Task Calculate_WhenSumsNotProvidedAndInvalidVatRate_ReturnsValidationFailure()
    {
        // arrange
        var configuration = _app.Services.GetRequiredService<IOptions<AppConfig>>();
        var input = new Faker<VatRequestInput>()
            .CustomInstantiator(f => new VatRequestInput(null, null, null,
                GetInvalidVatRate(f.Random, configuration.Value.AustrianVatRates)))
            .Generate();
        var content = JsonSerializer.Serialize(input);

        // act
        var response = await _httpClient.PostAsync("/vat-calculator",
            new StringContent(content, Encoding.UTF8, "application/json"));
        var responseText = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ValidationFailureResponse>(responseText, new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        });

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        result.Errors.Should().HaveCountGreaterOrEqualTo(2);
    }
    
    [Fact]
    public async Task Calculate_WhenTextEnteredInsteadOfNumber_ReturnsError()
    {
        // arrange
        var input = new
        {
            gross = "invalid",
            vatRate = 20
        };
        var content = JsonSerializer.Serialize(input);

        // act
        var response = await _httpClient.PostAsync("/vat-calculator",
            new StringContent(content, Encoding.UTF8, "application/json"));
        var responseText = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ErrorResponse>(responseText, new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        });

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        result.Status.Should().Be(500);
        result.TraceId.Should().NotBeEmpty();
        result.Message.Should().NotBeEmpty();
    }

    private float GetInvalidVatRate(Randomizer r, float[] excludedValues)
    {
        float randomFloat;

        do
        {
            randomFloat = r.Float(1, 101);
        } while (excludedValues.Contains(randomFloat));
        
        return randomFloat;
    }
}