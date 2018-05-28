using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatClient
{
    public class ChatClient
    {
        private CancellationTokenSource cancellationTokenSource;
        private TaskFactory taskFactory;
        private Connection connection;

        public ChatClient()
        {
            cancellationTokenSource = new CancellationTokenSource();
            taskFactory = new TaskFactory(cancellationTokenSource.Token);
        }

        /// <summary>
        /// Occurs when the <see cref="ChatClient"/> is disconnected.
        /// </summary>
        public event EventHandler Disconnected;

        /// <summary>
        /// Occurs when the <see cref="ChatClient"/> has received message.
        /// </summary>
        public event EventHandler<string> MessageReceived;

        /// <summary>
        /// Gets a value indicating whether the <see cref="ChatClient"/> is connected to a remote host.
        /// </summary>
        public bool Connected
        {
            get { return connection?.TcpClient.Connected == true; }
        }

        /// <summary>
        /// Connects to a remote host asynchronous.
        /// </summary>
        public async Task<bool> Connect(string username, IPAddress address)
        {
            TcpClient tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(address, 6010);
            connection = new Connection(tcpClient, taskFactory);

            if (await Authenticate(username))
            {
                WaitMessage();
                return true;
            }
            else
            {
                connection.Close();
                return false;
            }
        }

        /// <summary>
        /// Closes the connection.
        /// </summary>
        public void Close()
        {
            connection.Close();
        }

        /// <summary>
        /// Sends a message to the server.
        /// </summary>
        public void SendMessage(string message)
        {
            connection.WriteLine(message);
        }

        #region Raising Events
        /// <summary>
        /// Raises the <see cref="Disconnected"/> event.
        /// </summary>
        protected virtual void OnDisconnected(EventArgs e)
        {
            Disconnected.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="MessageReceived"/> event.
        /// </summary>
        protected virtual void OnMessageReceived(string e)
        {
            MessageReceived.Invoke(this, e);
        }
        #endregion

        /// <summary>
        /// Authenticate this client using the username.
        /// </summary>
        private async Task<bool> Authenticate(string username)
        {
            connection.WriteLine(username);

            // Message follows this pattern [0-1]|.*
            string message = await connection.ReadLineAsync();

            if (message[0] == '1')
            {
                return true;
            }
            else
            {
                OnMessageReceived(message.Substring(2));
                return false;
            }
        }

        /// <summary>
        /// Waits for incomming message from clients.
        /// </summary>
        private async void WaitMessage()
        {
            string message;

            try
            {
                while ((message = await connection.ReadLineAsync()) != "")
                {
                    // User is disconnected if message is invalid.
                    if (message == null)
                    {
                        connection.Close();
                        OnDisconnected(new EventArgs());
                        return;
                    }
                    else
                    {
                        OnMessageReceived(message);
                    }
                }
            }
            catch
            {
                connection.Close();
                OnDisconnected(new EventArgs());
            }
        }
    }
}
