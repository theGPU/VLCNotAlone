using System;
using System.Collections.Generic;
using System.Text;
using VLCNotAlone.Services.Networking;
using VLCNotAlone.Shared.Networking;

namespace VLCNotAlone.Services
{
    internal static class ServiceLocator
    {
        internal static readonly NetworkClient NetworkClient = RegisterService<NetworkClient>();

        static T RegisterService<T>() where T : new()
        {
            var service = new T();
            return service;
        }
    }
}
