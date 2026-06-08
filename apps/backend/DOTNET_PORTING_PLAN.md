# Stocker - Python to .NET Porting Plan

## Mục tiêu

Port file `tradingview.py` sang .NET 8 sử dụng FastEndpoints, trả về JSON thay vì CSV.

---

## Kiến trúc Vertical Slicing

```
Stocker.Api/
├── Features/
│   └── StockRanking/
│       ├── Constants/
│       │   └── ColumnDefinitions.cs
│       ├── Models/
│       │   ├── CompanyRank.cs
│       │   ├── RankedCompany.cs
│       │   ├── TradingViewModels.cs
│       │   └── ResponseDtos.cs
│       ├── Services/
│       │   ├── TradingViewDataFetcher.cs
│       │   ├── RankingCalculator.cs
│       │   ├── MarketCapFilter.cs
│       │   └── StockRankingService.cs
│       └── RankStocksEndpoint.cs
├── Program.cs
└── appsettings.json
```

---

## 1. Constants

### `ColumnDefinitions.cs`
```csharp
namespace Stocker.Features.StockRanking.Constants;

public static class ColumnDefinitions
{
    public static readonly string[] PeColumns = 
    [
        "name", "description", "close", "change", "volume",
        "relative_volume_10d_calc", "market_cap_basic",
        "fundamental_currency_code", "price_earnings_ttm",
        "earnings_per_share_diluted_ttm",
        "earnings_per_share_diluted_yoy_growth_ttm",
        "dividends_yield_current", "sector.tr", "market", "sector"
    ];
    
    public static readonly string[] RoaColumns = 
    [
        "name", "description", "gross_margin_ttm", "operating_margin_ttm",
        "pre_tax_margin_ttm", "net_margin_ttm", "free_cash_flow_margin_ttm",
        "return_on_assets_fq", "return_on_equity_fq",
        "return_on_invested_capital_fq", "research_and_dev_ratio_ttm"
    ];
}
```

---

## 2. Models

### `CompanyRank.cs`
```csharp
namespace Stocker.Features.StockRanking.Models;

public class CompanyRank
{
    public int Rank { get; set; }
    public Dictionary<string, object> CompanyData { get; set; }
    
    public CompanyRank(int rank, Dictionary<string, object> companyData)
    {
        Rank = rank;
        CompanyData = companyData;
    }
}
```

### `RankedCompany.cs`
```csharp
namespace Stocker.Features.StockRanking.Models;

public class RankedCompany
{
    public string Name { get; set; }
    public Dictionary<string, object> Data { get; set; }
    public int CombinedRank { get; set; }
    public int PeRank { get; set; }
    public int RoaRank { get; set; }
    
    public T GetData<T>(string key)
    {
        if (Data.TryGetValue(key, out var value) && value != null)
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        return default(T);
    }
}
```

### `TradingViewModels.cs`
```csharp
using System.Text.Json.Serialization;

namespace Stocker.Features.StockRanking.Models;

public record TradingViewRequest
{
    public string[] Columns { get; init; }
    public int[] Range { get; init; }
    public string Preset { get; init; }
    public SortOption Sort { get; init; }
    public bool IgnoreUnknownFields { get; init; } = false;
    public Options Options { get; init; } = new();
}

public record SortOption(string SortBy, string SortOrder);

public record Options
{
    public string Lang { get; init; } = "en";
}

public record TradingViewResponse
{
    [JsonPropertyName("totalCount")]
    public int TotalCount { get; init; }
    
    [JsonPropertyName("data")]
    public StockDataPoint[] Data { get; init; }
}

public record StockDataPoint
{
    [JsonPropertyName("d")]
    public object[] Data { get; init; }
    
    [JsonPropertyName("s")]
    public string StockIdentifier { get; init; }
}
```

### `ResponseDtos.cs`
```csharp
namespace Stocker.Features.StockRanking.Models;

public class RankingResponse
{
    public int TotalRanked { get; set; }
    public int TotalMissingCap { get; set; }
    public List<StockDto> RankedStocks { get; set; } = new();
    public List<StockDto> MissingCapStocks { get; set; } = new();
}

public class StockDto
{
    public string Name { get; set; }
    public int CombinedRank { get; set; }
    public int PeRank { get; set; }
    public int RoaRank { get; set; }
    
    // Market data
    public long? MarketCap { get; set; }
    public decimal? Price { get; set; }
    public decimal? Change { get; set; }
    public long? Volume { get; set; }
    
    // Valuation metrics
    public decimal? PeRatio { get; set; }
    public decimal? Eps { get; set; }
    public decimal? Roa { get; set; }
    public decimal? DividendsYield { get; set; }
    
    // Company info
    public string? Description { get; set; }
    public string? Sector { get; set; }
    public string? Market { get; set; }
    public string? StockExchange { get; set; }
}
```

---

## 3. Services

### `TradingViewDataFetcher.cs`
```csharp
using System.Text.Json;
using Stocker.Features.StockRanking.Constants;
using Stocker.Features.StockRanking.Models;

namespace Stocker.Features.StockRanking.Services;

public class TradingViewDataFetcher
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://scanner.tradingview.com/vietnam/scan";

    public TradingViewDataFetcher(HttpClient httpClient)
    {
        _httpClient = httpClient;
        SetupHeaders();
    }

    public async Task<(TradingViewResponse PeData, TradingViewResponse RoaData)> FetchAllStockDataAsync(CancellationToken ct)
    {
        var totalCount = await GetTotalCountAsync(ct);
        
        var peTask = GetStocksByPeAsync(totalCount, ct);
        var roaTask = GetStocksByRoaAsync(totalCount, ct);
        
        await Task.WhenAll(peTask, roaTask);
        
        return (await peTask, await roaTask);
    }

    private async Task<int> GetTotalCountAsync(CancellationToken ct)
    {
        var request = new TradingViewRequest
        {
            Columns = Array.Empty<string>(),
            Range = new[] { 0, 1 },
            Preset = "all_stocks"
        };

        var response = await PostAsync(request, ct);
        return response.TotalCount;
    }

    private async Task<TradingViewResponse> GetStocksByPeAsync(int maxStocks, CancellationToken ct)
    {
        var request = new TradingViewRequest
        {
            Columns = ColumnDefinitions.PeColumns,
            Range = new[] { 0, maxStocks },
            Sort = new SortOption("price_earnings_ttm", "asc"),
            Preset = "all_stocks"
        };

        return await PostAsync(request, ct);
    }

    private async Task<TradingViewResponse> GetStocksByRoaAsync(int maxStocks, CancellationToken ct)
    {
        var request = new TradingViewRequest
        {
            Columns = ColumnDefinitions.RoaColumns,
            Range = new[] { 0, maxStocks },
            Sort = new SortOption("return_on_assets_fq", "desc"),
            Preset = "all_stocks"
        };

        return await PostAsync(request, ct);
    }

    private async Task<TradingViewResponse> PostAsync(TradingViewRequest request, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{BaseUrl}?label-product=markets-screener", content, ct);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<TradingViewResponse>(responseBody);
    }

    private void SetupHeaders()
    {
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("accept", "application/json");
        _httpClient.DefaultRequestHeaders.Add("content-type", "application/json");
    }
}
```

### `RankingCalculator.cs`
```csharp
using Stocker.Features.StockRanking.Constants;
using Stocker.Features.StockRanking.Models;

namespace Stocker.Features.StockRanking.Services;

public class RankingCalculator
{
    public Dictionary<string, RankedCompany> CalculateRankings(
        TradingViewResponse peData,
        TradingViewResponse roaData)
    {
        var peRank = ExtractRank(peData, ColumnDefinitions.PeColumns);
        var roaRank = ExtractRank(roaData, ColumnDefinitions.RoaColumns);

        return CombineRankings(peRank, roaRank);
    }

    private Dictionary<string, CompanyRank> ExtractRank(TradingViewResponse response, string[] columns)
    {
        var ranking = new Dictionary<string, CompanyRank>();

        if (response?.Data == null)
            return ranking;

        for (int i = 0; i < response.Data.Length; i++)
        {
            var dataPoint = response.Data[i];
            var recordData = dataPoint.Data;

            var companyData = new Dictionary<string, object>
            {
                ["stock_exchange"] = dataPoint.StockIdentifier.Split(':')[0]
            };

            for (int j = 0; j < columns.Length; j++)
            {
                companyData[columns[j]] = recordData[j];
            }

            var name = companyData["name"].ToString();
            ranking[name] = new CompanyRank(i, companyData);
        }

        return ranking;
    }

    private Dictionary<string, RankedCompany> CombineRankings(
        Dictionary<string, CompanyRank> peRank,
        Dictionary<string, CompanyRank> roaRank)
    {
        var finalRank = new Dictionary<string, RankedCompany>();

        foreach (var companyName in peRank.Keys)
        {
            if (!roaRank.ContainsKey(companyName))
                continue;

            var peCompany = peRank[companyName];
            var roaCompany = roaRank[companyName];

            var combinedRank = peCompany.Rank + roaCompany.Rank;

            var companyData = new Dictionary<string, object>();

            foreach (var kvp in peCompany.CompanyData)
                companyData[kvp.Key] = kvp.Value;

            foreach (var kvp in roaCompany.CompanyData)
                companyData[kvp.Key] = kvp.Value;

            companyData["combined_rank"] = combinedRank;
            companyData["pe_rank"] = peCompany.Rank;
            companyData["roa_rank"] = roaCompany.Rank;

            finalRank[companyName] = new RankedCompany
            {
                Name = companyName,
                Data = companyData,
                CombinedRank = combinedRank,
                PeRank = peCompany.Rank,
                RoaRank = roaCompany.Rank
            };
        }

        return finalRank;
    }
}
```

### `MarketCapFilter.cs`
```csharp
using Stocker.Features.StockRanking.Models;

namespace Stocker.Features.StockRanking.Services;

public class MarketCapFilter
{
    public (List<RankedCompany> Valid, List<RankedCompany> MissingCap) FilterByMarketCap(
        Dictionary<string, RankedCompany> rankedCompanies)
    {
        var valid = new List<RankedCompany>();
        var missingCap = new List<RankedCompany>();

        foreach (var kvp in rankedCompanies.OrderBy(x => x.Value.CombinedRank))
        {
            var company = kvp.Value;
            var marketCap = company.Data.GetValueOrDefault("market_cap_basic");

            if (marketCap == null)
            {
                missingCap.Add(company);
            }
            else
            {
                valid.Add(company);
            }
        }

        return (valid, missingCap);
    }
}
```

### `StockRankingService.cs`
```csharp
using Stocker.Features.StockRanking.Models;

namespace Stocker.Features.StockRanking.Services;

public class StockRankingService
{
    private readonly TradingViewDataFetcher _dataFetcher;
    private readonly RankingCalculator _calculator;
    private readonly MarketCapFilter _filter;

    public StockRankingService(
        TradingViewDataFetcher dataFetcher,
        RankingCalculator calculator,
        MarketCapFilter filter)
    {
        _dataFetcher = dataFetcher;
        _calculator = calculator;
        _filter = filter;
    }

    public async Task<RankingResult> RankStocksAsync(CancellationToken ct = default)
    {
        // 1. Fetch data from TradingView
        var (peData, roaData) = await _dataFetcher.FetchAllStockDataAsync(ct);
        
        // 2. Calculate rankings
        var rankedCompanies = _calculator.CalculateRankings(peData, roaData);
        
        // 3. Filter by market cap
        var (valid, missingCap) = _filter.FilterByMarketCap(rankedCompanies);

        return new RankingResult
        {
            TotalRanked = valid.Count,
            TotalMissingCap = missingCap.Count,
            RankedStocks = valid,
            MissingCapStocks = missingCap
        };
    }
}

public class RankingResult
{
    public int TotalRanked { get; set; }
    public int TotalMissingCap { get; set; }
    public List<RankedCompany> RankedStocks { get; set; } = new();
    public List<RankedCompany> MissingCapStocks { get; set; } = new();
}
```

---

## 4. Endpoints

### `RankStocksEndpoint.cs`
```csharp
using FastEndpoints;
using Stocker.Features.StockRanking.Models;
using Stocker.Features.StockRanking.Services;

namespace Stocker.Features.StockRanking.Endpoints;

public class RankStocksEndpoint : EndpointWithoutRequest<RankingResponse>
{
    private readonly StockRankingService _stockRankingService;

    public RankStocksEndpoint(StockRankingService stockRankingService)
    {
        _stockRankingService = stockRankingService;
    }

    public override void Configure()
    {
        Get("/api/stocks/ranking");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _stockRankingService.RankStocksAsync(ct);

        await SendAsync(new RankingResponse
        {
            TotalRanked = result.TotalRanked,
            TotalMissingCap = result.TotalMissingCap,
            RankedStocks = result.RankedStocks.Select(s => MapToDto(s)).ToList(),
            MissingCapStocks = result.MissingCapStocks.Select(s => MapToDto(s)).ToList()
        });
    }

    private static StockDto MapToDto(RankedCompany s)
    {
        return new StockDto
        {
            Name = s.Name,
            CombinedRank = s.CombinedRank,
            PeRank = s.PeRank,
            RoaRank = s.RoaRank,
            MarketCap = s.GetData<long?>("market_cap_basic"),
            Price = s.GetData<decimal?>("close"),
            Change = s.GetData<decimal?>("change"),
            Volume = s.GetData<long?>("volume"),
            PeRatio = s.GetData<decimal?>("price_earnings_ttm"),
            Roa = s.GetData<decimal?>("return_on_assets_fq"),
            Eps = s.GetData<decimal?>("earnings_per_share_diluted_ttm"),
            DividendsYield = s.GetData<decimal?>("dividends_yield_current"),
            Description = s.GetData<string>("description"),
            Sector = s.GetData<string>("sector"),
            Market = s.GetData<string>("market"),
            StockExchange = s.GetData<string>("stock_exchange")
        };
    }
}
```

---

## 5. Program.cs

```csharp
using FastEndpoints;
using FastEndpoints.Swagger;
using Stocker.Features.StockRanking.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFastEndpoints();
builder.Services.AddSwaggerDoc();

// Register feature services
builder.Services.AddHttpClient<TradingViewDataFetcher>();
builder.Services.AddScoped<RankingCalculator>();
builder.Services.AddScoped<MarketCapFilter>();
builder.Services.AddScoped<StockRankingService>();

var app = builder.Build();

app.UseFastEndpoints();
app.UseSwaggerGen();

app.Run();
```

---

## API Response Example

```
GET /api/stocks/ranking
```

```json
{
  "totalRanked": 42,
  "totalMissingCap": 8,
  "rankedStocks": [
    {
      "name": "VCB",
      "combinedRank": 10,
      "peRank": 5,
      "roaRank": 5,
      "marketCap": 1500000000000,
      "price": 92.5,
      "change": 1.2,
      "volume": 1500000,
      "peRatio": 8.5,
      "eps": 10.2,
      "roa": 1.8,
      "dividendsYield": 5.2,
      "description": "Vietnam Joint Stock Commercial Bank for Industry and Trade",
      "sector": "Finance",
      "market": "HOSE",
      "stockExchange": "HOSE"
    }
  ],
  "missingCapStocks": [...]
}
```

---

## Required NuGet Packages

```xml
<PackageReference Include="FastEndpoints" Version="5.3.0" />
<PackageReference Include="FastEndpoints.Swagger" Version="5.3.0" />
```

---

## Implementation Steps

1. **Tạo dự án**: `dotnet new webapi -n Stocker.Api`
2. **Cài đặt packages**: FastEndpoints & Swagger
3. **Tạo thư mục**: `Features/StockRanking/...`
4. **Tạo từng file** theo thứ tự:
   - Constants → Models → Services → Endpoints
5. **Cập nhật Program.cs**
6. **Chạy và test với Swagger**

**Thời gian dự kiến**: 8-12 tiếng
