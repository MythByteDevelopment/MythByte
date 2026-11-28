using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace MythByte.Shared.Maps;

internal static class TiledJsonWriter
{
    private static readonly JsonSerializerOptions ValueOptions = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    public static string Write(JsonNode root)
    {
        var builder = new StringBuilder();
        WriteNode(builder, root, 0, null);
        builder.Append('\n');
        return builder.ToString();
    }

    private static void WriteNode(StringBuilder builder, JsonNode? node, int indent, int? rowWidth)
    {
        switch (node)
        {
            case JsonObject obj:
                WriteObject(builder, obj, indent);
                break;
            case JsonArray array when rowWidth is > 0 && array.All(v => v is JsonValue):
                WriteRows(builder, array, indent, rowWidth.Value);
                break;
            case JsonArray array:
                WriteArray(builder, array, indent);
                break;
            default:
                builder.Append(node?.ToJsonString(ValueOptions) ?? "null");
                break;
        }
    }

    private static void WriteObject(StringBuilder builder, JsonObject obj, int indent)
    {
        if (obj.Count == 0)
        {
            builder.Append("{}");
            return;
        }

        builder.Append("{\n");
        var first = true;
        foreach (var (name, value) in obj)
        {
            if (!first)
                builder.Append(",\n");

            first = false;
            builder.Append(' ', indent + 1).Append(JsonValue.Create(name)!.ToJsonString(ValueOptions)).Append(": ");
            var rowWidth = name == "data" && obj["width"] is JsonValue width && width.TryGetValue<int>(out var columns) ? columns : (int?)null;
            WriteNode(builder, value, indent + 1, rowWidth);
        }

        builder.Append('\n').Append(' ', indent).Append('}');
    }

    private static void WriteArray(StringBuilder builder, JsonArray array, int indent)
    {
        if (array.Count == 0)
        {
            builder.Append("[]");
            return;
        }

        builder.Append("[\n");
        for (var i = 0; i < array.Count; i++)
        {
            if (i > 0)
                builder.Append(",\n");

            builder.Append(' ', indent + 1);
            WriteNode(builder, array[i], indent + 1, null);
        }

        builder.Append('\n').Append(' ', indent).Append(']');
    }

    private static void WriteRows(StringBuilder builder, JsonArray array, int indent, int rowWidth)
    {
        builder.Append("[\n");
        for (var i = 0; i < array.Count; i++)
        {
            if (i % rowWidth == 0)
                builder.Append(' ', indent + 1);

            builder.Append(array[i]?.ToJsonString(ValueOptions) ?? "null");

            if (i < array.Count - 1)
                builder.Append(i % rowWidth == rowWidth - 1 ? ",\n" : ",");
        }

        builder.Append('\n').Append(' ', indent).Append(']');
    }
}
