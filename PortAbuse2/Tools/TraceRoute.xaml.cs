using System.Threading.Tasks;
using System.Windows;
using PortAbuse2.Common;
using PortAbuse2.Core.Geo;
using PortAbuse2.Core.Ip;
using PortAbuse2.Core.Result;
using PortAbuse2.Core.Trace;

namespace PortAbuse2.Tools
{
    /// <summary>
    /// Interaction logic for TraceRoute.xaml
    /// </summary>
    public partial class TraceRoute : Window
    {
        private readonly ConnectionInformation _connectionInformation;
        private int _addressIndex = 1;
        private readonly TraceContext _context;

        public TraceRoute(ConnectionInformation connectionInformation)
        {
            this._connectionInformation = connectionInformation;
            this._context = new TraceContext();
            this.DataContext = this._context;
            this._context.Target = connectionInformation.ShowIp;
            InitializeComponent();
            this.Loaded += this.OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Task.Run(this.StartTrace);
        }

        private async Task StartTrace()
        {
            await Network.GetTraceRouteAsync(this._connectionInformation.ShowIp, this.NewIpDiscovered, 2000);
            this.TraceDone();
        }

        private void TraceDone()
        {
            this._context.IsComplete = true;
        }

        private void NewIpDiscovered(TraceResponse response)
        {
            this.Dispatcher.Invoke(() =>
            {
                var traceEntry = new TraceEntry(this._addressIndex++, response.Address);
                if (response.IsOk())
                {
                    GeoWorker.InsertGeoDataQueue(traceEntry);
                    DnsHost.FillIpHost(traceEntry);
                    Task.Run(() => PingWorker.GetPingStats(traceEntry, 10, 2000));
                }

                this._context.Items.Add(traceEntry);
            });
        }
    }
}
