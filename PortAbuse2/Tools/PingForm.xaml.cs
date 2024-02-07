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
    /// Interaction logic for PingForm.xaml
    /// </summary>
    public partial class PingForm : Window
    {
        private readonly ConnectionInformation _connectionInformation;
        private int _addressIndex = 1;
        private readonly PingContext _context;

        public PingForm(ConnectionInformation connectionInformation)
        {
            this._connectionInformation = connectionInformation;
            this._context = new PingContext();
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
            this._context.IsRunning = true;
            await PingWorker.StartPing(this._connectionInformation.ShowIp, this._context, NewPingReceived, 2000);
        }

        private void NewPingReceived(PingEntry pingEntry)
        {
            this.Dispatcher.Invoke(() =>
            {
                this._context.Items.Add(pingEntry);
            });
        }
    }
}
