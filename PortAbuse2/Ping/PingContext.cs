using System.Collections.ObjectModel;
using System.Net;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using PortAbuse2.Core.Common;
using SkiaSharp;

namespace PortAbuse2.Ping;

public class PingContext : RunnableContext
{
    private ObservableCollection<long> _items = new();
    private IPAddress _target;

    public PingContext()
    {
        // create a series with the data 
        Series = new ISeries[]
        {
            new LineSeries<long>
            {
                Values = _items,
                GeometryFill = null,
                GeometryStroke = null,
                LineSmoothness = 0,
                Stroke = null,
                Fill = new SolidColorPaint(SKColors.LightBlue, 1)
            }
        };
    }

    public ObservableCollection<long> Items
    {
        get => _items;
        set
        {
            _items = value;
            OnPropertyChanged();
        }
    }

    public IPAddress Target
    {
        get => _target;
        set
        {
            _target = value;
            OnPropertyChanged();
        }
    }


    public ISeries[] Series { get; set; }
}