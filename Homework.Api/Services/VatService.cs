using System.Diagnostics;
using FluentValidation;
using Homework.Api.Configuration;
using Homework.Api.Models;
using Homework.Api.Services.Contracts;
using Homework.Api.Validation;
using Microsoft.Extensions.Options;
using OneOf;

namespace Homework.Api.Services;

public class VatService : IVatService
{
    private readonly IValidator<VatRequestInput> _vatRequestValidator;
    private readonly AppConfig _appConfig;

    public VatService(IValidator<VatRequestInput> vatRequestValidator, IOptions<AppConfig> configOptions)
    {
        _vatRequestValidator = vatRequestValidator;
        _appConfig = configOptions.Value;
    }
    
    public OneOf<VatCalculationResponse, ValidationFailed> Calculate(VatRequestInput input)
    {
        var validationResult = _vatRequestValidator.Validate(input);
        if (!validationResult.IsValid)
            return new ValidationFailed(validationResult.Errors);
        
        // calculation logic
        decimal vat, gross, net; 
        if (input.Gross.HasValue)
        {
            net = Math.Round(input.Gross.Value / (decimal)(1 + input.VatRate / 100), _appConfig.ResponseDecimalPlaces);
            vat = Math.Round(input.Gross.Value - net, _appConfig.ResponseDecimalPlaces);
            return new VatCalculationResponse(input.Gross.Value, net, vat);
        }

        if (input.Net.HasValue)
        {
            vat = Math.Round(input.Net.Value * (decimal)input.VatRate / 100, _appConfig.ResponseDecimalPlaces);
            gross = Math.Round(vat + input.Net.Value, _appConfig.ResponseDecimalPlaces);
            return new VatCalculationResponse(gross, input.Net.Value, vat);
        }

        if (input.Vat.HasValue)
        {
            net = Math.Round(input.Vat.Value / (decimal)input.VatRate * 100, _appConfig.ResponseDecimalPlaces);
            gross = Math.Round(input.Vat.Value + net, _appConfig.ResponseDecimalPlaces);
            return new VatCalculationResponse(gross, net, input.Vat.Value);
        }

        throw new UnreachableException(); // this must not happen
    }
}