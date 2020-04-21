using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Gms.Games.MultiPlayer.RealTime;
using CleverEver.Droid.Connectivity.Google;

namespace CleverEver.Droid.Connectivity
{
    static class GoogleSerializationHelpers
    {
        public static byte[] SerializeMessage<T>(T message)
        {
            using (var ms = new System.IO.MemoryStream())
            using (var writer = new Newtonsoft.Json.Bson.BsonWriter(ms))
            {
                GoogleServer.MessageSerializer.Serialize(writer, message);
                return ms.ToArray();
            }
        }

        public static object DeserializeMessage(RealTimeMessage message)
        {
            using (var ms = new System.IO.MemoryStream(message.GetMessageData()))
            using (var reader = new Newtonsoft.Json.Bson.BsonReader(ms))
            {
                return GoogleServer.MessageSerializer.Deserialize(reader);
            }
        }
    }
}