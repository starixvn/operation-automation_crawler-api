using Newtonsoft.Json;

namespace Tennis.Extensions;

public static class JsonExtensions
{
    public static T TryDeserializeObject<T>(string json)
    {
        try
        {
            var result = JsonConvert.DeserializeObject<T>(json);
            if (result == null)
            {
                throw new JsonSerializationException($"Deserialization returned null. JSON: {json}");
            }
            return result;
        }
        catch (JsonSerializationException ex)
        {
            throw new JsonSerializationException($"Failed to deserialize JSON to type {typeof(T).Name}. JSON: {json}", ex);
        }
        catch (Exception ex)
        {
            throw new JsonSerializationException($"Unexpected error while deserializing JSON to type {typeof(T).Name}. JSON: {json}", ex);
        }
    }
}
