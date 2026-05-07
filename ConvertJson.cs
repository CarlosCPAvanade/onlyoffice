private static object? ConvertJson(JsonElement element)
{
    switch (element.ValueKind)
    {
        case JsonValueKind.Object:
            var dict = new Dictionary<string, object?>();
            foreach (var prop in element.EnumerateObject())
            {
                dict[prop.Name] = ConvertJson(prop.Value);
            }
            return dict;

        case JsonValueKind.Array:
            var list = new List<object?>();
            foreach (var item in element.EnumerateArray())
            {
                list.Add(ConvertJson(item));
            }
            return list;

        case JsonValueKind.String:
            return element.GetString();

        case JsonValueKind.Number:
            if (element.TryGetInt64(out var l)) return l;
            if (element.TryGetDouble(out var d)) return d;
            return null;

        case JsonValueKind.True:
            return true;

        case JsonValueKind.False:
            return false;

        default:
            return null;
    }
}