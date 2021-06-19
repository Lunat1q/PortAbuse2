using PortAbuse2.Common;
using PortAbuse2.Core.Trace;

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
    }
}