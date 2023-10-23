using MarketDataProvider.BybitApi.DTO.Stream;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;

namespace MarketDataProvider;

internal class BybitMessageRouter
{
    public event Action<RequestResponseMessage>? RequestResponseMessage;
    public event Action<StreamMessage<SpotUpdate>>? SpotSecurityUpdated;
    public event Action<StreamMessage<TradeDescription>>? NewTrade;

    private JSchema? _tradeSchema;
    public JSchema TradeSchema
    {
        get
        {
            return _tradeSchema ??=
                _schemaGenerator.Generate(typeof(StreamMessage<TradeDescription>));
        }
    }

    private JSchema? _spotSecurityUpdatedSchema;
    public JSchema SpotSecurityUpdatedSchema
    {
        get
        {
            return _spotSecurityUpdatedSchema ??= 
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
                RequestResponseMessage?.Invoke(requestResponseMsg);
                return;
            }

            if (jobj.Is<StreamMessage<TradeDescription>>(TradeSchema, out var newtrade))
            {
                NewTrade?.Invoke(newtrade);
                return;
            }

            if (jobj.Is<StreamMessage<SpotUpdate>>(SpotSecurityUpdatedSchema, out var spotUpdate))
            {
                SpotSecurityUpdated?.Invoke(spotUpdate);
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
