using Microsoft.AspNetCore.Mvc.Testing;
using NSubstitute;
using Stocker.Features.Stock.StockRanking;
using Stocker.Tests.Helpers.TestDataBuilders;
using Stocker.Tests.Integration;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Stocker.Tests.Integration.Features.Stock.StockRanking;

public class RankStocksEndpointTests : IClassFixture<TestWebApplicationFactory>
{
  private readonly HttpClient _client;
  private readonly TestWebApplicationFactory _factory;

  public RankStocksEndpointTests(TestWebApplicationFactory factory)
  {
    _factory = factory;
    _client = factory.Client;
  }

  #region Happy Path Tests

  [Fact]
  public async Task GetRanking_ValidRequest_ReturnsOk()
  {
    // Arrange
    SetupMockData();

    // Act
    var response = await _client.GetAsync("/api/stocks/ranking?NumberOfStocks=10");

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  [Fact]
  public async Task GetRanking_ValidRequest_ReturnsCorrectStructure()
  {
    // Arrange
    SetupMockData();

    // Act
    var response = await _client.GetAsync("/api/stocks/ranking?NumberOfStocks=10");

    // Assert
    var result = await response.Content.ReadFromJsonAsync<RankingResponse>();
    Assert.NotNull(result);
    Assert.NotNull(result.RankedStocks);
    Assert.NotNull(result.MissingCapStocks);
  }

  [Fact]
  public async Task GetRanking_ValidRequest_ReturnsCorrectCount()
  {
    // Arrange
    SetupMockData();

    // Act
    var response = await _client.GetAsync("/api/stocks/ranking?NumberOfStocks=2");

    // Assert
    var result = await response.Content.ReadFromJsonAsync<RankingResponse>();
    Assert.NotNull(result);
    Assert.Equal(2, result.TotalRanked);
    Assert.Equal(2, result.RankedStocks.Count);
  }

  [Fact]
  public async Task GetRanking_WithMinimumMarketCap_FiltersCorrectly()
  {
    // Arrange
    SetupMockData();

    // Act
    var response = await _client.GetAsync("/api/stocks/ranking?NumberOfStocks=10&MinimumMarketCap=4000000000000");

    // Assert
    var result = await response.Content.ReadFromJsonAsync<RankingResponse>();
    Assert.NotNull(result);
    Assert.True(result.RankedStocks.All(s => s.MarketCap >= 4000000000000m));
  }

  [Fact]
  public async Task GetRanking_WithNumberOfStocks_LimitsResults()
  {
    // Arrange
    SetupMockData();

    // Act
    var response = await _client.GetAsync("/api/stocks/ranking?NumberOfStocks=1");

    // Assert
    var result = await response.Content.ReadFromJsonAsync<RankingResponse>();
    Assert.NotNull(result);
    Assert.Single(result.RankedStocks);
  }

  #endregion

  #region Request Validation Tests

  [Fact]
  public async Task GetRanking_InvalidNumberOfStocks_ReturnsBadRequest()
  {
    // Arrange
    SetupMockData();

    // Act
    var response = await _client.GetAsync("/api/stocks/ranking?NumberOfStocks=invalid");

    // Assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task GetRanking_NegativeNumberOfStocks_ReturnsBadRequest()
  {
    // Arrange
    SetupMockData();

    // Act
    var response = await _client.GetAsync("/api/stocks/ranking?NumberOfStocks=-1");

    // Assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task GetRanking_ZeroNumberOfStocks_ReturnsBadRequest()
  {
    // Arrange
    SetupMockData();

    // Act
    var response = await _client.GetAsync("/api/stocks/ranking?NumberOfStocks=0");

    // Assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task GetRanking_NegativeMinimumMarketCap_ReturnsBadRequest()
  {
    // Arrange
    SetupMockData();

    // Act
    var response = await _client.GetAsync("/api/stocks/ranking?NumberOfStocks=10&MinimumMarketCap=-1000");

    // Assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task GetRanking_MissingRequiredFields_ReturnsBadRequest()
  {
    // Arrange
    SetupMockData();

    // Act
    var response = await _client.GetAsync("/api/stocks/ranking");

    // Assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  #endregion

  #region Response Format Tests

  [Fact]
  public async Task GetRanking_ResponseIncludesAllRequiredFields()
  {
    // Arrange
    SetupMockData();

    // Act
    var response = await _client.GetAsync("/api/stocks/ranking?NumberOfStocks=2");

    // Assert
    var result = await response.Content.ReadFromJsonAsync<RankingResponse>();
    Assert.NotNull(result);
    Assert.True(result.TotalRanked >= 0);
    Assert.True(result.TotalMissingCap >= 0);
    Assert.NotNull(result.RankedStocks);
    Assert.NotNull(result.MissingCapStocks);
  }

  [Fact]
  public async Task GetRanking_RankedStocks_HaveCorrectRanks()
  {
    // Arrange
    SetupMockData();

    // Act
    var response = await _client.GetAsync("/api/stocks/ranking?NumberOfStocks=2");

    // Assert
    var result = await response.Content.ReadFromJsonAsync<RankingResponse>();
    Assert.NotNull(result);
    Assert.NotNull(result.RankedStocks);
    foreach (var stock in result.RankedStocks)
    {
      Assert.True(stock.CombinedRank >= 0);
      Assert.True(stock.PeRank >= 0);
      Assert.True(stock.RoicRank >= 0);
    }
  }

  [Fact]
  public async Task GetRanking_MissingCapStocks_AreTracked()
  {
    // Arrange
    SetupMockDataWithMissingCap();

    // Act
    var response = await _client.GetAsync("/api/stocks/ranking?NumberOfStocks=10");

    // Assert
    var result = await response.Content.ReadFromJsonAsync<RankingResponse>();
    Assert.NotNull(result);
    Assert.True(result.TotalMissingCap > 0);
    Assert.NotNull(result.MissingCapStocks);
    Assert.True(result.MissingCapStocks.All(s => s.MarketCap == null));
  }

  [Fact]
  public async Task GetRanking_Stocks_IncludeMarketData()
  {
    // Arrange
    SetupMockData();

    // Act
    var response = await _client.GetAsync("/api/stocks/ranking?NumberOfStocks=2");

    // Assert
    var result = await response.Content.ReadFromJsonAsync<RankingResponse>();
    Assert.NotNull(result);
    foreach (var stock in result.RankedStocks)
    {
      Assert.NotNull(stock.Name);
      Assert.True(stock.Price >= 0 || stock.Price == null);
      Assert.True(stock.Volume >= 0 || stock.Volume == null);
    }
  }

  [Fact]
  public async Task GetRanking_Response_HasCorrectContentType()
  {
    // Arrange
    SetupMockData();

    // Act
    var response = await _client.GetAsync("/api/stocks/ranking?NumberOfStocks=10");

    // Assert
    Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
  }

  #endregion

  #region Helper Methods

  private void SetupMockData()
  {
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

    _factory.MockTradingViewClient.FetchAllStockDataAsync(Arg.Any<CancellationToken>())
      .Returns((peData, roicData));
  }

  private void SetupMockDataWithMissingCap()
  {
    var peData = TradingViewResponseBuilder.CreatePeResponse()
      .AddStock("VCB", "Vietcombank", 50.5m, 1.2m, 1000000L, 1.5m, 5000000000000m, "VND", 5.0m, 10000m, 10.5m, 2.5m, "Finance", "HOSE", "Banks")
      .AddStock("VIC", "Vingroup", 25.0m, -0.5m, 2000000L, 1.2m, null, "VND", 8.0m, 8000m, 15.0m, 3.0m, "Real Estate", "HOSE", "Real Estate")
      .Build();

    var roicData = TradingViewResponseBuilder.CreateRoicResponse()
      .AddStock("VCB", "Vietcombank", 45.0m, 35.0m, 30.0m, 25.0m, 20.0m, 1.5m, 12.0m, 15.0m, 2.0m)
      .AddStock("VIC", "Vingroup", 40.0m, 30.0m, 25.0m, 20.0m, 18.0m, 1.2m, 10.0m, 12.0m, 1.5m)
      .Build();

    _factory.MockTradingViewClient.FetchAllStockDataAsync(Arg.Any<CancellationToken>())
      .Returns((peData, roicData));
  }

  #endregion
}
