using System;

namespace Utilizr.Util;

public class BandwidthUsage: EventArgs
{
    private DateTime _lastUpdate = DateTime.UtcNow;
    public long BytesSent { get; private set; }
    public long BytesRecieved { get; private set; }

    private long _unaccountedRx;
    private long _unaccountedTx;

    public long TxBytesPerSecond { get; set; }
    public long RxBytesPerSecond { get; set; }

    public double MinDurationSeconds { get; set; } = 1;

    public void Update(long totalBytesSend, long totalBytesRecieved)
    {
        var newTime = DateTime.UtcNow;
        var secsPassed = (newTime - _lastUpdate).TotalSeconds;
        var outDiff = totalBytesSend - BytesSent;
        var inDiff = totalBytesRecieved - BytesRecieved;
        BytesRecieved = totalBytesRecieved;
        BytesSent = totalBytesSend;
        if (secsPassed > MinDurationSeconds)
        {
            TxBytesPerSecond = (long)((outDiff+_unaccountedTx) / secsPassed);
            RxBytesPerSecond = (long)((inDiff+_unaccountedRx) / secsPassed);
            _lastUpdate = newTime;
            _unaccountedRx = _unaccountedTx = 0;
        } else {
            _unaccountedRx += inDiff;
            _unaccountedTx += outDiff;
        }
    }

    public void Reset()
    {
        _lastUpdate = DateTime.UtcNow;
        BytesSent = 0;
        BytesRecieved = 0;
        TxBytesPerSecond = 0;
        RxBytesPerSecond = 0;
    }

    public BandwidthUsage()
    {

    }

    public override string ToString()
    {
        return $"In: {BytesRecieved}, Out: {BytesSent}";
    }
}