using MarketDataProvider.Bybit.Rest;
using MarketDataProvider.BybitApi;
using MarketDataProvider.BybitApi.DTO.Stream;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;

namespace MarketDataProvider
{
    internal class HeartbeatProvider : IHeartbeatProvider
    {
        private JSchema? _heartBeatSchema;
        public JSchema? HeartBeatSchema 
        { 
            get
            {
                return _heartBeatSchema ??= 
                    new JSchemaGenerator().Generate(typeof(RequestResponseMessage));
            } 
        }

        private readonly RequestResponseMessage _msg = new();
        private readonly StreamRequestFactory _requestFactory;

        public HeartbeatProvider(StreamRequestFactory requestFactory)
        {
            _requestFactory = requestFactory;
        }

        public bool IsHeartbeatReply(object message)
        {
            try
            {
                if (message is not JObject jobj)
                {
                    if (message is string stringMsg)
                    {
                        jobj = JObject.Parse(stringMsg);
                    }
                    else
                    {
                        return false; 
                    }
                }

                var operation = jobj.Value<string>("op");

                return 
                    StreamOperations
                        .HeartbeatReply
                        .Equals(operation, StringComparison.CurrentCultureIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        public object GetNextMessage() => _requestFactory.CreateHeartbeatMessage();
    }
}
