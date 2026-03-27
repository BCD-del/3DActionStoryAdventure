using Newtonsoft.Json;

namespace Assets._Project.Develop.Runtime.Utilites.DataManagment.Serializers
{
    public class JsonSerializer : IDataSerializer
    {
        public TData Deserialize<TData>(string serializedData)
        {
            return JsonConvert.DeserializeObject<TData>(serializedData, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
        }

        public string Serialize<TData>(TData data)
        {
            return JsonConvert.SerializeObject(data, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented, // for tests, Formatting.None for 1 string in prod
                TypeNameHandling = TypeNameHandling.Auto // write types only if needed
            });
        }
    }
}
