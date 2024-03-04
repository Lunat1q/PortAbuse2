﻿using System.Collections.ObjectModel;
using System.Net;
using LiveChartsCore;
using LiveChartsCore.ConditionalDraw;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using PortAbuse2.Core.Common;
using SkiaSharp;

namespace PortAbuse2.Ping;

public class PingContext : RunnableContext
{
    private ObservableCollection<long> _items = new();
    private IPAddress _target;
    private long _min;
    private long _max;
    private double _avg;
    private double _lossPercentage;

    public PingContext()
    {
        // create a series with the data 
        Series = new ISeries[]
        {
            new LineSeries<long>
            {
                Values = _items,
                GeometryFill =  new SolidColorPaint(SKColors.Red, 0),
                GeometryStroke = null,
                GeometrySize = 0,
                LineSmoothness = 0,
                Stroke = null,
                Fill = new SolidColorPaint(SKColors.LightBlue, 1)
            }
            .OnPointMeasured(point =>
            {
                if (point.PrimaryValue == -1)
                {
                    point.Visual.Fill = new SolidColorPaint(SKColors.Red, 1);
                    point.Visual.Width = 10;
                    point.Visual.Height = 10;
                }
            })
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

    public long Min
    {
        get => _min;
        set
        {
            _min = value;
            OnPropertyChanged();
        }
    }

    public long Max
    {
        get => _max;
        set
        {
            _max = value;
            OnPropertyChanged();
        }
    }

    public double Avg
    {
        get => _avg;
        set
        {
            _avg = value;
            OnPropertyChanged();
        }
    }

    public double LossPercentage
    {
        get => _lossPercentage;
        set
        {
            _lossPercentage = value;
            OnPropertyChanged();
        }
    }



    public ISeries[] Series { get; set; }
}