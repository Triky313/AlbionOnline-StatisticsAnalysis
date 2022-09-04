using NATS.Client;
using StatisticsAnalysisTool.Models.Nats;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using log4net;
using StatisticsAnalysisTool.Models.NetworkModel;

namespace StatisticsAnalysisTool.Common;

// Daten Struktur
// https://github.com/BroderickHyman/albiondata-client/tree/master/lib

// NATS Topics:
//goldprices.deduped
//marketorders.deduped
//mapdata.deduped

// https://www.youtube.com/watch?v=VPHGgJiQUHw
public class NatsController
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    private static IConnection ConnectToNats()
    {
        var connectionFactory = new ConnectionFactory();
        var options = ConnectionFactory.GetDefaultOptions();
        options.Url = "nats://www.albion-online-data.com:4222";
        options.User = "public";
        options.Password = "thenewalbiondata";

        return connectionFactory.CreateConnection(options);
    }

    public static async Task PushMarketOrderAsync(IEnumerable<AuctionGetOffer> auctionOffers, int clusterIndex)
    {
        try
        {
            using (var connection = ConnectToNats())
            {
                foreach (var valueAuctionGetOffer in auctionOffers)
                {
                    var natsMarketOrder = new NatsMarketOrder(valueAuctionGetOffer, clusterIndex);
                    var marketOrderString = JsonSerializer.Serialize(natsMarketOrder);
                    var bytes = Encoding.UTF8.GetBytes(marketOrderString);
                    connection.Publish("marketorders.deduped", bytes);
                }

                if (connection.State != ConnState.CLOSED)
                {
                    connection.Close();
                }
            }

            await Task.CompletedTask;
        }
        catch (Exception e)
        {
            Log.Error(nameof(PushMarketOrderAsync), e);
        }
    }

    public static void AA()
    {
        // Create a new connection factory to create
        // a connection.
        ConnectionFactory cf = new ConnectionFactory();

        // Creates a live connection to the default
        // NATS Server running locally
        IConnection c = cf.CreateConnection("nats://public:thenewalbiondata@www.albion-online-data.com:4222");

        // Setup an event handler to process incoming messages.
        // An anonymous delegate function is used for brevity.
        EventHandler<MsgHandlerEventArgs> h = (sender, args) =>
        {
            // print the message
            Console.WriteLine(args.Message);

            // Here are some of the accessible properties from
            // the message:
            // args.Message.Data;
            // args.Message.Reply;
            // args.Message.Subject;
            // args.Message.ArrivalSubcription.Subject;
            // args.Message.ArrivalSubcription.QueuedMessageCount;
            // args.Message.ArrivalSubcription.Queue;

            // Unsubscribing from within the delegate function is supported.
            args.Message.ArrivalSubcription.Unsubscribe();
        };

        // The simple way to create an asynchronous subscriber
        // is to simply pass the event in.  Messages will start
        // arriving immediately.
        IAsyncSubscription s = c.SubscribeAsync("foo", h);

        // Alternatively, create an asynchronous subscriber on subject foo,
        // assign a message handler, then start the subscriber.   When
        // multicasting delegates, this allows all message handlers
        // to be setup before messages start arriving.
        IAsyncSubscription sAsync = c.SubscribeAsync("foo");
        sAsync.MessageHandler += h;
        sAsync.Start();

        // Simple synchronous subscriber
        ISyncSubscription sSync = c.SubscribeSync("foo");

        // Using a synchronous subscriber, gets the first message available,
        // waiting up to 1000 milliseconds (1 second)
        Msg m = sSync.NextMessage(1000);

        c.Publish("foo", Encoding.UTF8.GetBytes("hello world"));

        // Unsubscribing
        sAsync.Unsubscribe();

        // Publish requests to the given reply subject:
        c.Publish("foo", "bar", Encoding.UTF8.GetBytes("help!"));

        // Sends a request (internally creates an inbox) and Auto-Unsubscribe the
        // internal subscriber, which means that the subscriber is unsubscribed
        // when receiving the first response from potentially many repliers.
        // This call will wait for the reply for up to 1000 milliseconds (1 second).
        m = c.Request("foo", Encoding.UTF8.GetBytes("help"), 1000);

        // Draining and closing a connection
        c.Drain();

        // Closing a connection
        c.Close();
    }
}