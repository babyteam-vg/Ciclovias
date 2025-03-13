using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using UnityEngine;

public class Vector2Converter : JsonConverter<Vector2>
{
    public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer)
    {
        JObject obj = new JObject
        {
            { "x", value.x },
            { "y", value.y }
        };
        obj.WriteTo(writer);
    }

    public override Vector2 ReadJson(JsonReader reader, System.Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JObject obj = JObject.Load(reader);
        return new Vector2((float)obj["x"], (float)obj["y"]);
    }
}

public class Vector2IntConverter : JsonConverter<Vector2Int>
{
    public override void WriteJson(JsonWriter writer, Vector2Int value, JsonSerializer serializer)
    {
        JObject obj = new JObject
        {
            { "x", value.x },
            { "y", value.y }
        };
        obj.WriteTo(writer);
    }

    public override Vector2Int ReadJson(JsonReader reader, System.Type objectType, Vector2Int existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JObject obj = JObject.Load(reader);
        return new Vector2Int((int)obj["x"], (int)obj["y"]);
    }
}

public class Vector3Converter : JsonConverter<Vector3>
{
    public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
    {
        JObject obj = new JObject
        {
            { "x", value.x },
            { "y", value.y },
            { "z", value.z }
        };
        obj.WriteTo(writer);
    }

    public override Vector3 ReadJson(JsonReader reader, System.Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JObject obj = JObject.Load(reader);
        return new Vector3((float)obj["x"], (float)obj["y"], (float)obj["z"]);
    }
}

public class Vector3IntConverter : JsonConverter<Vector3Int>
{
    public override void WriteJson(JsonWriter writer, Vector3Int value, JsonSerializer serializer)
    {
        JObject obj = new JObject
        {
            { "x", value.x },
            { "y", value.y },
            { "z", value.z }
        };
        obj.WriteTo(writer);
    }

    public override Vector3Int ReadJson(JsonReader reader, System.Type objectType, Vector3Int existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JObject obj = JObject.Load(reader);
        return new Vector3Int((int)obj["x"], (int)obj["y"], (int)obj["z"]);
    }
}
