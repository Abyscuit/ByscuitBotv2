using Newtonsoft.Json;

namespace ByscuitBotv2.Util
{
    public static class ObjectCloner
    {
        
        // https://stackoverflow.com/questions/78536/deep-cloning-objects
        public static T Clone<T>(this T source)
        {
            // Don't serialize a null object, simply return the default for that object
            if (ReferenceEquals(source, null))
            {
                return default(T);
            }
            
            var deserializeSettings = new JsonSerializerSettings
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace
            };

            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source), deserializeSettings);
        }
        
    }
}
