using DomainServiceHttp;
using Grpc.Core;

namespace DomainServiceGrpc.Services;

public class ExchangeRateService(ILogger<ExchangeRateService> logger, IExchangeRateRepository exchangeRateRepository) : ExchangeRate.ExchangeRateBase
{
	private const string DefaultFromCurrency = "USD";

	public override async Task<ExchangeRateResponse> GetRate(ExchangeRateRequest request, ServerCallContext context)
	{
		logger.LogInformation("The currency received {Currency}", request.FromCurrency);
		string fromCurrency = request.HasFromCurrency ? request.FromCurrency : DefaultFromCurrency;

		var exchangeRatesResponse = await exchangeRateRepository.GetExchangeRate(fromCurrency);

		var result = new ExchangeRateResponse();

		foreach (var (key, value) in exchangeRatesResponse.Rates)
		{
			result.Rates.Add(key, (double)value);
		}

		return result;
	}
}
