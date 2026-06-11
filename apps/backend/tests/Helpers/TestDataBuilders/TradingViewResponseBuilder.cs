using System.Text.Json.Nodes;
using Stocker.Features.Stock.StockRanking;
using Stocker.Tests.Helpers.TestDataBuilders;

namespace Stocker.Tests.Helpers.TestDataBuilders;

public class TradingViewResponseBuilder
{
  private readonly List<StockDataPoint> _dataPoints = new();

  public TradingViewResponseBuilder AddStock(string identifier, params object?[] values)
  {
    var dataArray = JsonNodeFactory.CreateJsonArray(values);

    _dataPoints.Add(new StockDataPoint
    {
      Data = dataArray,
      StockIdentifier = identifier
    });

    return this;
  }

  public TradingViewResponseBuilder AddStockWithData(string identifier, JsonArray dataArray)
  {
    _dataPoints.Add(new StockDataPoint
    {
      Data = dataArray,
      StockIdentifier = identifier
    });

    return this;
  }

  public TradingViewResponse Build()
  {
    return new TradingViewResponse
    {
      TotalCount = _dataPoints.Count,
      Data = _dataPoints.ToArray()
    };
  }

  public static TradingViewResponseBuilder CreatePeResponse()
  {
    return new TradingViewResponseBuilder();
  }

  public static TradingViewResponseBuilder CreateRoicResponse()
  {
    return new TradingViewResponseBuilder();
  }
}
