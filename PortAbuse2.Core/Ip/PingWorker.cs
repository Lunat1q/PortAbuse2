using PortAbuse2.Common;
using PortAbuse2.Core.Common;
using PortAbuse2.Core.Trace;
using System;
using System.Net;
using System.Threading.Tasks;

namespace PortAbuse2.Core.Ip
{
    public sealed class PingWorker
    {
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
        public static async Task StartPing(IPAddress ipAddress, RunnableContext context, Action<long, long, long, double, double> handleNewPing, int timeout = 10000)
        {

            var minValue = long.MaxValue;
            var maxValue = long.MinValue;
            var avgValue = 0.0;
            var failedPercent = 0.0;
            var total = 0L;
            var totalCount = 0L;
            var failedCount = 0L;

            while (context.IsRunning)
            {
                totalCount++;
                var pingValue = await Network.GetPingAsync(ipAddress, timeout);
                if (pingValue == -1)
                {
                    failedCount++;
                }
                else
                {
                    if (minValue > pingValue)
                    {
                        minValue = pingValue;
                    }

                    if (maxValue < pingValue)
                    {
                        maxValue = pingValue;
                    }
                }
                avgValue = Math.Round((double)total / totalCount, 1);
                failedPercent = Math.Round((double) failedCount * 100 / totalCount, 1);

                handleNewPing(pingValue, minValue, maxValue, avgValue, failedPercent);
                var nextWait = 300 - (int) pingValue;
                await Task.Delay(Math.Max(nextWait, 0));

                total += pingValue;
            }

        }
    }
}