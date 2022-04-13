using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace VLCNotAloneShared
{
    public class ClientApi
    {
        public string host;
        public int port;

        private bool _connected = false;
        public bool Connected { get { return _connected; } set { _connected = value; OnConnectChanged?.Invoke(value); } }
        internal Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        internal object socketSendLocker = new object();

        public Action<bool> OnConnectChanged;

        public Action<bool, string, string> OnEvent; //isError, title, desc
        public Action OnDisconnect;

        public Action<string> OnSetLocalMediaFile;
        public Action<string> OnSetGlobalMediaFile;
        public Action<string> OnSetInternetMediaFile;
        public Action<long> OnSetTimeRecived; 
        public Action<long?> OnPause;
        public Action OnResume;

        public Action<string> OnClientConnected;
        public Action<string> OnClientDisconnected;

        public Action<int> OnWhatTime;
        public Action<string, string, long> OnWhatTimeResponce;

        public Action<string[]> OnClientsList;

        private int clientId;

        public void Connect(string nickname)
        {
            try
            {
                clientId = new Random().Next();
                if (socket.Connected)
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Disconnect(false);
                }
                socket.Dispose();

                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                OnEvent?.Invoke(false, "Connect", "Connecting...");
                socket.Connect(host, port);
                Connected = true;
                OnEvent?.Invoke(false, "Connect", "Sending hello");
                SendHelloMessage(nickname);
                OnEvent?.Invoke(false, "Connect", "Successful");
                Task.Run(() => ReciverWorker(clientId));
                Task.Run(() => PingWorker(clientId));
            } catch (Exception ex)
            {
                OnEvent?.Invoke(true, "Connect", $"Error: {ex.Message}");
                Connected = false;
            }
        }

        public void ReciverWorker(int workerClientId)
        {
            try
            {
                byte[] bytes = new byte[2048];
                var data = "";
                while (Connected)
                {
                    while (Connected)
                    {
                        int bytesRec = socket.Receive(bytes);
                        data += Encoding.UTF8.GetString(bytes, 0, bytesRec);
                        if (data.IndexOf("<ETX>") > -1)
                            break;
                    }

                    var commands = data.Split("<ETX>");
                    data = commands[commands.Length - 1];

                    for (var i = 0; i < commands.Length - 1; i++)
                        InvokeCommandAction(SharedApi.DecodeCommand(commands[i]));
                }
            } catch (Exception ex)
            {
                if (Connected && workerClientId == clientId)
                {
                    OnEvent?.Invoke(true, "ReciverWorker", $"Error: {ex}");
                    Connected = false;
                    OnDisconnect?.Invoke();
                }
            }
        }

        public void InvokeCommandAction(string[] command)
        {
            if (command[0] == "Ping")
                return;

            Trace.WriteLine(command[0]);
            switch (command[0])
            {
                case "SetTime":
                    OnSetTimeRecived?.Invoke(long.Parse(command[1]));
                    break;
                case "Pause":
                    OnPause?.Invoke(command.Length > 1 ? long.Parse(command[1]) : null);
                    break;
                case "Resume":
                    OnResume?.Invoke();
                    break;
                case "SetLocalMediaFile":
                    OnSetLocalMediaFile?.Invoke(command[1]);
                    break;
                case "SetGlobalMediaFile":
                    OnSetGlobalMediaFile?.Invoke(command[1]);
                    break;
                case "SetInternetMediaFile":
                    OnSetInternetMediaFile?.Invoke(command[1]);
                    break;
                case "ClientConnected":
                    OnClientConnected?.Invoke(command[1]);
                    break;
                case "ClientDisconnected":
                    OnClientDisconnected?.Invoke(command[1]);
                    break;
                case "WhatTime":
                    OnWhatTime?.Invoke(int.Parse(command[1]));
                    break;
                case "WhatTimeResponce":
                    var destClientId = int.Parse(command[1]);
                    if (destClientId == clientId)
                        OnWhatTimeResponce?.Invoke(command[2], command[3], long.Parse(command[4]));
                    break;
                case "ClientsList":
                    OnClientsList?.Invoke(command.Skip(1).ToArray());
                    break;
            }
        }

        public async Task PingWorker(int workerClientId)
        {
            try
            {
                while (Connected)
                {
                    await Task.Delay(10000);
                    SendCommand("Ping", true);
                }
            }
            catch (Exception ex)
            {
                if (Connected && workerClientId == clientId)
                {
                    OnEvent?.Invoke(true, "PingWorker", $"Error: {ex}");
                    Connected = false;
                    OnDisconnect?.Invoke();
                }
            }
        }

        internal void SendHelloMessage(string nickname) => SendCommand(SharedApi.CreateCommand("Hello", $"Api:{SharedApi.apiVersion}", $"{clientId}", $"{Environment.UserName}", nickname));

        public void SetTime(long time) => SendCommand(SharedApi.CreateCommand("SetTime", time.ToString()));
        public void Pause() => SendCommand(SharedApi.CreateCommand("Pause"));
        public void Pause(long time) => SendCommand(SharedApi.CreateCommand("Pause", time.ToString()));
        public void Resume() => SendCommand(SharedApi.CreateCommand("Resume"));

        public void SetLocalMediaFile(string path) => SendCommand(SharedApi.CreateCommand("SetLocalMediaFile", path));
        public void SetGlobalMediaFile(string path) => SendCommand(SharedApi.CreateCommand("SetGlobalMediaFile", path));
        public void SetInternetMediaFile(string path) => SendCommand(SharedApi.CreateCommand("SetInternetMediaFile", path));

        public void RequestClientList() => SendCommand(SharedApi.CreateCommand("GetClientList"));

        public void SendWhatTimeResponce(int clientId, string mode, string path, long time) => SendCommand(SharedApi.CreateCommand("WhatTimeResponce", clientId.ToString(), mode, path, time.ToString()));

        internal void SendCommand(string command, bool throwError = false)
        {
            try
            {
                lock (socketSendLocker)
                    socket.Send(Encoding.UTF8.GetBytes(command + "<ETX>"));
            }
            catch (Exception ex)
            {
                if (throwError)
                    throw;

                if (Connected)
                {
                    OnEvent?.Invoke(true, "SendCommand", $"Error: {ex}");
                    Connected = false;
                    OnDisconnect?.Invoke();
                }
            }
        }
    }
}
