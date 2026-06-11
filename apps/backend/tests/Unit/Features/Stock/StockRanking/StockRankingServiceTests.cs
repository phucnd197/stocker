using NSubstitute;
using Stocker.Core.Clients;
using Stocker.Features.Stock.StockRanking;
using Stocker.Tests.Helpers.TestDataBuilders;
using Xunit;

namespace Stocker.Tests.Unit.Features.Stock.StockRanking;

public class StockRankingServiceTests
{
  private readonly ITradingViewClient _mockTradingViewClient;
  private readonly StockRankingService _service;

  public StockRankingServiceTests()
  {
    _mockTradingViewClient = Substitute.For<ITradingViewClient>();
    _service = new StockRankingService(_mockTradingViewClient);
  }

  #region Service Orchestration Tests

  [Fact]
  public async Task RankStocksAsync_FetchesDataFromTradingView()
  {
    // Arrange
    var request = RankingRequestBuilder.CreateDefault().Build();
    SetupMockStockData();

    // Act
    await _service.RankStocksAsync(request);

    // Assert
    await _mockTradingViewClient.Received(1).FetchAllStockDataAsync(false, Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task RankStocksAsync_PropagatesRefreshFlag()
  {
    // Arrange
    var request = RankingRequestBuilder.CreateDefault().WithRefresh(true).Build();
    SetupMockStockData();

    // Act
    await _service.RankStocksAsync(request);

    // Assert
    await _mockTradingViewClient.Received(1).FetchAllStockDataAsync(true, Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task RankStocksAsync_PropagatesCancellationToken()
  {
    // Arrange
    var request = RankingRequestBuilder.CreateDefault().Build();
    using var cts = new CancellationTokenSource();
    var token = cts.Token;
    SetupMockStockData();

    // Act
    await _service.RankStocksAsync(request, token);

    // Assert
    await _mockTradingViewClient.Received(1).FetchAllStockDataAsync(Arg.Any<bool>(), token);
  }

  #endregion

  #region Ranking Calculation Tests

  [Fact]
  public void CalculateRankings_TwoStocks_AssignsCorrectPeAndRoicRanks()
  {
    // Arrange
    var stockData = new List<CompanyData>
    {
      CreateVcb(),
      CreateVic()
    };

    // Act
    var result = _service.CalculateRankings(stockData);

    // Assert - PE ascending: VCB(5.0) rank 0, VIC(8.0) rank 1
    // ROIC descending: VCB(15.0) rank 0, VIC(12.0) rank 1
    Assert.Equal(2, result.Count);

    var vcb = result.First(s => s.Identifier == "VCB");
    Assert.Equal(0, vcb.PeRank);
    Assert.Equal(0, vcb.RoicRank);
    Assert.Equal(0, vcb.CombinedRank);

    var vic = result.First(s => s.Identifier == "VIC");
    Assert.Equal(1, vic.PeRank);
    Assert.Equal(1, vic.RoicRank);
    Assert.Equal(2, vic.CombinedRank);
  }

  [Fact]
  public void CalculateRankings_FourStocks_CalculatesCorrectCombinedRank()
  {
    // Arrange - PE: VCB(5.0), HPG(7.0), VIC(8.0), MWG(9.0)
    // PE ranks: VCB=0, HPG=1, VIC=2, MWG=3
    // ROIC: HPG(20.0), VCB(15.0), VIC(12.0), MWG(10.0)
    // ROIC ranks: HPG=0, VCB=1, VIC=2, MWG=3
    // Combined: VCB=0+1=1, HPG=1+0=1, VIC=2+2=4, MWG=3+3=6
    var stockData = new List<CompanyData>
    {
      CreateVcb(),
      CreateVic(),
      CreateHpg(),
      CreateMwg()
    };

    // Act
    var result = _service.CalculateRankings(stockData);

    // Assert
    Assert.Equal(4, result.Count);

    var vcb = result.First(s => s.Identifier == "VCB");
    Assert.Equal(0, vcb.PeRank);
    Assert.Equal(1, vcb.RoicRank);
    Assert.Equal(1, vcb.CombinedRank);

    var hpg = result.First(s => s.Identifier == "HPG");
    Assert.Equal(1, hpg.PeRank);
    Assert.Equal(0, hpg.RoicRank);
    Assert.Equal(1, hpg.CombinedRank);

    var vic = result.First(s => s.Identifier == "VIC");
    Assert.Equal(2, vic.PeRank);
    Assert.Equal(2, vic.RoicRank);
    Assert.Equal(4, vic.CombinedRank);

    var mwg = result.First(s => s.Identifier == "MWG");
    Assert.Equal(3, mwg.PeRank);
    Assert.Equal(3, mwg.RoicRank);
    Assert.Equal(6, mwg.CombinedRank);
  }

  [Fact]
  public void CalculateRankings_SingleStock_GetsAllZeroRanks()
  {
    // Arrange
    var stockData = new List<CompanyData> { CreateVcb() };

    // Act
    var result = _service.CalculateRankings(stockData);

    // Assert
    var vcb = result.First(s => s.Identifier == "VCB");
    Assert.Equal(0, vcb.PeRank);
    Assert.Equal(0, vcb.RoicRank);
    Assert.Equal(0, vcb.CombinedRank);
  }

  [Fact]
  public void CalculateRankings_NullPeValue_SortedFirstInAscendingOrder()
  {
    // Arrange - null PE sorts before all non-null values in OrderBy
    // PE order: null(VCB), 7.0(HPG), 8.0(VIC)
    // PE ranks: VCB=0, HPG=1, VIC=2
    var stockData = new List<CompanyData>
    {
      new CompanyDataBuilder().WithIdentifier("VCB").WithName("Vietcombank")
        .WithPeRatio(null).WithRoic(15.0m).Build(),
      CreateVic(),
      CreateHpg()
    };

    // Act
    var result = _service.CalculateRankings(stockData);

    // Assert
    var vcb = result.First(s => s.Identifier == "VCB");
    Assert.Equal(0, vcb.PeRank); // null PE gets best (lowest) PE rank
    Assert.Equal(1, vcb.RoicRank); // ROIC 15.0 is second after HPG 20.0
  }

  [Fact]
  public void CalculateRankings_NullRoicValue_SortedLastInDescendingOrder()
  {
    // Arrange - null ROIC sorts after all non-null values in OrderByDescending
    // ROIC order: 15.0(VCB), 12.0(VIC), null(HPG)
    // ROIC ranks: VCB=0, VIC=1, HPG=2
    var stockData = new List<CompanyData>
    {
      CreateVcb(),
      CreateVic(),
      new CompanyDataBuilder().WithIdentifier("HPG").WithName("Hoa Phat")
        .WithPeRatio(7.0m).WithRoic(null).Build()
    };

    // Act
    var result = _service.CalculateRankings(stockData);

    // Assert
    var hpg = result.First(s => s.Identifier == "HPG");
    Assert.Equal(2, hpg.RoicRank); // null ROIC gets worst (highest) ROIC rank
  }

  [Fact]
  public void CalculateRankings_PreservesIndividualRanks()
  {
    // Arrange
    var stockData = new List<CompanyData> { CreateVcb(), CreateVic() };

    // Act
    var result = _service.CalculateRankings(stockData);

    // Assert - verify all rank properties are set on every stock
    foreach (var stock in result)
    {
      Assert.True(stock.PeRank >= 0);
      Assert.True(stock.RoicRank >= 0);
      Assert.Equal(stock.PeRank + stock.RoicRank, stock.CombinedRank);
    }
  }

  #endregion

  #region Market Cap Filtering Tests

  [Fact]
  public async Task RankStocksAsync_WithMinimumMarketCap_FiltersCorrectly()
  {
    // Arrange - VCB: 5T, VIC: 3T, threshold: 4T
    var request = RankingRequestBuilder.CreateDefault()
      .WithMinimumMarketCap(4000000000000m)
      .Build();

    var stockData = new[]
    {
      CreateVcb(), // MarketCap = 5T - passes
      CreateVic()  // MarketCap = 3T - filtered out
    };
    _mockTradingViewClient.FetchAllStockDataAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>())
      .Returns(stockData);

    // Act
    var (ranked, missingCap) = await _service.RankStocksAsync(request);

    // Assert
    Assert.Single(ranked);
    Assert.Equal("VCB", ranked[0].Identifier);
    Assert.Empty(missingCap);
  }

  [Fact]
  public async Task RankStocksAsync_WithNullMinimumMarketCap_IncludesAll()
  {
    // Arrange
    var request = RankingRequestBuilder.CreateDefault()
      .WithMinimumMarketCap(null)
      .Build();

    var stockData = new[] { CreateVcb(), CreateVic() };
    _mockTradingViewClient.FetchAllStockDataAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>())
      .Returns(stockData);

    // Act
    var (ranked, missingCap) = await _service.RankStocksAsync(request);

    // Assert
    Assert.Equal(2, ranked.Count);
    Assert.Empty(missingCap);
  }

  [Fact]
  public async Task RankStocksAsync_SeparatesMissingCapStocks()
  {
    // Arrange - VIC has null market cap
    var request = RankingRequestBuilder.CreateDefault().Build();

    var stockData = new[]
    {
      CreateVcb(),
      new CompanyDataBuilder().WithIdentifier("VIC").WithName("Vingroup")
        .WithPeRatio(8.0m).WithRoic(12.0m).WithMarketCap(null).Build()
    };
    _mockTradingViewClient.FetchAllStockDataAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>())
      .Returns(stockData);

    // Act
    var (ranked, missingCap) = await _service.RankStocksAsync(request);

    // Assert
    Assert.Single(ranked);
    Assert.Single(missingCap);
    Assert.Equal("VIC", missingCap[0].Identifier);
  }

  [Fact]
  public async Task RankStocksAsync_BelowMinimumCap_ExcludedFromResults()
  {
    // Arrange - VCB: 5T (passes), VIC: 3T (below threshold)
    var request = RankingRequestBuilder.CreateDefault()
      .WithMinimumMarketCap(4000000000000m)
      .Build();

    var stockData = new[] { CreateVcb(), CreateVic() };
    _mockTradingViewClient.FetchAllStockDataAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>())
      .Returns(stockData);

    // Act
    var (ranked, missingCap) = await _service.RankStocksAsync(request);

    // Assert
    Assert.Single(ranked);
    Assert.Equal("VCB", ranked[0].Identifier);
    Assert.Empty(missingCap); // VIC has a market cap, it's just below threshold
  }

  [Fact]
  public async Task RankStocksAsync_EmptyData_ReturnsEmptyResults()
  {
    // Arrange
    var request = RankingRequestBuilder.CreateDefault().Build();
    _mockTradingViewClient.FetchAllStockDataAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>())
      .Returns(Array.Empty<CompanyData>());

    // Act
    var (ranked, missingCap) = await _service.RankStocksAsync(request);

    // Assert
    Assert.Empty(ranked);
    Assert.Empty(missingCap);
  }

  #endregion

  #region Result Sorting Tests

  [Fact]
  public async Task RankStocksAsync_SortsByCombinedRankAscending()
  {
    // Arrange - Combined: VCB=0+0=0, VIC=1+1=2
    var request = RankingRequestBuilder.CreateDefault().Build();
    var stockData = new[] { CreateVcb(), CreateVic() };
    _mockTradingViewClient.FetchAllStockDataAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>())
      .Returns(stockData);

    // Act
    var (ranked, _) = await _service.RankStocksAsync(request);

    // Assert - sorted ascending by combined rank
    for (int i = 1; i < ranked.Count; i++)
    {
      Assert.True(ranked[i].CombinedRank >= ranked[i - 1].CombinedRank);
    }
  }

  [Fact]
  public async Task RankStocksAsync_FourStocks_CorrectSortOrder()
  {
    // Arrange - Combined: VCB=1, HPG=1, VIC=4, MWG=6
    var request = RankingRequestBuilder.CreateDefault().Build();
    var stockData = new CompanyData[] { CreateVcb(), CreateVic(), CreateHpg(), CreateMwg() };
    _mockTradingViewClient.FetchAllStockDataAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>())
      .Returns(stockData);

    // Act
    var (ranked, _) = await _service.RankStocksAsync(request);

    // Assert
    Assert.Equal(4, ranked.Count);
    Assert.Equal(1, ranked[0].CombinedRank); // VCB or HPG
    Assert.Equal(1, ranked[1].CombinedRank); // HPG or VCB
    Assert.Equal(4, ranked[2].CombinedRank); // VIC
    Assert.Equal(6, ranked[3].CombinedRank); // MWG
  }

  [Fact]
  public async Task RankStocksAsync_PreservesRankOrderInResults()
  {
    // Arrange
    var request = RankingRequestBuilder.CreateDefault().Build();
    var stockData = new[] { CreateVcb(), CreateVic() };
    _mockTradingViewClient.FetchAllStockDataAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>())
      .Returns(stockData);

    // Act
    var (ranked, _) = await _service.RankStocksAsync(request);

    // Assert
    Assert.Equal(0, ranked[0].CombinedRank); // VCB
    Assert.Equal(2, ranked[1].CombinedRank); // VIC
    Assert.Equal(0, ranked[0].PeRank);
    Assert.Equal(1, ranked[1].PeRank);
  }

  #endregion

  #region Helper Methods

  private void SetupMockStockData()
  {
    var stockData = new[] { CreateVcb(), CreateVic() };
    _mockTradingViewClient.FetchAllStockDataAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>())
      .Returns(stockData);
  }

  private static CompanyData CreateVcb() =>
    new CompanyDataBuilder()
      .WithIdentifier("VCB").WithName("Vietcombank")
      .WithPrice(50.5m).WithChange(1.2m).WithVolume(1000000L)
      .WithMarketCap(5000000000000m).WithFundamentalCurrencyCode("VND")
      .WithPeRatio(5.0m).WithEps(10000m).WithEpsGrowth(10.5m).WithDividendsYield(2.5m)
      .WithGrossMargin(45.0m).WithOperatingMargin(35.0m).WithRoic(15.0m)
      .WithSectorTr("Finance").WithMarket("HOSE").WithSector("Banks")
      .Build();

  private static CompanyData CreateVic() =>
    new CompanyDataBuilder()
      .WithIdentifier("VIC").WithName("Vingroup")
      .WithPrice(25.0m).WithChange(-0.5m).WithVolume(2000000L)
      .WithMarketCap(3000000000000m).WithFundamentalCurrencyCode("VND")
      .WithPeRatio(8.0m).WithEps(8000m).WithEpsGrowth(15.0m).WithDividendsYield(3.0m)
      .WithGrossMargin(40.0m).WithOperatingMargin(30.0m).WithRoic(12.0m)
      .WithSectorTr("Real Estate").WithMarket("HOSE").WithSector("Real Estate")
      .Build();

  private static CompanyData CreateHpg() =>
    new CompanyDataBuilder()
      .WithIdentifier("HPG").WithName("Hoa Phat")
      .WithPrice(30.0m).WithChange(0.5m).WithVolume(1500000L)
      .WithMarketCap(2000000000000m).WithFundamentalCurrencyCode("VND")
      .WithPeRatio(7.0m).WithEps(9000m).WithEpsGrowth(12.0m).WithDividendsYield(2.0m)
      .WithGrossMargin(50.0m).WithOperatingMargin(40.0m).WithRoic(20.0m)
      .WithSectorTr("Steel").WithMarket("HOSE").WithSector("Steel")
      .Build();

  private static CompanyData CreateMwg() =>
    new CompanyDataBuilder()
      .WithIdentifier("MWG").WithName("Mobile World")
      .WithPrice(20.0m).WithChange(0.3m).WithVolume(800000L)
      .WithMarketCap(1000000000000m).WithFundamentalCurrencyCode("VND")
      .WithPeRatio(9.0m).WithEps(5000m).WithEpsGrowth(8.0m).WithDividendsYield(1.5m)
      .WithGrossMargin(35.0m).WithOperatingMargin(25.0m).WithRoic(10.0m)
      .WithSectorTr("Retail").WithMarket("HOSE").WithSector("Retail")
      .Build();

  #endregion
}
