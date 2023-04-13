using FluentValidation;
using Homework.Api.Configuration;
using Homework.Api.Models;
using Microsoft.Extensions.Options;

namespace Homework.Api.Validation;

public class VatRequestInputValidation : AbstractValidator<VatRequestInput>
{
    private readonly IOptions<AppConfig> _rates;

    public VatRequestInputValidation(IOptions<AppConfig> rates)
    {
        _rates = rates;
        
        RuleFor(x => x)
            .Cascade(CascadeMode.Stop)
            .Must(NotHaveMoreThanOneInput)
            .WithMessage("Please only one input (either gross, net or vat amount).");

        RuleFor(x => x.VatRate).Must(BeValidRate)
            .WithMessage("{PropertyName} {PropertyValue} is not applicable for Austria");
    }

    private bool BeValidRate(float arg) => Array.IndexOf(_rates.Value.AustrianVatRates, arg) > -1;

    private bool NotHaveMoreThanOneInput(VatRequestInput arg) =>
        arg.Gross.HasValue ^ arg.Net.HasValue ^ arg.Vat.HasValue;
}