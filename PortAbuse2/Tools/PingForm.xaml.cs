using System.Threading.Tasks;
using System.Windows;
using LiveChartsCore.SkiaSharpView;
using PortAbuse2.Common;
using PortAbuse2.Core.Geo;
using PortAbuse2.Core.Ip;
using PortAbuse2.Core.Result;
using PortAbuse2.Core.Trace;
using PortAbuse2.Ping;

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

            Chart.XAxes =  new[]
            {
                new Axis
                {
                    IsVisible = false
                }
            };
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

        private void NewPingReceived(long pingValue, long minValue, long maxValue, double avgValue, double failedPercent)
        {
            this.Dispatcher.Invoke(() =>
            {
                this._context.Items.Add(pingValue);
                this._context.Min = minValue;
                this._context.Max = maxValue;
                this._context.Avg = avgValue;
                this._context.FailedPercent = failedPercent;
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _context.IsRunning = false;
        }
    }
}
