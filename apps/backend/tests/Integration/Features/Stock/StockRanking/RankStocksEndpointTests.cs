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
  // private HttpClient _client;
  private readonly TestWebApplicationFactory _factory;

  public RankStocksEndpointTests(TestWebApplicationFactory factory)
  {
    _factory = factory;
  }

  #region Happy Path Tests

  [Fact]
  public async Task GetRanking_ValidRequest_ReturnsOk()
  {
    // Arrange
    SetupMockData();

    // Act
    var response = await _factory.CreateKestrelClient().GetAsync("/api/stocks/ranking?NumberOfStocks=10");

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  [Fact]
  public async Task GetRanking_ValidRequest_ReturnsCorrectStructure()
  {
    // Arrange
    SetupMockData();

    // Act
    var response = await _factory.CreateKestrelClient().GetAsync("/api/stocks/ranking?NumberOfStocks=10");

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
    var response = await _factory.CreateKestrelClient().GetAsync("/api/stocks/ranking?NumberOfStocks=3");

    // Assert
    var result = await response.Content.ReadFromJsonAsync<RankingResponse>();
    Assert.NotNull(result);
    Assert.Equal(3, result.TotalRanked);
    Assert.Equal(3, result.RankedStocks.Count);
  }

  [Fact]
  public async Task GetRanking_WithMinimumMarketCap_FiltersCorrectly()
  {
    // Arrange
    SetupMockData();

    // Act
    var response = await _factory.CreateKestrelClient().GetAsync("/api/stocks/ranking?NumberOfStocks=10&MinimumMarketCap=4000000000000");

    // Assert
    var result = await response.Content.ReadFromJsonAsync<RankingResponse>();
    Assert.NotNull(result);
    Assert.True(result.RankedStocks.All(s => s.MarketCapBasic >= 4000000000000m));
  }

  #endregion

  #region Request Validation Tests

  [Fact]
  public async Task GetRanking_InvalidNumberOfStocks_ReturnsBadRequest()
  {
    // Arrange
    SetupMockData();

    // Act
    var response = await _factory.CreateKestrelClient().GetAsync("/api/stocks/ranking?NumberOfStocks=invalid");

    // Assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task GetRanking_NegativeNumberOfStocks_ReturnsBadRequest()
  {
    // Arrange
    SetupMockData();

    // Act
    var response = await _factory.CreateKestrelClient().GetAsync("/api/stocks/ranking?NumberOfStocks=-1");

    // Assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task GetRanking_ZeroNumberOfStocks_ReturnsBadRequest()
  {
    // Arrange
    SetupMockData();

    // Act
    var response = await _factory.CreateKestrelClient().GetAsync("/api/stocks/ranking?NumberOfStocks=0");

    // Assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task GetRanking_NegativeMinimumMarketCap_ReturnsBadRequest()
  {
    // Arrange
    SetupMockData();

    // Act
    var response = await _factory.CreateKestrelClient().GetAsync("/api/stocks/ranking?NumberOfStocks=10&MinimumMarketCap=-1000");

    // Assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task GetRanking_MissingRequiredFields_ReturnsBadRequest()
  {
    // Arrange
    SetupMockData();

    // Act
    var response = await _factory.CreateKestrelClient().GetAsync("/api/stocks/ranking");

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
    var response = await _factory.CreateKestrelClient().GetAsync("/api/stocks/ranking?NumberOfStocks=2");

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
    var response = await _factory.CreateKestrelClient().GetAsync("/api/stocks/ranking?NumberOfStocks=2");

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
    var response = await _factory.CreateKestrelClient().GetAsync("/api/stocks/ranking?NumberOfStocks=10");

    // Assert
    var result = await response.Content.ReadFromJsonAsync<RankingResponse>();
    Assert.NotNull(result);
    Assert.True(result.TotalMissingCap > 0);
    Assert.NotNull(result.MissingCapStocks);
    Assert.True(result.MissingCapStocks.All(s => s.MarketCapBasic == null));
  }

  [Fact]
  public async Task GetRanking_Stocks_IncludeMarketData()
  {
    // Arrange
    SetupMockData();

    // Act
    var response = await _factory.CreateKestrelClient().GetAsync("/api/stocks/ranking?NumberOfStocks=2");

    // Assert
    var result = await response.Content.ReadFromJsonAsync<RankingResponse>();
    Assert.NotNull(result);
    foreach (var stock in result.RankedStocks)
    {
      Assert.NotNull(stock.Name);
      Assert.True(stock.Close >= 0 || stock.Close == null);
      Assert.True(stock.Volume >= 0 || stock.Volume == null);
    }
  }

  [Fact]
  public async Task GetRanking_Response_HasCorrectContentType()
  {
    // Arrange
    SetupMockData();

    // Act
    var response = await _factory.CreateKestrelClient().GetAsync("/api/stocks/ranking?NumberOfStocks=10");

    // Assert
    Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
  }

  #endregion

  #region Helper Methods

  private void SetupMockData()
  {
    var stockData = new[]
    {
      new CompanyDataBuilder().WithIdentifier("HOSE:VCB").WithName("Vietcombank")
        .WithPrice(50.5m).WithChange(1.2m).WithVolume(1000000L)
        .WithMarketCap(5000000000000m).WithFundamentalCurrencyCode("VND")
        .WithPeRatio(5.0m).WithEps(10000m).WithRoic(15.0m)
        .WithSectorTr("Finance").WithMarket("HOSE").WithSector("Banks")
        .Build(),
      new CompanyDataBuilder().WithIdentifier("HOSE:VIC").WithName("Vingroup")
        .WithPrice(25.0m).WithChange(-0.5m).WithVolume(2000000L)
        .WithMarketCap(3000000000000m).WithFundamentalCurrencyCode("VND")
        .WithPeRatio(8.0m).WithEps(8000m).WithRoic(12.0m)
        .WithSectorTr("Real Estate").WithMarket("HOSE").WithSector("Real Estate")
        .Build(),
      new CompanyDataBuilder().WithIdentifier("HOSE:HPG").WithName("Hoa Phat")
        .WithPrice(30.0m).WithChange(0.5m).WithVolume(1500000L)
        .WithMarketCap(2000000000000m).WithFundamentalCurrencyCode("VND")
        .WithPeRatio(7.0m).WithEps(9000m).WithRoic(20.0m)
        .WithSectorTr("Steel").WithMarket("HOSE").WithSector("Steel")
        .Build()
    };

    _factory.MockTradingViewClient.FetchAllStockDataAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>())
      .Returns(stockData);
  }

  private void SetupMockDataWithMissingCap()
  {
    var stockData = new[]
    {
      new CompanyDataBuilder().WithIdentifier("HOSE:VCB").WithName("Vietcombank")
        .WithPrice(50.5m).WithChange(1.2m).WithVolume(1000000L)
        .WithMarketCap(5000000000000m).WithFundamentalCurrencyCode("VND")
        .WithPeRatio(5.0m).WithEps(10000m).WithRoic(15.0m)
        .WithSectorTr("Finance").WithMarket("HOSE").WithSector("Banks")
        .Build(),
      new CompanyDataBuilder().WithIdentifier("HOSE:VIC").WithName("Vingroup")
        .WithPrice(25.0m).WithChange(-0.5m).WithVolume(2000000L)
        .WithMarketCap(null).WithFundamentalCurrencyCode("VND")
        .WithPeRatio(8.0m).WithEps(8000m).WithRoic(12.0m)
        .WithSectorTr("Real Estate").WithMarket("HOSE").WithSector("Real Estate")
        .Build()
    };

    _factory.MockTradingViewClient.FetchAllStockDataAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>())
      .Returns(stockData);
  }

  #endregion
}
