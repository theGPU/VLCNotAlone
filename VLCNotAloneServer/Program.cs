using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using VLCNotAloneShared;

namespace VLCNotAloneServer
{
    internal class Program
    {
        private static int port = 2048;

        private static ConcurrentQueue<string> ClientsCommandsQueue = new ConcurrentQueue<string>();
        private static List<ConnectedClientPOCO> Clients = new List<ConnectedClientPOCO>();
        //private static List<Socket> clientSockets = new List<Socket>();
        private static object clientSocketsLocker = new object();

        static async Task Main(string[] args)
        {
            if (args.Length >= 1)
                port = int.Parse(args[0]);

            Console.WriteLine("Starting server...");
            GetExternalIp();
            var proxyInputWorker = Task.Run(ProxyInputWorker).ConfigureAwait(false);
            var peoxyOutputWorker = ProxyOutputWorker().ConfigureAwait(false);
            var pingWorker = PingWorker().ConfigureAwait(false);

            await proxyInputWorker;
            await peoxyOutputWorker;
            await pingWorker;
        }

        static void GetExternalIp()
        {
            using HttpClient client = new HttpClient();
            Console.WriteLine($"External address: {client.GetStringAsync("http://ip-api.com/line/?fields=8192").Result.TrimEnd()}:{port}");
        }

        static void ProxyInputWorker()
        {
            byte[] bytes = new byte[2048];

            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            socket.Bind(new IPEndPoint(IPAddress.Any, port));
            socket.Listen(0);
            
            while (true)
            {
                try
                {
                    Console.WriteLine("Waiting for a connection...");
                    Socket handler = socket.Accept();
                    var helloMessage = "";
                    while (true)
                    {
                        int bytesRec = handler.Receive(bytes);
                        helloMessage += Encoding.UTF8.GetString(bytes, 0, bytesRec);
                        if (helloMessage.IndexOf("<ETX>") > -1)
                            break;
                    }

                    Console.WriteLine(helloMessage);
                    var helloCommand = SharedApi.DecodeCommand(helloMessage.Replace("<ETX>", ""));
                    if (helloCommand[0] == "Hello" && helloCommand[1] == $"Api:{SharedApi.apiVersion}")
                    {
                        handler.Send(Encoding.UTF8.GetBytes("ConnectionEstablished<ETX>"));
                        lock (clientSocketsLocker)
                        {
                            var client = new ConnectedClientPOCO { APIVersion = helloCommand[1], Id = helloCommand[2], UserName = helloCommand[3], Socket = handler };
                            Clients.Add(client);

                            ClientsCommandsQueue.Enqueue(SharedApi.CreateCommand("ClientConnected", handler.RemoteEndPoint.ToString()));

                            if (Clients.Count > 1)
                                Clients[0].Socket.Send(Encoding.UTF8.GetBytes(SharedApi.CreateCommand("WhatTime", client.Id)));
                        }

                        Task.Run(() => ClientWorker(handler));
                    }
                    else
                    {
                        handler.Send(Encoding.UTF8.GetBytes("WrongHello<ETX>"));
                        handler.Dispose();
                    }
                } catch (Exception ex)
                {
                    Console.WriteLine($"{ex}");
                }
            }
        }

        static async Task ProxyOutputWorker()
        {
            while (true)
            {
                try
                {
                    if (ClientsCommandsQueue.TryDequeue(out var command))
                    {
                        var commandBytes = Encoding.UTF8.GetBytes(command + "<ETX>");
                        lock (clientSocketsLocker)
                        {
                            for (var i = Clients.Count - 1; i >= 0; i--)
                            {
                                try
                                {
                                    Clients[i].Socket.Send(commandBytes);
                                }
                                catch (Exception ex)
                                {
                                    ClientsCommandsQueue.Enqueue(SharedApi.CreateCommand("ClientDisconnected", Clients[i].Socket.RemoteEndPoint.ToString()));
                                    Clients.RemoveAt(i);
                                    Console.WriteLine($"{ex}");
                                }
                            }
                        }
                    } else
                    {
                        await Task.Delay(100);
                    }
                } catch (Exception ex)
                {
                    Console.WriteLine($"{ex}");
                }
            }
        }

        static async Task PingWorker()
        {
            while (true)
            {
                await Task.Delay(5000);
                ClientsCommandsQueue.Enqueue(SharedApi.CreateCommand("Ping"));
            }
        }

        static void ClientWorker(Socket client)
        {
            try
            {
                Task.Run(() => OnClientRequestClientsList(client));

                byte[] bytes = new byte[2048];
                var data = "";
                while (true)
                {
                    while (true)
                    {
                        int bytesRec = client.Receive(bytes);
                        data += Encoding.UTF8.GetString(bytes, 0, bytesRec);
                        if (data.IndexOf("<ETX>") > -1)
                            break;
                    }

                    var commands = data.Split("<ETX>");
                    data = commands[commands.Length - 1];

                    for (var i = 0; i < commands.Length-1; i++)
                    {
                        if (commands[i] == "Ping")
                            continue;

                        if (commands[i] == "GetClientList")
                            Task.Run(() => OnClientRequestClientsList(client));

                        Console.WriteLine($"New command from client: {commands[i]}");
                        ClientsCommandsQueue.Enqueue(commands[i]);
                    }
                }
            } catch (Exception ex)
            {
                Console.WriteLine($"{ex}");
            }
        }

        static void OnClientRequestClientsList(Socket socket)
        {
            try
            {
                lock (clientSocketsLocker)
                {
                    var response = SharedApi.CreateCommand("ClientsList", Clients.Select(x => $"[{x.UserName}] {x.Id} {x.APIVersion}").ToArray()) + "<ETX>";
                    socket.Send(Encoding.UTF8.GetBytes(response));
                }
            } catch (Exception ex)
            {
                Console.WriteLine($"{ex}");
            }
        }
    }
}
