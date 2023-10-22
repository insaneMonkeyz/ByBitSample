using MarketDataProvider.BybitApi.DTO.Stream;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;

namespace MarketDataProvider;

internal class BybitMessageRouter
{
    public event Action<RequestResponseMessage>? OnRequestResponseMessage;
    public event Action<StreamMessage<SpotUpdate>>? OnSpotSecurityUpdated;

    private JSchema? _spotSecurityUpdated;
    public JSchema SpotSecurityUpdated
    {
        get
        {
            return _spotSecurityUpdated ??= 
                _schemaGenerator.Generate(typeof(StreamMessage<SpotUpdate>));
        }
    }

    private JSchema? _requestResponseSchema;
    public JSchema RequestResponseSchema
    {
        get
        {
            return _requestResponseSchema ??= 
                _schemaGenerator.Generate(typeof(RequestResponseMessage));
        }
    }

    public BybitMessageRouter(IDataTransmitter dataTransmitter)
    {
        _dataTransmitter = dataTransmitter;
        _dataTransmitter.ServerReply += OnServerReply;
    }

    private void OnServerReply(object? sender, object arg)
    {
        try
        {
            if (arg is not JObject jobj)
            {
                if (arg is string stringMsg)
                {
                    jobj = JObject.Parse(stringMsg);
                }
                else
                {
                    return;
                }
            }

            if (jobj.Is<RequestResponseMessage>(RequestResponseSchema, out var requestResponseMsg))
            {
                OnRequestResponseMessage?.Invoke(requestResponseMsg);
                return;
            }

            if (jobj.Is<StreamMessage<SpotUpdate>>(SpotSecurityUpdated, out var spotUpdate))
            {
                OnSpotSecurityUpdated?.Invoke(spotUpdate);
                return;
            }
        }
        catch (Exception)
        {
        }
    }

    private readonly IDataTransmitter _dataTransmitter;
    private readonly JSchemaGenerator _schemaGenerator = new();
}
