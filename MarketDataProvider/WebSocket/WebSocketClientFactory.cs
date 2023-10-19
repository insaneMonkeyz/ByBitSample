using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketDataProvider.WebSocket
{
    public class WebSocketClientFactory : IAbstractWebSocketFactory
    {
        public IWebSocketClient CreateWebSocketClient() => new WebSocketAdapter();
    }
}
