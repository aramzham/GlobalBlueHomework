namespace Homework.Api.Models;

public record VatRequestInput(decimal? Gross, decimal? Net, decimal? Vat, float VatRate);