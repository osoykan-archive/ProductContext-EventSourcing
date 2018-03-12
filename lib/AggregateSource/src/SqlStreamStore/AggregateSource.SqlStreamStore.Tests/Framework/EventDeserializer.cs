using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SqlStreamStore.Streams;

namespace AggregateSource.SqlStreamStore.Tests
{
    public class EventDeserializer : IEventDeserializer
    {
        public async Task<object> DeserializeAsync(StreamMessage rawMessage)
        {
            return JsonConvert.DeserializeObject(await rawMessage.GetJsonData(), Type.GetType(rawMessage.Type, true));
        }
    }
}