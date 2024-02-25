using System;
using System.Threading;

namespace PortAbuse2.Core.Common;

public class ExThread
{
    private readonly CancellationTokenSource _tokenSource;

    public ExThread(Thread td, CancellationTokenSource tokenSource, DateTime tdTime = new())
    {
        _tokenSource = tokenSource;
        Td = td;
        TdTime = tdTime;
    }

    public Thread Td { get; }

    public DateTime TdTime { get; }

    public void Abort()
    {
        _tokenSource.Cancel();
    }
}