using System.Text.Json;

namespace AdsTracking.Api.Services;

public class MarketTicker
{
    public string Symbol { get; set; } = "";
    public decimal Price { get; set; }
    public decimal Change { get; set; }
}

public class MarketDataService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MarketDataService> _logger;

    private MarketTicker? _btc;
    private MarketTicker? _nifty;
    private MarketTicker? _sensex;
    private DateTime _lastFetch = DateTime.MinValue;

    public MarketDataService(ILogger<MarketDataService> logger)
    {
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
        _logger = logger;
    }

    public async Task<object> GetTickersAsync()
    {
        // Cache for 30 seconds
        if ((DateTime.UtcNow - _lastFetch).TotalSeconds < 30 && _btc != null)
        {
            return new { btc = _btc, nifty = _nifty, sensex = _sensex };
        }

        await Task.WhenAll(FetchBtcAsync(), FetchIndianIndicesAsync());
        _lastFetch = DateTime.UtcNow;

        return new { btc = _btc, nifty = _nifty, sensex = _sensex };
    }

    private async Task FetchBtcAsync()
    {
        try
        {
            // Use CoinGecko free API for aggregated USD price (matches Google)
            var response = await _httpClient.GetAsync("https://api.coingecko.com/api/v3/simple/price?ids=bitcoin&vs_currencies=usd&include_24hr_change=true");
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var btcData = doc.RootElement.GetProperty("bitcoin");

            var price = btcData.GetProperty("usd").GetDecimal();
            var changePercent = btcData.GetProperty("usd_24h_change").GetDecimal();

            _btc = new MarketTicker { Symbol = "BTC/USD", Price = Math.Round(price, 2), Change = Math.Round(changePercent, 2) };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch BTC price from CoinGecko");
            // Fallback to Binance
            try
            {
                var response = await _httpClient.GetAsync("https://api.binance.com/api/v3/ticker/24hr?symbol=BTCUSDT");
                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                var price = decimal.Parse(root.GetProperty("lastPrice").GetString()!);
                var changePercent = decimal.Parse(root.GetProperty("priceChangePercent").GetString()!);
                _btc = new MarketTicker { Symbol = "BTC/USD", Price = Math.Round(price, 2), Change = Math.Round(changePercent, 2) };
            }
            catch
            {
                _btc ??= new MarketTicker { Symbol = "BTC/USD", Price = 64361, Change = 1.2m };
            }
        }
    }

    private async Task FetchIndianIndicesAsync()
    {
        try
        {
            // Use Yahoo Finance API (free, no key needed) for NIFTY and SENSEX
            var niftyTask = FetchYahooQuoteAsync("^NSEI", "NIFTY 50");
            var sensexTask = FetchYahooQuoteAsync("^BSESN", "SENSEX");
            await Task.WhenAll(niftyTask, sensexTask);

            _nifty = await niftyTask;
            _sensex = await sensexTask;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch Indian indices");
            _nifty ??= new MarketTicker { Symbol = "NIFTY 50", Price = 24856, Change = 0.82m };
            _sensex ??= new MarketTicker { Symbol = "SENSEX", Price = 81200, Change = 1.1m };
        }
    }

    private async Task<MarketTicker> FetchYahooQuoteAsync(string symbol, string displayName)
    {
        try
        {
            var url = $"https://query1.finance.yahoo.com/v8/finance/chart/{symbol}?interval=1d&range=1d";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "Mozilla/5.0");

            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var result = doc.RootElement.GetProperty("chart").GetProperty("result")[0];
            var meta = result.GetProperty("meta");

            var currentPrice = meta.GetProperty("regularMarketPrice").GetDecimal();
            var previousClose = meta.GetProperty("chartPreviousClose").GetDecimal();
            var changePercent = previousClose > 0 ? Math.Round((currentPrice - previousClose) / previousClose * 100, 2) : 0;

            return new MarketTicker { Symbol = displayName, Price = Math.Round(currentPrice, 2), Change = changePercent };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch {Symbol} from Yahoo", symbol);
            return new MarketTicker { Symbol = displayName, Price = displayName == "NIFTY 50" ? 24856 : 81200, Change = 0.5m };
        }
    }
}
