namespace DomainServiceHttp;

public record ExchangeRateModel(Dictionary<string, decimal> Rates);

public interface IExchangeRateRepository
{
	Task<ExchangeRateModel> GetExchangeRate(string? fromCurrency = null, CancellationToken cancellationToken = default);
}

public class OnlineExchangeRateRepository : IExchangeRateRepository
{
	private const string DefaultFromCurrency = "USD";

	public async Task<ExchangeRateModel> GetExchangeRate(string? fromCurrency = null, CancellationToken cancellationToken = default)
	{
		var url = "https://open.er-api.com/v6/latest/" + (fromCurrency ?? DefaultFromCurrency);
		using var client = new HttpClient();
		var response = await client.GetFromJsonAsync<ExchangeRateModel>(url, cancellationToken: cancellationToken);
		return response!;
	}
}

public class OfflineExchangeRateRepository : IExchangeRateRepository
{
	private const string DefaultFromCurrency = "USD";

	public Task<ExchangeRateModel> GetExchangeRate(string? fromCurrency = null, CancellationToken cancellationToken = default)
	{
		var currency = fromCurrency ?? DefaultFromCurrency;
		var rates = new Dictionary<string, decimal>()
		{
			[currency] = 1.0m
		};

		rates.TryAdd("EUR", 0.85m);
		rates.TryAdd("USD", 1.15m);

		return Task.FromResult(new ExchangeRateModel(rates));
	}
}
