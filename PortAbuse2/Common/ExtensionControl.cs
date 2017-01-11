using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PortAbuse2.Core.ApplicationExtensions;
using PortAbuse2.Core.Listener;

namespace PortAbuse2.Common
{
    internal class ExtensionControl
    {
        private readonly List<IApplicationExtension> _extensionList = new List<IApplicationExtension>();
        private IApplicationExtension _currentExt;

        internal ExtensionControl(CoreReceiver receiver)
        {
            var wfapp = new WarframeApp();
            _extensionList.Add(wfapp);
            wfapp.Receiver = receiver;
        }

        public void AppSelected(string name)
        {
            Stop();

            foreach (var ext in _extensionList)
            {
                if (ext.AppNames.All(x => x != name)) continue;
                _currentExt = ext;
                ext.Start();
                break;
            }
        }

        public void Stop()
        {
            _currentExt?.Stop();
        }
    }
}
