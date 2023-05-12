using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.ObjectModel;

namespace SignalRTestApp
{
    public class MessageData
    {
        public string Message { get; set; }
        public string User { get; set; }
    }

    internal class Program
    {
        HubConnection hubConnection;
        ObservableCollection<MessageData> Messages;

        bool isBusy;
        public bool IsBusy { get; set; }

        bool isConnected;
        public bool IsConnected { get; set; }

        static async Task Main(string[] args)
        {
            await new Program().Init();
        }

        public async Task Init()
        {
            hubConnection = new HubConnectionBuilder().WithUrl("http://localhost:5051/server").Build();
            Messages = new ObservableCollection<MessageData>();

            hubConnection.KeepAliveInterval = TimeSpan.FromSeconds(5);
            hubConnection.On<string, string>("Receive", (user, message) => SendLocalMessage(user, message));
            hubConnection.Closed += HubConnection_Closed; 
            await Connect();
            var i = 0;
            while (true)
            {
                i++;
                await SendMessage("theCPU", $"test {i}");
                await Task.Delay(2000);
            }
        }

        private async Task HubConnection_Closed(Exception? arg)
        {
            IsConnected = false;
            TimeSpan retryDuration = TimeSpan.FromSeconds(60);
            DateTime retryTill = DateTime.UtcNow.Add(retryDuration);

            while (DateTime.UtcNow < retryTill)
            {
                await Connect();
                if (IsConnected)
                    return;
            }
            Console.WriteLine("Connection closed");
        }

        public async Task Connect()
        {
            if (IsConnected)
                return;
            try
            {
                await hubConnection.StartAsync();
                SendLocalMessage(String.Empty, "Вы вошли в чат...");

                IsConnected = true;
            }
            catch (Exception ex)
            {
                SendLocalMessage(String.Empty, $"Ошибка подключения: {ex.Message}");
            }
        }

        // Отключение от чата
        public async Task Disconnect()
        {
            if (!IsConnected)
                return;

            await hubConnection.StopAsync();
            IsConnected = false;
            SendLocalMessage(String.Empty, "Вы покинули чат...");
        }

        // Отправка сообщения
        async Task SendMessage(string username, string message)
        {
            try
            {
                IsBusy = true;
                await hubConnection.InvokeAsync("Send", username, message);
            }
            catch (Exception ex)
            {
                SendLocalMessage(String.Empty, $"Ошибка отправки: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }
        // Добавление сообщения
        private void SendLocalMessage(string user, string message)
        {
            Console.WriteLine($"New message from {user}: {message}");
        }
    }
}