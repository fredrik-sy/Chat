using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatServer
{
    public partial class Form1 : Form
    {
        private ChatServer chatServer;

        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Occurs when the <see cref="Button"/> is clicked.
        /// </summary>
        private void Button_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;

            switch (button.Name)
            {
                case "btnStartListening":
                    StartChatServer();
                    break;
                case "btnSend":
                    chatServer.SendMessage(txtMessage.Text);
                    txtMessage.Text = "";
                    break;
            }
        }

        /// <summary>
        /// Starts the chat server if it is not running else stops the chat server.
        /// </summary>
        private void StartChatServer()
        {
            if (chatServer?.Running == true)
            {
                txtMessage.Text = "";
                txtMessage.Enabled = false;
                btnSend.Enabled = false;
                chatServer.StopListening();
                chatServer.EventLog -= ChatServer_EventLog;
                btnStartListening.Text = "Start Listening";
            }
            else
            {
                if (IPAddress.TryParse(txtIPString.Text, out IPAddress address))
                {
                    try
                    {
                        chatServer = new ChatServer(address);
                        chatServer.EventLog += ChatServer_EventLog;
                        chatServer.StartListening();
                        btnStartListening.Text = "Stop Listening";
                        btnSend.Enabled = true;
                        txtMessage.Enabled = true;
                        txtLog.AppendText("Monitoring connections...\n");
                    }
                    catch
                    {
                        MessageBox.Show("The requested address is not valid.");
                    }
                }
            }
        }

        /// <summary>
        /// Occurs when the <see cref="ChatServer"/> is logging.
        /// </summary>
        private void ChatServer_EventLog(object sender, string e)
        {
            txtLog.AppendText(e + '\n');
        }
    }
}
