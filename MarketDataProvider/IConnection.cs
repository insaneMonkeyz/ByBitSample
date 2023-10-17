using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketDataProvider
{
    public interface IConnection
    {
        event EventHandler<ConnectionState> ConnectionStateChanged;
        ConnectionState ConnectionState { get; }
        Task ConnectAsync(ConnectionParameters parameters, CancellationToken cancellationToken);
        Task DisconnectAsync(CancellationToken cancellationToken);
    }
}
