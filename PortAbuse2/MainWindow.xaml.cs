using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PortAbuse2.Applications;
using PortAbuse2.Common;
using PortAbuse2.Core;
using PortAbuse2.Core.Ip;
using PortAbuse2.Core.Port;

namespace PortAbuse2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly ObservableCollection<AppEntry> _allAppWithPorts = new ObservableCollection<AppEntry>();

        public MainWindow()
        {
            InitializeComponent();

            Admin.CheckAdmin();

            LoadInterfaces();

            AppListComboBox.ItemsSource = _allAppWithPorts;

            Task.Run(RefreshLoadProceses);
        }

        private async Task RefreshLoadProceses()
        {
            var apps = await AppList.GetRunningApplications();
            foreach (var p in apps)
            {
                if (_allAppWithPorts.All(x => x.InstancePid != p.InstancePid))
                {
                    await Dispatcher.BeginInvoke(new ThreadStart(delegate
                    {
                        p.Icon = AppIcon.GetIcon(p.FullName, false);
                        _allAppWithPorts.Add(p);
                    }));
                }
            }
        }

        private void LoadInterfaces()
        {
            var interfaces = IpInterface.GetIpInterfaces();
            InterfaceBox.ItemsSource = interfaces;
        }

        private void RefreshButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _allAppWithPorts.Clear();
            Task.Run(RefreshLoadProceses);
        }
    }
}
