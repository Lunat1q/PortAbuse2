using System.Collections.ObjectModel;
using System.Net;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using PortAbuse2.Core.Common;

namespace PortAbuse2.Ping
{
    public class PingContext : RunnableContext
    {
        private ObservableCollection<long> _items = new ObservableCollection<long>();
        private IPAddress _target;
        private bool _isComplete;

        public ObservableCollection<long> Items
        {
            get => this._items;
            set
            {
                this._items = value;
                this.OnPropertyChanged();
            }
        }

        public IPAddress Target
        {
            get => this._target;
            set
            {
                this._target = value;
                this.OnPropertyChanged();
            }
        }

        

        public ISeries[] Series { get; set; }

        public PingContext()
        {
            // create a series with the data 
            Series = new ISeries[]
            {
            new LineSeries<long>
            {
                Values = this._items,
                GeometryFill = null,
                GeometryStroke = null,
                LineSmoothness = 0,
                Stroke = null,
                Fill = new SolidColorPaint(SKColors.LightBlue, 1)
            }
            };
        }
    }
}




