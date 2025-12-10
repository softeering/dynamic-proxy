using Grpc.Core;

namespace DomainServiceGrpc.Services;

public class ExchangeRateService(ILogger<ExchangeRateService> logger) : ExchangeRate.ExchangeRateBase
{
	private const string DefaultFromCurrency = "USD";

	public override async Task<ExchangeRateResponse> GetRate(ExchangeRateRequest request, ServerCallContext context)
	{
		logger.LogInformation("The currency received {Currency}", request.FromCurrency);
		string fromCurrency = request.HasFromCurrency ? request.FromCurrency : DefaultFromCurrency;

		var url = "https://open.er-api.com/v6/latest/" + (fromCurrency ?? DefaultFromCurrency);
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

record ExchangeRateModel(Dictionary<string, double> Rates);
