using FluentValidation.Results;
using Homework.Api.Models;

namespace Homework.Api.Validation;

public class ValidationFailed
{
    private readonly IEnumerable<ValidationFailure> _errors;

    public ValidationFailed(IEnumerable<ValidationFailure> errors)
    {
        _errors = errors;
    }

    public ValidationFailed(ValidationFailure error)
    {
        _errors = new[] { error };
    }

    // TODO: think about extension methods
    public ValidationFailureResponse ToResponseModel()
    {
        var errors = _errors.Select(x => new ValidationFailureModel(x.PropertyName, new[] { x.ErrorMessage }));
        return new ValidationFailureResponse(errors);
    }
}