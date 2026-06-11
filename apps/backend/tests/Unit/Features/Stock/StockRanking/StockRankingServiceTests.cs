using NSubstitute;
using Stocker.Features.Stock.StockRanking;
using Stocker.Tests.Helpers.TestDataBuilders;
using Xunit;

namespace Stocker.Tests.Unit.Features.Stock.StockRanking;

public class StockRankingServiceTests
{
  private readonly ITradingViewClient _mockTradingViewClient;
  private readonly RankingCalculator _calculator;
  private readonly StockRankingService _service;

  public StockRankingServiceTests()
  {
    _mockTradingViewClient = Substitute.For<ITradingViewClient>();
    _calculator = new RankingCalculator();
    _service = new StockRankingService(_mockTradingViewClient, _calculator);
  }

  #region Service Orchestration Tests

  [Fact]
  public async Task RankStocksAsync_FetchesDataFromTradingView()
  {
    // Arrange
    var request = RankingRequestBuilder.CreateDefault().Build();
    var peData = TradingViewResponseBuilder.CreatePeResponse()
      .AddStock("VCB", "Vietcombank", 50.5m, 1.2m, 1000000L, 1.5m, 5000000000000m, "VND", 5.0m, 10000m, 10.5m, 2.5m, "Finance", "HOSE", "Banks")
      .Build();
    var roicData = TradingViewResponseBuilder.CreateRoicResponse()
      .AddStock("VCB", "Vietcombank", 45.0m, 35.0m, 30.0m, 25.0m, 20.0m, 1.5m, 12.0m, 15.0m, 2.0m)
      .Build();

    _mockTradingViewClient.FetchAllStockDataAsync(Arg.Any<CancellationToken>())
      .Returns((peData, roicData));

    // Act
    await _service.RankStocksAsync(request);

    // Assert
    await _mockTradingViewClient.Received(1).FetchAllStockDataAsync(Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task RankStocksAsync_ValidRequest_ReturnsCorrectRankings()
  {
    // Arrange
    var request = RankingRequestBuilder.CreateDefault()
      .WithNumberOfStocks(10)
      .Build();

    var peData = TradingViewResponseBuilder.CreatePeResponse()
      .AddStock("VCB", "Vietcombank", 50.5m, 1.2m, 1000000L, 1.5m, 5000000000000m, "VND", 5.0m, 10000m, 10.5m, 2.5m, "Finance", "HOSE", "Banks")
      .AddStock("VIC", "Vingroup", 25.0m, -0.5m, 2000000L, 1.2m, 3000000000000m, "VND", 8.0m, 8000m, 15.0m, 3.0m, "Real Estate", "HOSE", "Real Estate")
      .Build();

    var roicData = TradingViewResponseBuilder.CreateRoicResponse()
      .AddStock("VCB", "Vietcombank", 45.0m, 35.0m, 30.0m, 25.0m, 20.0m, 1.5m, 12.0m, 15.0m, 2.0m)
      .AddStock("VIC", "Vingroup", 40.0m, 30.0m, 25.0m, 20.0m, 18.0m, 1.2m, 10.0m, 12.0m, 1.5m)
      .Build();

    _mockTradingViewClient.FetchAllStockDataAsync(Arg.Any<CancellationToken>())
      .Returns((peData, roicData));

    // Act
    var result = await _service.RankStocksAsync(request);

    // Assert
    Assert.Equal(2, result.TotalRanked);
    Assert.Equal(0, result.TotalMissingCap);
    Assert.Equal(2, result.RankedStocks.Count);
    Assert.Equal(0, result.RankedStocks[0].CombinedRank);
    Assert.Equal("VCB", result.RankedStocks[0].Name);
  }

  [Fact]
  public async Task RankStocksAsync_PropagatesCancellationToken()
  {
    // Arrange
    var request = RankingRequestBuilder.CreateDefault().Build();
    var cts = new CancellationTokenSource();
    var token = cts.Token;

    var peData = TradingViewResponseBuilder.CreatePeResponse()
      .AddStock("VCB", "Vietcombank", 50.5m, 1.2m, 1000000L, 1.5m, 5000000000000m, "VND", 5.0m, 10000m, 10.5m, 2.5m, "Finance", "HOSE", "Banks")
      .Build();
    var roicData = TradingViewResponseBuilder.CreateRoicResponse()
      .AddStock("VCB", "Vietcombank", 45.0m, 35.0m, 30.0m, 25.0m, 20.0m, 1.5m, 12.0m, 15.0m, 2.0m)
      .Build();

    _mockTradingViewClient.FetchAllStockDataAsync(token)
      .Returns((peData, roicData));

    // Act
    await _service.RankStocksAsync(request, token);

    // Assert
    await _mockTradingViewClient.Received(1).FetchAllStockDataAsync(token);
  }

  #endregion

  #region Market Cap Filtering Tests

  [Fact]
  public async Task RankStocksAsync_WithMinimumMarketCap_FiltersCorrectly()
  {
    // Arrange
    var request = RankingRequestBuilder.CreateDefault()
      .WithMinimumMarketCap(4000000000000m) // 4 trillion
      .Build();

    var peData = TradingViewResponseBuilder.CreatePeResponse()
      .AddStock("VCB", "Vietcombank", 50.5m, 1.2m, 1000000L, 1.5m, 5000000000000m, "VND", 5.0m, 10000m, 10.5m, 2.5m, "Finance", "HOSE", "Banks")
      .AddStock("VIC", "Vingroup", 25.0m, -0.5m, 2000000L, 1.2m, 3000000000000m, "VND", 8.0m, 8000m, 15.0m, 3.0m, "Real Estate", "HOSE", "Real Estate")
      .Build();

    var roicData = TradingViewResponseBuilder.CreateRoicResponse()
      .AddStock("VCB", "Vietcombank", 45.0m, 35.0m, 30.0m, 25.0m, 20.0m, 1.5m, 12.0m, 15.0m, 2.0m)
      .AddStock("VIC", "Vingroup", 40.0m, 30.0m, 25.0m, 20.0m, 18.0m, 1.2m, 10.0m, 12.0m, 1.5m)
      .Build();

    _mockTradingViewClient.FetchAllStockDataAsync(Arg.Any<CancellationToken>())
      .Returns((peData, roicData));

    // Act
    var result = await _service.RankStocksAsync(request);

    // Assert
    Assert.Equal(1, result.TotalRanked);
    Assert.Equal(0, result.TotalMissingCap);
    Assert.Single(result.RankedStocks);
    Assert.Equal("VCB", result.RankedStocks[0].Name);
  }

  [Fact]
  public async Task RankStocksAsync_WithNullMinimumMarketCap_IncludesAll()
  {
    // Arrange
    var request = RankingRequestBuilder.CreateDefault()
      .WithMinimumMarketCap(null)
      .Build();

    var peData = TradingViewResponseBuilder.CreatePeResponse()
      .AddStock("VCB", "Vietcombank", 50.5m, 1.2m, 1000000L, 1.5m, 5000000000000m, "VND", 5.0m, 10000m, 10.5m, 2.5m, "Finance", "HOSE", "Banks")
      .AddStock("VIC", "Vingroup", 25.0m, -0.5m, 2000000L, 1.2m, 3000000000000m, "VND", 8.0m, 8000m, 15.0m, 3.0m, "Real Estate", "HOSE", "Real Estate")
      .Build();

    var roicData = TradingViewResponseBuilder.CreateRoicResponse()
      .AddStock("VCB", "Vietcombank", 45.0m, 35.0m, 30.0m, 25.0m, 20.0m, 1.5m, 12.0m, 15.0m, 2.0m)
      .AddStock("VIC", "Vingroup", 40.0m, 30.0m, 25.0m, 20.0m, 18.0m, 1.2m, 10.0m, 12.0m, 1.5m)
      .Build();

    _mockTradingViewClient.FetchAllStockDataAsync(Arg.Any<CancellationToken>())
      .Returns((peData, roicData));

    // Act
    var result = await _service.RankStocksAsync(request);

    // Assert
    Assert.Equal(2, result.TotalRanked);
    Assert.Equal(0, result.TotalMissingCap);
    Assert.Equal(2, result.RankedStocks.Count);
  }

  [Fact]
  public async Task RankStocksAsync_SeparatesMissingCapStocks()
  {
    // Arrange
    var request = RankingRequestBuilder.CreateDefault().Build();

    var peData = TradingViewResponseBuilder.CreatePeResponse()
      .AddStock("VCB", "Vietcombank", 50.5m, 1.2m, 1000000L, 1.5m, 5000000000000m, "VND", 5.0m, 10000m, 10.5m, 2.5m, "Finance", "HOSE", "Banks")
      .AddStock("VIC", "Vingroup", 25.0m, -0.5m, 2000000L, 1.2m, null, "VND", 8.0m, 8000m, 15.0m, 3.0m, "Real Estate", "HOSE", "Real Estate")
      .Build();

    var roicData = TradingViewResponseBuilder.CreateRoicResponse()
      .AddStock("VCB", "Vietcombank", 45.0m, 35.0m, 30.0m, 25.0m, 20.0m, 1.5m, 12.0m, 15.0m, 2.0m)
      .AddStock("VIC", "Vingroup", 40.0m, 30.0m, 25.0m, 20.0m, 18.0m, 1.2m, 10.0m, 12.0m, 1.5m)
      .Build();

    _mockTradingViewClient.FetchAllStockDataAsync(Arg.Any<CancellationToken>())
      .Returns((peData, roicData));

    // Act
    var result = await _service.RankStocksAsync(request);

    // Assert
    Assert.Equal(1, result.TotalRanked);
    Assert.Equal(1, result.TotalMissingCap);
    Assert.Single(result.RankedStocks);
    Assert.Single(result.MissingCapStocks);
    Assert.Equal("VIC", result.MissingCapStocks[0].Name);
  }

  [Fact]
  public async Task RankStocksAsync_BelowMinimumCap_ExcludedFromResults()
  {
    // Arrange
    var request = RankingRequestBuilder.CreateDefault()
      .WithMinimumMarketCap(4000000000000m) // 4 trillion
      .Build();

    var peData = TradingViewResponseBuilder.CreatePeResponse()
      .AddStock("VCB", "Vietcombank", 50.5m, 1.2m, 1000000L, 1.5m, 5000000000000m, "VND", 5.0m, 10000m, 10.5m, 2.5m, "Finance", "HOSE", "Banks")
      .AddStock("VIC", "Vingroup", 25.0m, -0.5m, 2000000L, 1.2m, 3000000000000m, "VND", 8.0m, 8000m, 15.0m, 3.0m, "Real Estate", "HOSE", "Real Estate")
      .Build();

    var roicData = TradingViewResponseBuilder.CreateRoicResponse()
      .AddStock("VCB", "Vietcombank", 45.0m, 35.0m, 30.0m, 25.0m, 20.0m, 1.5m, 12.0m, 15.0m, 2.0m)
      .AddStock("VIC", "Vingroup", 40.0m, 30.0m, 25.0m, 20.0m, 18.0m, 1.2m, 10.0m, 12.0m, 1.5m)
      .Build();

    _mockTradingViewClient.FetchAllStockDataAsync(Arg.Any<CancellationToken>())
      .Returns((peData, roicData));

    // Act
    var result = await _service.RankStocksAsync(request);

    // Assert
    Assert.Equal(1, result.TotalRanked);
    Assert.Single(result.RankedStocks);
    Assert.Equal("VCB", result.RankedStocks[0].Name);
    Assert.True(result.RankedStocks.All(s => s.Data.MarketCapBasic >= 4000000000000m));
  }

  #endregion

  #region Result Limiting Tests

  [Fact]
  public async Task RankStocksAsync_TakesCorrectNumberOfStocks()
  {
    // Arrange
    var request = RankingRequestBuilder.CreateDefault()
      .WithNumberOfStocks(2)
      .Build();

    var peData = TradingViewResponseBuilder.CreatePeResponse()
      .AddStock("VCB", "Vietcombank", 50.5m, 1.2m, 1000000L, 1.5m, 5000000000000m, "VND", 5.0m, 10000m, 10.5m, 2.5m, "Finance", "HOSE", "Banks")
      .AddStock("VIC", "Vingroup", 25.0m, -0.5m, 2000000L, 1.2m, 3000000000000m, "VND", 8.0m, 8000m, 15.0m, 3.0m, "Real Estate", "HOSE", "Real Estate")
      .AddStock("HPG", "Hoa Phat", 30.0m, 0.5m, 1500000L, 1.3m, 2000000000000m, "VND", 7.0m, 9000m, 12.0m, 2.0m, "Steel", "HOSE", "Steel")
      .Build();

    var roicData = TradingViewResponseBuilder.CreateRoicResponse()
      .AddStock("VCB", "Vietcombank", 45.0m, 35.0m, 30.0m, 25.0m, 20.0m, 1.5m, 12.0m, 15.0m, 2.0m)
      .AddStock("VIC", "Vingroup", 40.0m, 30.0m, 25.0m, 20.0m, 18.0m, 1.2m, 10.0m, 12.0m, 1.5m)
      .AddStock("HPG", "Hoa Phat", 35.0m, 25.0m, 20.0m, 15.0m, 16.0m, 1.0m, 8.0m, 10.0m, 1.2m)
      .Build();

    _mockTradingViewClient.FetchAllStockDataAsync(Arg.Any<CancellationToken>())
      .Returns((peData, roicData));

    // Act
    var result = await _service.RankStocksAsync(request);

    // Assert
    Assert.Equal(2, result.TotalRanked);
    Assert.Equal(2, result.RankedStocks.Count);
  }

  [Fact]
  public async Task RankStocksAsync_SortsByCombinedRank()
  {
    // Arrange
    var request = RankingRequestBuilder.CreateDefault()
      .WithNumberOfStocks(3)
      .Build();

    var peData = TradingViewResponseBuilder.CreatePeResponse()
      .AddStock("VCB", "Vietcombank", 50.5m, 1.2m, 1000000L, 1.5m, 5000000000000m, "VND", 5.0m, 10000m, 10.5m, 2.5m, "Finance", "HOSE", "Banks")
      .AddStock("VIC", "Vingroup", 25.0m, -0.5m, 2000000L, 1.2m, 3000000000000m, "VND", 8.0m, 8000m, 15.0m, 3.0m, "Real Estate", "HOSE", "Real Estate")
      .AddStock("HPG", "Hoa Phat", 30.0m, 0.5m, 1500000L, 1.3m, 2000000000000m, "VND", 7.0m, 9000m, 12.0m, 2.0m, "Steel", "HOSE", "Steel")
      .Build();

    var roicData = TradingViewResponseBuilder.CreateRoicResponse()
      .AddStock("VCB", "Vietcombank", 45.0m, 35.0m, 30.0m, 25.0m, 20.0m, 1.5m, 12.0m, 15.0m, 2.0m)
      .AddStock("VIC", "Vingroup", 40.0m, 30.0m, 25.0m, 20.0m, 18.0m, 1.2m, 10.0m, 12.0m, 1.5m)
      .AddStock("HPG", "Hoa Phat", 35.0m, 25.0m, 20.0m, 15.0m, 16.0m, 1.0m, 8.0m, 10.0m, 1.2m)
      .Build();

    _mockTradingViewClient.FetchAllStockDataAsync(Arg.Any<CancellationToken>())
      .Returns((peData, roicData));

    // Act
    var result = await _service.RankStocksAsync(request);

    // Assert
    Assert.Equal(3, result.RankedStocks.Count);
    // Verify sorted by combined rank (ascending)
    for (int i = 1; i < result.RankedStocks.Count; i++)
    {
      Assert.True(result.RankedStocks[i].CombinedRank >= result.RankedStocks[i - 1].CombinedRank);
    }
  }

  [Fact]
  public async Task RankStocksAsync_RequestMoreThanAvailable_ReturnsAll()
  {
    // Arrange
    var request = RankingRequestBuilder.CreateDefault()
      .WithNumberOfStocks(10)
      .Build();

    var peData = TradingViewResponseBuilder.CreatePeResponse()
      .AddStock("VCB", "Vietcombank", 50.5m, 1.2m, 1000000L, 1.5m, 5000000000000m, "VND", 5.0m, 10000m, 10.5m, 2.5m, "Finance", "HOSE", "Banks")
      .AddStock("VIC", "Vingroup", 25.0m, -0.5m, 2000000L, 1.2m, 3000000000000m, "VND", 8.0m, 8000m, 15.0m, 3.0m, "Real Estate", "HOSE", "Real Estate")
      .Build();

    var roicData = TradingViewResponseBuilder.CreateRoicResponse()
      .AddStock("VCB", "Vietcombank", 45.0m, 35.0m, 30.0m, 25.0m, 20.0m, 1.5m, 12.0m, 15.0m, 2.0m)
      .AddStock("VIC", "Vingroup", 40.0m, 30.0m, 25.0m, 20.0m, 18.0m, 1.2m, 10.0m, 12.0m, 1.5m)
      .Build();

    _mockTradingViewClient.FetchAllStockDataAsync(Arg.Any<CancellationToken>())
      .Returns((peData, roicData));

    // Act
    var result = await _service.RankStocksAsync(request);

    // Assert
    Assert.Equal(2, result.TotalRanked);
    Assert.Equal(2, result.RankedStocks.Count);
  }

  [Fact]
  public async Task RankStocksAsync_PreservesRankOrderInResults()
  {
    // Arrange
    var request = RankingRequestBuilder.CreateDefault()
      .WithNumberOfStocks(2)
      .Build();

    var peData = TradingViewResponseBuilder.CreatePeResponse()
      .AddStock("VCB", "Vietcombank", 50.5m, 1.2m, 1000000L, 1.5m, 5000000000000m, "VND", 5.0m, 10000m, 10.5m, 2.5m, "Finance", "HOSE", "Banks")
      .AddStock("VIC", "Vingroup", 25.0m, -0.5m, 2000000L, 1.2m, 3000000000000m, "VND", 8.0m, 8000m, 15.0m, 3.0m, "Real Estate", "HOSE", "Real Estate")
      .Build();

    var roicData = TradingViewResponseBuilder.CreateRoicResponse()
      .AddStock("VCB", "Vietcombank", 45.0m, 35.0m, 30.0m, 25.0m, 20.0m, 1.5m, 12.0m, 15.0m, 2.0m)
      .AddStock("VIC", "Vingroup", 40.0m, 30.0m, 25.0m, 20.0m, 18.0m, 1.2m, 10.0m, 12.0m, 1.5m)
      .Build();

    _mockTradingViewClient.FetchAllStockDataAsync(Arg.Any<CancellationToken>())
      .Returns((peData, roicData));

    // Act
    var result = await _service.RankStocksAsync(request);

    // Assert
    Assert.Equal(0, result.RankedStocks[0].CombinedRank);
    Assert.Equal(2, result.RankedStocks[1].CombinedRank);
    Assert.Equal(0, result.RankedStocks[0].PeRank);
    Assert.Equal(1, result.RankedStocks[1].PeRank);
  }

  #endregion
}
