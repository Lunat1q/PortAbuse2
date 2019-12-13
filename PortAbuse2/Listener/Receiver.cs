using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using PortAbuse2.Core.Listener;
using PortAbuse2.Core.Result;

namespace PortAbuse2.Listener
{
    internal class Receiver : CoreReceiver
    {
        private readonly IResultReceiver _receiver;
        private readonly Dispatcher _dispatcher;

        public Receiver(IResultReceiver receiver, Dispatcher dispatcher, bool minimizeHostname = false, bool hideOld = false, bool hideSmall = false) : base (receiver, minimizeHostname, hideOld,hideSmall)
        {
            this._receiver = receiver;
            this._dispatcher = dispatcher;
            this.InvokedAdd = this.Add;
        }

        private async Task<bool> Add(ConnectionInformation info)
        {
            await this._dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new ThreadStart(delegate { this._receiver.AddAsync(info); })
            );
            
            return true;
        }
    }
}