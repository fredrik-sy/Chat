using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatServer
{
    public class ChatServer
    {
        private CancellationTokenSource cancellationTokenSource;
        private Dictionary<string, Connection> users;
        private TaskFactory taskFactory;
        private TcpListener tcpListener;

        public ChatServer(IPAddress address)
        {
            cancellationTokenSource = new CancellationTokenSource();
            users = new Dictionary<string, Connection>();
            taskFactory = new TaskFactory(cancellationTokenSource.Token);
            tcpListener = new TcpListener(address, 6010);
        }

        /// <summary>
        /// Occurs when the <see cref="ChatServer"/> is logging.
        /// </summary>
        public event EventHandler<string> EventLog;

        /// <summary>
        /// Gets a value indicating whether the <see cref="ChatServer"/> is running.
        /// </summary>
        public bool Running { get; private set; }

        /// <summary>
        /// Starts listening for connections.
        /// </summary>
        public void StartListening()
        {
            tcpListener.Start();
            Running = true;
            KeepListening();
        }

        /// <summary>
        /// Stops listening for connections and closes all connection instance.
        /// </summary>
        public void StopListening()
        {
            // Close the client connection
            foreach(Connection connection in users.Values)
            {
                connection.Close();
            }

            users.Clear();
            Running = false;
            cancellationTokenSource.Cancel();
            tcpListener.Stop();
        }

        /// <summary>
        /// Sends message to all clients.
        /// </summary>
        public void SendMessage(string message)
        {
            OnEventLog(message);

            foreach (Connection connection in users.Values)
            {
                connection.WriteLine(message);
            }
        }

        #region Raising Events
        /// <summary>
        /// Raises the <see cref="EventLog"/> event.
        /// </summary>
        protected virtual void OnEventLog(string e)
        {
            EventLog?.Invoke(this, e);
        }
        #endregion

        /// <summary>
        /// Keep listening for connections asynchronously.
        /// </summary>
        private async void KeepListening()
        {
            try
            {
                while (Running)
                {
                    TcpClient tcpClient = await taskFactory.StartNew(tcpListener.AcceptTcpClient);
                    Connection connection = new Connection(tcpClient, taskFactory);
                    AuthenticateUser(connection);
                }
            }
            catch
            {
                // Occurs when the tcp listener is closed
            }
        }

        /// <summary>
        /// Authenticate a user and proceed with listening on message if connected successfully.
        /// </summary>
        private async void AuthenticateUser(Connection connection)
        {
            string username = await connection.ReadLineAsync();

            if (users.ContainsKey(username))
            {
                // 0 means not connected
                connection.WriteLine("0|This username is already connected.");
                connection.Close();
            }
            else
            {
                // 1 means connected successfully
                connection.WriteLine("1");
                users.Add(username, connection);

                SendMessage(username + " has joined the chat.");
                WaitMessage(username, connection);
            }
        }

        /// <summary>
        /// Waits for incomming message and delegates the message to clients.
        /// </summary>
        private async void WaitMessage(string username, Connection connection)
        {
            string message;

            try
            {
                while ((message = await connection.ReadLineAsync()) != "")
                {
                    // Remove user if message is invalid since the user has been disconnected
                    if (message == null)
                    {
                        users.Remove(username);
                        SendMessage(username + " has left the chat.");
                        return;
                    }
                    else
                    {
                        SendMessage(username + ": " + message);
                    }
                }
            }
            catch
            {
                // Occurs when the connection is closed
            }
        }
    }
}
