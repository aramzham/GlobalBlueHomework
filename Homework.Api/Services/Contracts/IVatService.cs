using Homework.Api.Models;
using Homework.Api.Validation;
using OneOf;

namespace Homework.Api.Services.Contracts;

public interface IVatService
{
    OneOf<VatCalculationResponse, ValidationFailed> Calculate(VatRequestInput input);
}