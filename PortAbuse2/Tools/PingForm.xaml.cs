using System.Threading.Tasks;
using System.Windows;
using LiveChartsCore.SkiaSharpView;
using PortAbuse2.Core.Result;
using PortAbuse2.Ping;
using static PortAbuse2.Core.Ip.PingWorker;

namespace PortAbuse2.Tools;

/// <summary>
///     Interaction logic for PingForm.xaml
/// </summary>
public partial class PingForm : Window
{
    private readonly ConnectionInformation _connectionInformation;
    private readonly PingContext _context;

    public PingForm(ConnectionInformation connectionInformation)
    {
        _connectionInformation = connectionInformation;
        _context = new PingContext();
        DataContext = _context;
        _context.Target = connectionInformation.ShowIp;
        InitializeComponent();
        Loaded += OnLoaded;

        Chart.XAxes = new[]
        {
            new Axis
            {
                IsVisible = false
            }
        };
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Task.Run(StartTrace);
    }

    private async Task StartTrace()
    {
        _context.IsRunning = true;
        await StartPing(_connectionInformation.ShowIp, _context, NewPingReceived, 2000);
    }

    private void NewPingReceived(PingInfo pingInfo)
    {
        Dispatcher.Invoke(() =>
        {
            _context.Items.Add(pingInfo.Value);
            _context.Min = pingInfo.Min;
            _context.Max = pingInfo.Max;
            _context.Avg = pingInfo.Average;
            _context.LossPercentage = pingInfo.LossPercentage;
        });
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        _context.IsRunning = false;
    }
}