using Grpc.Core;

namespace DomainServiceGrpc.Services;

public class ExchangeRateService(ILogger<ExchangeRateService> logger) : DomainServiceGrpc.ExchangeRate.ExchangeRateBase
{
	private const string DEFAULT_FROM_CURRENCY = "USD";

	public async override Task<ExchangeRateResponse> GetExchangeRate(ExchangeRateRequest request, ServerCallContext context)
	{
		logger.LogInformation("The currency received {Currency}", request.FromCurrency);
		string fromCurrency = request.HasFromCurrency ? request.FromCurrency : DEFAULT_FROM_CURRENCY;

		var url = "https://open.er-api.com/v6/latest/" + (fromCurrency ?? DEFAULT_FROM_CURRENCY);
		using var client = new HttpClient();
		var response = await client.GetFromJsonAsync<ExchangeRateModel>(url);

		var result = new ExchangeRateResponse();
		if (response is not null)
		{
			foreach (var (key, value) in response.Rates)
			{
				result.Rates.Add(key, value);
			}
		}

		return result;
	}
}

record ExchangeRateModel(Dictionary<string, string> Rates);
