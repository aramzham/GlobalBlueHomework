using Bogus;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Homework.Api.Configuration;
using Homework.Api.Models;
using Homework.Api.Services;
using Homework.Api.Services.Contracts;
using Microsoft.Extensions.Options;
using NSubstitute;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace Homework.Api.Tests.Unit.Services;

public class VatServiceTests
{
    private const decimal Precision = 0.09m;
    
    private readonly IVatService _sut;
    private readonly IValidator<VatRequestInput> _requestValidatorSubstitute;
    private readonly IOptions<AppConfig> _configSubstitute;
    private readonly Faker _faker;
    private readonly int _decimalPlaces;

    public VatServiceTests()
    {
        _faker = new Faker();
        _requestValidatorSubstitute = Substitute.For<IValidator<VatRequestInput>>();

        _decimalPlaces = 2; // taken the same as in VatServiceTestsData test data
        _configSubstitute = Substitute.For<IOptions<AppConfig>>();
        var appConfig = new Faker<AppConfig>()
            .RuleFor(x => x.ResponseDecimalPlaces, _decimalPlaces)
            .Generate();
        _configSubstitute.Value.Returns(appConfig);
        
        _sut = new VatService(_requestValidatorSubstitute, _configSubstitute);
    }

    [Fact]
    public void Calculate_WhenInputNotValid_ReturnsValidationFailed()
    {
        // arrange
        var propName = _faker.Random.String();
        var message = _faker.Random.String();

        _requestValidatorSubstitute.Validate(Arg.Any<VatRequestInput>()).Returns(
            new ValidationResult(new List<ValidationFailure>() { new(propName, message) }));

        // act
        var result = _sut.Calculate(default);

        // assert
        result.IsT0.Should().BeFalse();
        result.IsT1.Should().BeTrue();
    }

    [Fact]
    public void Calculate_WhenGrossAmountGiven_ReturnsResponseWithNetAndVat()
    {
        // arrange
        var input = new Faker<VatRequestInput>()
            .CustomInstantiator(f => new VatRequestInput(f.Random.Decimal(), null, null, f.Random.Float()))
            .Generate();
        
        _requestValidatorSubstitute.Validate(Arg.Any<VatRequestInput>()).Returns(new ValidationResult());

        // act
        var result = _sut.Calculate(input);

        // assert
        result.IsT0.Should().BeTrue();
        result.IsT1.Should().BeFalse();
        result.AsT0.Gross.Should().Be(input.Gross.Value);
        result.AsT0.Gross.Should().BeApproximately(result.AsT0.Net + result.AsT0.Vat, Precision);
        GetDecimalPlaces(result.AsT0.Net).Should().Be(_decimalPlaces);
    }
    
    [Fact]
    public void Calculate_WhenNetAmountGiven_ReturnsResponseWithGrossAndVat()
    {
        // arrange
        var input = new Faker<VatRequestInput>()
            .CustomInstantiator(f => new VatRequestInput(null, f.Random.Decimal(), null, f.Random.Float()))
            .Generate();
        
        _requestValidatorSubstitute.Validate(Arg.Any<VatRequestInput>()).Returns(new ValidationResult());

        // act
        var result = _sut.Calculate(input);

        // assert
        result.IsT0.Should().BeTrue();
        result.IsT1.Should().BeFalse();
        result.AsT0.Net.Should().Be(input.Net.Value);
        result.AsT0.Gross.Should().BeApproximately(result.AsT0.Net + result.AsT0.Vat, Precision);
        GetDecimalPlaces(result.AsT0.Gross).Should().Be(_decimalPlaces);
    }
    
    [Fact]
    public void Calculate_WhenVatAmountGiven_ReturnsResponseWithNetAndGross()
    {
        // arrange
        var input = new Faker<VatRequestInput>()
            .CustomInstantiator(f => new VatRequestInput(null, null, f.Random.Decimal(), f.Random.Float()))
            .Generate();
        
        _requestValidatorSubstitute.Validate(Arg.Any<VatRequestInput>()).Returns(new ValidationResult());

        // act
        var result = _sut.Calculate(input);

        // assert
        result.IsT0.Should().BeTrue();
        result.IsT1.Should().BeFalse();
        result.AsT0.Vat.Should().Be(input.Vat.Value);
        result.AsT0.Gross.Should().BeApproximately(result.AsT0.Net + result.AsT0.Vat, Precision);
        GetDecimalPlaces(result.AsT0.Net).Should().Be(_decimalPlaces);
    }
    
    [Theory]
    [ClassData(typeof(VatServiceTestsData))]
    public void Calculate_WhenInputIsCorrect_ReturnsCorrectData(VatRequestInput input, VatCalculationResponse output)
    {
        // arrange
        _requestValidatorSubstitute.Validate(Arg.Any<VatRequestInput>()).Returns(new ValidationResult());
        
        // act
        var result = _sut.Calculate(input);
        
        // assert
        result.AsT0.Gross.Should().Be(output.Gross);
        result.AsT0.Net.Should().Be(output.Net);
        result.AsT0.Vat.Should().Be(output.Vat);
    }

    private int GetDecimalPlaces(decimal myDecimal)
    {
        var bits = decimal.GetBits(myDecimal);
        var decimalPlaces = (bits[3] >> 16) & 0x000000FF;
        return decimalPlaces;
    }
}