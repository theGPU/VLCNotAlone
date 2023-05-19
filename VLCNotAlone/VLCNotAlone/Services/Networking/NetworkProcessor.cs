using System;
using System.Collections.Generic;
using System.Text;

namespace VLCNotAlone.Services.Networking
{
    internal static class NetworkProcessor
    {
        public static event Action<string, string> OnMessageReceived;

        public static void Receive(string username, string message)
        {
            OnMessageReceived?.Invoke(username, message);
        }
    }
}
