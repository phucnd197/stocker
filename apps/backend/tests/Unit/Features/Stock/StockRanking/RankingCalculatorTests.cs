using System.Text.Json.Nodes;
using Stocker.Features.Stock.StockRanking;
using Stocker.Tests.Helpers.TestDataBuilders;
using Xunit;

namespace Stocker.Tests.Unit.Features.Stock.StockRanking;

public class RankingCalculatorTests
{
  private readonly RankingCalculator _calculator = new();

  #region Data Extraction Tests

  [Fact]
  public void ExtractRank_ValidPeData_ReturnsCorrectRankings()
  {
    // Arrange
    var peResponse = TradingViewResponseBuilder.CreatePeResponse()
      .AddStock("VCB", "Vietcombank", 50.5m, 1.2m, 1000000L, 1.5m, 5000000000000m, "VND", 5.0m, 10000m, 10.5m, 2.5m, "Finance", "HOSE", "Banks")
      .AddStock("VIC", "Vingroup", 25.0m, -0.5m, 2000000L, 1.2m, 3000000000000m, "VND", 8.0m, 8000m, 15.0m, 3.0m, "Real Estate", "HOSE", "Real Estate")
      .Build();

    var roicResponse = TradingViewResponseBuilder.CreateRoicResponse()
      .AddStock("VCB", "Vietcombank", 45.0m, 35.0m, 30.0m, 25.0m, 20.0m, 1.5m, 12.0m, 15.0m, 2.0m)
      .AddStock("VIC", "Vingroup", 40.0m, 30.0m, 25.0m, 20.0m, 18.0m, 1.2m, 10.0m, 12.0m, 1.5m)
      .Build();

    // Act
    var result = _calculator.CalculateRankings(peResponse, roicResponse);

    // Assert
    Assert.Equal(2, result.Count);
    Assert.True(result.ContainsKey("VCB"));
    Assert.True(result.ContainsKey("VIC"));
    Assert.Equal(0, result["VCB"].PeRank);
    Assert.Equal(0, result["VCB"].RoicRank);
    Assert.Equal(0, result["VCB"].CombinedRank);
    Assert.Equal(1, result["VIC"].PeRank);
    Assert.Equal(1, result["VIC"].RoicRank);
    Assert.Equal(2, result["VIC"].CombinedRank);
  }

  [Fact]
  public void ExtractRank_NullResponse_ReturnsEmptyDictionary()
  {
    // Arrange & Act
    var result = _calculator.CalculateRankings(null!, new TradingViewResponse { TotalCount = 0, Data = [] });

    // Assert
    Assert.Empty(result);
  }

  [Fact]
  public void ExtractRank_NullData_ReturnsEmptyDictionary()
  {
    // Arrange
    var response = new TradingViewResponse { TotalCount = 0, Data = null };

    // Act
    var result = _calculator.CalculateRankings(response, new TradingViewResponse { TotalCount = 0, Data = [] });

    // Assert
    Assert.Empty(result);
  }

  [Fact]
  public void ExtractRank_ValidRoicData_ReturnsCorrectRankings()
  {
    // Arrange
    var peResponse = TradingViewResponseBuilder.CreatePeResponse()
      .AddStock("VCB", "Vietcombank", 50.5m, 1.2m, 1000000L, 1.5m, 5000000000000m, "VND", 5.0m, 10000m, 10.5m, 2.5m, "Finance", "HOSE", "Banks")
      .AddStock("VIC", "Vingroup", 25.0m, -0.5m, 2000000L, 1.2m, 3000000000000m, "VND", 8.0m, 8000m, 15.0m, 3.0m, "Real Estate", "HOSE", "Real Estate")
      .Build();

    var roicResponse = TradingViewResponseBuilder.CreateRoicResponse()
      .AddStock("VCB", "Vietcombank", 45.0m, 35.0m, 30.0m, 25.0m, 20.0m, 1.5m, 12.0m, 15.0m, 2.0m)
      .AddStock("VIC", "Vingroup", 40.0m, 30.0m, 25.0m, 20.0m, 18.0m, 1.2m, 10.0m, 12.0m, 1.5m)
      .Build();

    // Act
    var result = _calculator.CalculateRankings(peResponse, roicResponse);

    // Assert
    Assert.Equal(2, result.Count);
    Assert.True(result.ContainsKey("VCB"));
    Assert.True(result.ContainsKey("VIC"));
  }

  [Fact]
  public void ExtractRank_MissingCompanyName_SkipsInvalidEntry()
  {
    // Arrange - stocks with empty or null company names should be skipped
    var peResponse = TradingViewResponseBuilder.CreatePeResponse()
      .AddStock("VCB", "", 50.5m, 1.2m, 1000000L, 1.5m, 5000000000000m, "VND", 5.0m, 10000m, 10.5m, 2.5m, "Finance", "HOSE", "Banks")
      .AddStock("VIC", null, 25.0m, -0.5m, 2000000L, 1.2m, 3000000000000m, "VND", 8.0m, 8000m, 15.0m, 3.0m, "Real Estate", "HOSE", "Real Estate")
      .AddStock("HPG", "Hoa Phat", 30.0m, 0.5m, 1500000L, 1.3m, 2000000000000m, "VND", 7.0m, 9000m, 12.0m, 2.0m, "Steel", "HOSE", "Steel")
      .Build();

    var roicResponse = TradingViewResponseBuilder.CreateRoicResponse()
      .AddStock("VCB", "Vietcombank", 45.0m, 35.0m, 30.0m, 25.0m, 20.0m, 1.5m, 12.0m, 15.0m, 2.0m)
      .AddStock("VIC", "Vingroup", 40.0m, 30.0m, 25.0m, 20.0m, 18.0m, 1.2m, 10.0m, 12.0m, 1.5m)
      .AddStock("HPG", "Hoa Phat", 35.0m, 25.0m, 20.0m, 15.0m, 16.0m, 1.0m, 8.0m, 10.0m, 1.2m)
      .Build();

    // Act
    var result = _calculator.CalculateRankings(peResponse, roicResponse);

    // Assert - only HPG should be in results since VCB and VIC have invalid names in PE response
    Assert.Single(result);
    Assert.True(result.ContainsKey("HPG"));
    Assert.False(result.ContainsKey("VCB"));
    Assert.False(result.ContainsKey("VIC"));
  }

  #endregion

  #region Ranking Combination Tests

  [Fact]
  public void CombineRankings_OverlappingCompanies_CalculatesCombinedRank()
  {
    // Arrange
    var peResponse = TradingViewResponseBuilder.CreatePeResponse()
      .AddStock("VCB", "Vietcombank", 50.5m, 1.2m, 1000000L, 1.5m, 5000000000000m, "VND", 5.0m, 10000m, 10.5m, 2.5m, "Finance", "HOSE", "Banks")
      .AddStock("VIC", "Vingroup", 25.0m, -0.5m, 2000000L, 1.2m, 3000000000000m, "VND", 8.0m, 8000m, 15.0m, 3.0m, "Real Estate", "HOSE", "Real Estate")
      .Build();

    var roicResponse = TradingViewResponseBuilder.CreateRoicResponse()
      .AddStock("VCB", "Vietcombank", 45.0m, 35.0m, 30.0m, 25.0m, 20.0m, 1.5m, 12.0m, 15.0m, 2.0m)
      .AddStock("VIC", "Vingroup", 40.0m, 30.0m, 25.0m, 20.0m, 18.0m, 1.2m, 10.0m, 12.0m, 1.5m)
      .Build();

    // Act
    var result = _calculator.CalculateRankings(peResponse, roicResponse);

    // Assert
    Assert.Equal(2, result.Count);
    Assert.Equal(0, result["VCB"].PeRank);
    Assert.Equal(0, result["VCB"].RoicRank);
    Assert.Equal(0, result["VCB"].CombinedRank);
    Assert.Equal(1, result["VIC"].PeRank);
    Assert.Equal(1, result["VIC"].RoicRank);
    Assert.Equal(2, result["VIC"].CombinedRank);
  }

  [Fact]
  public void CombineRankings_MultipleStocks_CalculatesCorrectCombinedRank()
  {
    // Arrange - Test with 4 stocks where rankings differ between PE and ROIC
    // PE order: VCB (rank 0), VIC (rank 1), HPG (rank 2), MWG (rank 3)
    // ROIC order: HPG (rank 0), VCB (rank 1), VIC (rank 2), MWG (rank 3)
    // Expected combined: HPG (0+1=1), VCB (0+1=1), VIC (1+2=3), MWG (3+3=6)
    var peResponse = TradingViewResponseBuilder.CreatePeResponse()
      .AddStock("VCB", "Vietcombank", 50.5m, 1.2m, 1000000L, 1.5m, 5000000000000m, "VND", 5.0m, 10000m, 10.5m, 2.5m, "Finance", "HOSE", "Banks")
      .AddStock("VIC", "Vingroup", 25.0m, -0.5m, 2000000L, 1.2m, 3000000000000m, "VND", 8.0m, 8000m, 15.0m, 3.0m, "Real Estate", "HOSE", "Real Estate")
      .AddStock("HPG", "Hoa Phat", 30.0m, 0.5m, 1500000L, 1.3m, 2000000000000m, "VND", 7.0m, 9000m, 12.0m, 2.0m, "Steel", "HOSE", "Steel")
      .AddStock("MWG", "Mobile World", 20.0m, 0.3m, 800000L, 1.1m, 1000000000000m, "VND", 9.0m, 5000m, 8.0m, 1.5m, "Retail", "HOSE", "Retail")
      .Build();

    var roicResponse = TradingViewResponseBuilder.CreateRoicResponse()
      .AddStock("HPG", "Hoa Phat", 50.0m, 40.0m, 35.0m, 30.0m, 25.0m, 2.0m, 18.0m, 20.0m, 3.0m)
      .AddStock("VCB", "Vietcombank", 45.0m, 35.0m, 30.0m, 25.0m, 20.0m, 1.5m, 12.0m, 15.0m, 2.0m)
      .AddStock("VIC", "Vingroup", 40.0m, 30.0m, 25.0m, 20.0m, 18.0m, 1.2m, 10.0m, 12.0m, 1.5m)
      .AddStock("MWG", "Mobile World", 35.0m, 25.0m, 20.0m, 15.0m, 16.0m, 1.0m, 8.0m, 10.0m, 1.2m)
      .Build();

    // Act
    var result = _calculator.CalculateRankings(peResponse, roicResponse);

    // Assert
    Assert.Equal(4, result.Count);

    // Verify individual ranks
    Assert.Equal(0, result["VCB"].PeRank);
    Assert.Equal(1, result["VCB"].RoicRank);
    Assert.Equal(1, result["VCB"].CombinedRank);

    Assert.Equal(1, result["VIC"].PeRank);
    Assert.Equal(2, result["VIC"].RoicRank);
    Assert.Equal(3, result["VIC"].CombinedRank);

    Assert.Equal(2, result["HPG"].PeRank);
    Assert.Equal(0, result["HPG"].RoicRank);
    Assert.Equal(2, result["HPG"].CombinedRank);

    Assert.Equal(3, result["MWG"].PeRank);
    Assert.Equal(3, result["MWG"].RoicRank);
    Assert.Equal(6, result["MWG"].CombinedRank);

    // Verify sorting by combined rank (lower is better)
    var sortedStocks = result.Values.OrderBy(x => x.CombinedRank).ToList();
    Assert.Equal("VCB", sortedStocks[0].Name); // combined rank 1
    Assert.Equal("HPG", sortedStocks[1].Name); // combined rank 2
    Assert.Equal("VIC", sortedStocks[2].Name); // combined rank 3
    Assert.Equal("MWG", sortedStocks[3].Name); // combined rank 6
  }

  [Fact]
  public void CombineRankings_CompaniesInPeOnly_ExcludesFromResults()
  {
    // Arrange
    var peResponse = TradingViewResponseBuilder.CreatePeResponse()
      .AddStock("VCB", "Vietcombank", 50.5m, 1.2m, 1000000L, 1.5m, 5000000000000m, "VND", 5.0m, 10000m, 10.5m, 2.5m, "Finance", "HOSE", "Banks")
      .Build();

    var roicResponse = TradingViewResponseBuilder.CreateRoicResponse()
      .AddStock("VIC", "Vingroup", 40.0m, 30.0m, 25.0m, 20.0m, 18.0m, 1.2m, 10.0m, 12.0m, 1.5m)
      .Build();

    // Act
    var result = _calculator.CalculateRankings(peResponse, roicResponse);

    // Assert
    Assert.Empty(result);
  }

  [Fact]
  public void CombineRankings_CompaniesInRoicOnly_ExcludesFromResults()
  {
    // Arrange
    var peResponse = TradingViewResponseBuilder.CreatePeResponse()
      .AddStock("VCB", "Vietcombank", 50.5m, 1.2m, 1000000L, 1.5m, 5000000000000m, "VND", 5.0m, 10000m, 10.5m, 2.5m, "Finance", "HOSE", "Banks")
      .Build();

    var roicResponse = TradingViewResponseBuilder.CreateRoicResponse()
      .AddStock("VIC", "Vingroup", 40.0m, 30.0m, 25.0m, 20.0m, 18.0m, 1.2m, 10.0m, 12.0m, 1.5m)
      .Build();

    // Act
    var result = _calculator.CalculateRankings(peResponse, roicResponse);

    // Assert
    Assert.Empty(result);
  }

  [Fact]
  public void CombineRankings_CorrectlyMergesCompanyData()
  {
    // Arrange
    var peResponse = TradingViewResponseBuilder.CreatePeResponse()
      .AddStock("VCB", "Vietcombank", 50.5m, 1.2m, 1000000L, 1.5m, 5000000000000m, "VND", 5.0m, 10000m, 10.5m, 2.5m, "Finance", "HOSE", "Banks")
      .Build();

    var roicResponse = TradingViewResponseBuilder.CreateRoicResponse()
      .AddStock("VCB", "Vietcombank", 45.0m, 35.0m, 30.0m, 25.0m, 20.0m, 1.5m, 12.0m, 15.0m, 2.0m)
      .Build();

    // Act
    var result = _calculator.CalculateRankings(peResponse, roicResponse);

    // Assert
    Assert.Single(result);
    var vcb = result["VCB"];
    Assert.Equal("Vietcombank", vcb.Data.Name);
    Assert.Equal(50.5m, vcb.Data.Close);
    Assert.Equal(5.0m, vcb.Data.PriceEarningsTtm);
    Assert.Equal(45.0m, vcb.Data.GrossMarginTtm);
    Assert.Equal(15.0m, vcb.Data.ReturnOnInvestedCapitalFq);
  }

  [Fact]
  public void CombineRankings_PreservesIndividualRanks()
  {
    // Arrange
    var peResponse = TradingViewResponseBuilder.CreatePeResponse()
      .AddStock("VCB", "Vietcombank", 50.5m, 1.2m, 1000000L, 1.5m, 5000000000000m, "VND", 5.0m, 10000m, 10.5m, 2.5m, "Finance", "HOSE", "Banks")
      .Build();

    var roicResponse = TradingViewResponseBuilder.CreateRoicResponse()
      .AddStock("VCB", "Vietcombank", 45.0m, 35.0m, 30.0m, 25.0m, 20.0m, 1.5m, 12.0m, 15.0m, 2.0m)
      .Build();

    // Act
    var result = _calculator.CalculateRankings(peResponse, roicResponse);

    // Assert
    var vcb = result["VCB"];
    Assert.Equal(0, vcb.PeRank);
    Assert.Equal(0, vcb.RoicRank);
    Assert.Equal(0, vcb.CombinedRank);
  }

  #endregion

  #region JSON Value Conversion Tests

  [Fact]
  public void GetStringValue_ValidString_ReturnsString()
  {
    // Arrange
    var peResponse = TradingViewResponseBuilder.CreatePeResponse()
      .AddStock("VCB", "Vietcombank", 50.5m, 1.2m, 1000000L, 1.5m, 5000000000000m, "VND", 5.0m, 10000m, 10.5m, 2.5m, "Finance", "HOSE", "Banks")
      .Build();

    var roicResponse = TradingViewResponseBuilder.CreateRoicResponse()
      .AddStock("VCB", "Vietcombank", 45.0m, 35.0m, 30.0m, 25.0m, 20.0m, 1.5m, 12.0m, 15.0m, 2.0m)
      .Build();

    // Act
    var result = _calculator.CalculateRankings(peResponse, roicResponse);

    // Assert
    Assert.Equal("Vietcombank", result["VCB"].Data.Description);
    Assert.Equal("VND", result["VCB"].Data.FundamentalCurrencyCode);
    Assert.Equal("Finance", result["VCB"].Data.SectorTr);
  }

  [Fact]
  public void GetStringValue_NullValue_ReturnsNull()
  {
    // Arrange
    var peResponse = TradingViewResponseBuilder.CreatePeResponse()
      .AddStock("VCB", null, 50.5m, 1.2m, 1000000L, 1.5m, 5000000000000m, null, 5.0m, 10000m, 10.5m, 2.5m, null, "HOSE", null)
      .Build();

    var roicResponse = TradingViewResponseBuilder.CreateRoicResponse()
      .AddStock("VCB", "Vietcombank", 45.0m, 35.0m, 30.0m, 25.0m, 20.0m, 1.5m, 12.0m, 15.0m, 2.0m)
      .Build();

    // Act
    var result = _calculator.CalculateRankings(peResponse, roicResponse);

    // Assert
    var vcb = result["VCB"].Data;
    Assert.Null(vcb.Description);
    Assert.Null(vcb.FundamentalCurrencyCode);
    Assert.Null(vcb.SectorTr);
    Assert.Null(vcb.Sector);
  }

  [Fact]
  public void GetDecimalValue_ValidDecimal_ReturnsDecimal()
  {
    // Arrange
    var peResponse = TradingViewResponseBuilder.CreatePeResponse()
      .AddStock("VCB", "Vietcombank", 50.5m, 1.2m, 1000000L, 1.5m, 5000000000000m, "VND", 5.0m, 10000m, 10.5m, 2.5m, "Finance", "HOSE", "Banks")
      .Build();

    var roicResponse = TradingViewResponseBuilder.CreateRoicResponse()
      .AddStock("VCB", "Vietcombank", 45.0m, 35.0m, 30.0m, 25.0m, 20.0m, 1.5m, 12.0m, 15.0m, 2.0m)
      .Build();

    // Act
    var result = _calculator.CalculateRankings(peResponse, roicResponse);

    // Assert
    var vcb = result["VCB"].Data;
    Assert.Equal(50.5m, vcb.Close);
    Assert.Equal(1.2m, vcb.Change);
    Assert.Equal(5.0m, vcb.PriceEarningsTtm);
  }

  [Fact]
  public void GetDecimalValue_NullValue_ReturnsNull()
  {
    // Arrange
    var peResponse = TradingViewResponseBuilder.CreatePeResponse()
      .AddStock("VCB", "Vietcombank", null, null, 1000000L, 1.5m, 5000000000000m, "VND", null, null, null, null, "Finance", "HOSE", "Banks")
      .Build();

    var roicResponse = TradingViewResponseBuilder.CreateRoicResponse()
      .AddStock("VCB", "Vietcombank", 45.0m, 35.0m, 30.0m, 25.0m, 20.0m, 1.5m, 12.0m, 15.0m, 2.0m)
      .Build();

    // Act
    var result = _calculator.CalculateRankings(peResponse, roicResponse);

    // Assert
    var vcb = result["VCB"].Data;
    Assert.Null(vcb.Close);
    Assert.Null(vcb.Change);
    Assert.Null(vcb.PriceEarningsTtm);
  }

  [Fact]
  public void GetLongValue_ValidLong_ReturnsLong()
  {
    // Arrange
    var peResponse = TradingViewResponseBuilder.CreatePeResponse()
      .AddStock("VCB", "Vietcombank", 50.5m, 1.2m, 1000000L, 1.5m, 5000000000000m, "VND", 5.0m, 10000m, 10.5m, 2.5m, "Finance", "HOSE", "Banks")
      .Build();

    var roicResponse = TradingViewResponseBuilder.CreateRoicResponse()
      .AddStock("VCB", "Vietcombank", 45.0m, 35.0m, 30.0m, 25.0m, 20.0m, 1.5m, 12.0m, 15.0m, 2.0m)
      .Build();

    // Act
    var result = _calculator.CalculateRankings(peResponse, roicResponse);

    // Assert
    var vcb = result["VCB"].Data;
    Assert.Equal(1000000L, vcb.Volume);
  }

  [Fact]
  public void GetLongValue_NullValue_ReturnsNull()
  {
    // Arrange
    var peResponse = TradingViewResponseBuilder.CreatePeResponse()
      .AddStock("VCB", "Vietcombank", 50.5m, 1.2m, null, 1.5m, 5000000000000m, "VND", 5.0m, 10000m, 10.5m, 2.5m, "Finance", "HOSE", "Banks")
      .Build();

    var roicResponse = TradingViewResponseBuilder.CreateRoicResponse()
      .AddStock("VCB", "Vietcombank", 45.0m, 35.0m, 30.0m, 25.0m, 20.0m, 1.5m, 12.0m, 15.0m, 2.0m)
      .Build();

    // Act
    var result = _calculator.CalculateRankings(peResponse, roicResponse);

    // Assert
    Assert.Null(result["VCB"].Data.Volume);
  }

  #endregion
}
