using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace PortAbuse2.Common
{
    public class Network
    {
        public static async Task GetTraceRouteAsync(IPAddress address, Action<TraceResponse> newTraceHopFound, int timeout = 10000)
        {
            const int maxHops = 30;
            const int bufferSize = 32;

            var buffer = new byte[bufferSize];
            new Random().NextBytes(buffer);

            using (var ping = new Ping())
            {
                for (var ttl = 1; ttl <= maxHops; ttl++)
                {
                    var options = new PingOptions(ttl, false);
                    var reply = await ping.SendPingAsync(address, timeout, buffer, options);
                    newTraceHopFound(new TraceResponse(reply.Status, reply.Address));

                    // if we reach a status other than expired or timed out, we're done searching or there has been an error
                    if (reply.Status != IPStatus.TtlExpired && reply.Status != IPStatus.TimedOut)
                        break;
                }
            }

        }

        public static async Task<long> GetPingAsync(IPAddress address, int timeout = 10000)
        {
            using (var ping = new Ping())
            {
                try
                {
                    var reply = await ping.SendPingAsync(address, timeout);
                    return IsPingFailed(reply) ? -1 : reply.RoundtripTime;
                }
                catch
                {
                    return -1;
                }

            }
        }

        private static bool IsPingFailed(PingReply reply)
        {
            return reply.Status == IPStatus.TimedOut || reply.Status == IPStatus.TimeExceeded;
        }
    }
}