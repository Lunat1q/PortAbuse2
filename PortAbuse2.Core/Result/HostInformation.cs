using PortAbuse2.Core.Common;

namespace PortAbuse2.Core.Result
{
    public abstract class HostInformation : PaNotified
    {
        private GeoData? _geo;
        private LatencyStats? _latency;

        public GeoData Geo
        {
            get => this._geo ??= new GeoData();
            set
            {
                this._geo = value;
                this.OnPropertyChanged();
            }
        }

        public LatencyStats Latency
        {
            get => this._latency ??= new LatencyStats();
            set
            {
                this._latency = value; 
                this.OnPropertyChanged();
            }
        }
    }

    public class LatencyStats : PaNotified
    {
        private long _min;
        private long _max;
        private long _average;
        private bool _inProgress;
        private bool _executed;
        private bool _failed;

        public bool InProgress
        {
            get => this._inProgress;
            set
            {
                this._inProgress = value; 
                this.OnPropertyChanged();
            }
        }

        public bool Executed
        {
            get => this._executed;
            set
            {
                this._executed = value; 
                this.OnPropertyChanged();
            }
        }

        public long Min
        {
            get => this._min;
            set
            {
                this._min = value;
                this.OnPropertyChanged();
            }
        }

        public long Max
        {
            get => this._max;
            set
            {
                this._max = value;
                this.OnPropertyChanged();
            }
        }

        public long Average
        {
            get => this._average;
            set
            {
                this._average = value;
                this.OnPropertyChanged();
            }
        }

        public bool Failed
        {
            get => this._failed;
            set
            {
                this._failed = value; 
                this.OnPropertyChanged();
            }
        }
    }
}