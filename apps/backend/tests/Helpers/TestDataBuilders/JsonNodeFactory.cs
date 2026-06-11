using System.Text.Json;
using System.Text.Json.Nodes;

namespace Stocker.Tests.Helpers.TestDataBuilders;

public static class JsonNodeFactory
{
  public static JsonArray CreateJsonArray(params object?[] values)
  {
    var array = new JsonArray();
    foreach (var value in values)
    {
      array.Add(CreateNode(value));
    }
    return array;
  }

  public static JsonNode? CreateNode(object? value)
  {
    return value switch
    {
      null => JsonValue.Create((object?)null),
      string s => JsonValue.Create(s),
      decimal d => JsonValue.Create(d),
      double d => JsonValue.Create(d),
      float f => JsonValue.Create(f),
      long l => JsonValue.Create(l),
      int i => JsonValue.Create(i),
      short s => JsonValue.Create(s),
      byte b => JsonValue.Create(b),
      bool b => JsonValue.Create(b),
      _ => throw new ArgumentException($"Unsupported type: {value.GetType()}")
    };
  }

  public static JsonNode CreateStringNode(string? value)
  {
    return JsonValue.Create(value ?? (object?)null);
  }

  public static JsonNode CreateDecimalNode(decimal? value)
  {
    return JsonValue.Create(value ?? (object?)null);
  }

  public static JsonNode CreateLongNode(long? value)
  {
    return JsonValue.Create(value ?? (object?)null);
  }

  public static JsonNode CreateNullNode()
  {
    return JsonValue.Create((object?)null);
  }
}
