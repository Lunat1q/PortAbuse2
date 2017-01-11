using System.Linq;
using System.Threading;
using System.Windows;
using PortAbuse2.Core.Listener;

namespace PortAbuse2.Listener
{
    internal class Receiver : CoreReceiver
    {
        private readonly Window _window;

        public Receiver(Window window, bool minimizeHostname = false, bool hideOld = false, bool hideSmall = false) : base (minimizeHostname, hideOld,hideSmall)
        {
            _window = window;

            InvokedAdd = (ro) =>
            {
                _window.Dispatcher.BeginInvoke(new ThreadStart(delegate
                {
                    if (ResultObjects.All(x => x.ShowIp != ro.ShowIp))
                        ResultObjects.Add(ro);
                }));
                return true;
            };
        }
    }
}