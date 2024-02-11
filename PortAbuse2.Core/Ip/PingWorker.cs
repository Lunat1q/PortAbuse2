using System;
using System.Net;
using System.Threading.Tasks;
using PortAbuse2.Core.Common;
using PortAbuse2.Core.Trace;

namespace PortAbuse2.Core.Ip;

public sealed class PingWorker
{
    private const int TimeBetweenPings = 300;

    public static async void GetPingStats(TraceEntry traceEntry, int numberOfPings = 10, int timeout = 10000)
    {
        traceEntry.Latency.InProgress = true;
        traceEntry.Latency.Executed = true;

        var minValue = long.MaxValue;
        var maxValue = long.MinValue;
        var total = 0L;

        for (var i = 0; i < numberOfPings; i++)
        {
            var pingValue = await Network.GetPingAsync(traceEntry.Address, timeout);
            if (minValue > pingValue)
            {
                minValue = pingValue;
            }

            if (maxValue < pingValue)
            {
                maxValue = pingValue;
            }

            total += pingValue;
        }

        if (total < 0)
        {
            traceEntry.Latency.Failed = true;
        }

        traceEntry.Latency.Max = maxValue;
        traceEntry.Latency.Min = minValue;
        traceEntry.Latency.Average = total / numberOfPings;

        traceEntry.Latency.InProgress = false;
    }

    public static async Task StartPing(IPAddress ipAddress,
                                       RunnableContext context,
                                       Action<PingInfo> handleNewPing,
                                       int timeout = 10000)
    {
        var minValue = long.MaxValue;
        var maxValue = long.MinValue;
        var total = 0L;
        var totalCount = 0L;
        var timeoutCount = 0L;
        var valueReceived = false;

        while (context.IsRunning)
        {
            totalCount++;

            var pingValue = await Network.GetPingAsync(ipAddress, timeout);
            if (pingValue == -1)
            {
                timeoutCount++;
            }
            else
            {
                valueReceived = true;
                if (minValue > pingValue)
                {
                    minValue = pingValue;
                }

                if (maxValue < pingValue)
                {
                    maxValue = pingValue;
                }
            }

            var lossPercentage = (double)timeoutCount * 100 / totalCount;

            if (valueReceived)
            {
                var avgValue = (double)total / totalCount;
                handleNewPing(new PingInfo(pingValue, minValue, maxValue, avgValue, lossPercentage));
            }
            else
            {
                handleNewPing(new PingInfo(pingValue, 0L, 0L, 0.0, lossPercentage));
            }

            var nextWait = TimeBetweenPings - (int)pingValue;
            await Task.Delay(Math.Max(nextWait, 0));

            total += pingValue;
        }
    }

    public struct PingInfo
    {
        public PingInfo(long value, long min, long max, double average, double lossPercentage)
        {
            Value = value;
            LossPercentage = lossPercentage;
            Min = min;
            Max = max;
            Average = average;
        }

        public long Value { get; }

        public double LossPercentage { get; }

        public long Min { get; }

        public long Max { get; }

        public double Average { get; }
    }
}