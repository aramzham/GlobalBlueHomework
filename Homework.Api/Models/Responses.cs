namespace Homework.Api.Models;

public record VatCalculationResponse(decimal Gross, decimal Net, decimal Vat);

public record ValidationFailureResponse(IEnumerable<ValidationFailureModel> Errors);

public record ValidationFailureModel(string PropertyName, string[] Messages);

public record ErrorResponse(int Status, string TraceId, string Message);