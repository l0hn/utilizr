/*

This class has permanently moved to the Utilizr.Util namespace

*/

// using Utilizr.Globalisation;
// using Utilizr.Globalisation.Extensions;

// namespace Utilizr.Vpn
// {

    // public class BandwidthUsage: EventArgs
    // {
    //     private DateTime _lastUpdate = DateTime.UtcNow;
    //     public long BytesSent { get; private set; }
    //     public long BytesRecieved { get; private set; }

    //     public long TxBytesPerSecond { get; set; }
    //     public long RxBytesPerSecond { get; set; }

    //     public void Update(long totalBytesSend, long totalBytesRecieved)
    //     {
    //         var newTime = DateTime.UtcNow;
    //         var secsPassed = (newTime - _lastUpdate).TotalSeconds;
    //         var outDiff = totalBytesSend - BytesSent;
    //         var inDiff = totalBytesRecieved - BytesRecieved;
    //         if (secsPassed > 0)
    //         {
    //             TxBytesPerSecond = (long)(outDiff / secsPassed);
    //             RxBytesPerSecond = (long)(inDiff / secsPassed);
    //         }
    //         _lastUpdate = newTime;
    //         BytesRecieved = totalBytesRecieved;
    //         BytesSent = totalBytesSend;
    //     }

    //     public void Reset()
    //     {
    //         _lastUpdate = DateTime.UtcNow;
    //         BytesSent = 0;
    //         BytesRecieved = 0;
    //         TxBytesPerSecond = 0;
    //         RxBytesPerSecond = 0;
    //     }

    //     public BandwidthUsage()
    //     {

    //     }

    //     public override string ToString()
    //     {
    //         return L._("In: {0}, Out: {1}",
    //             BytesRecieved.ToBytesString(2),
    //             BytesSent.ToBytesString(2));
    //     }
    // }
// }