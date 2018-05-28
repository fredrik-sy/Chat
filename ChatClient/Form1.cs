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

namespace ChatClient
{
    public partial class Form1 : Form
    {
        private ChatClient chatClient;

        public Form1()
        {
            InitializeComponent();
            InitializeChatClient();
        }

        private void InitializeChatClient()
        {
            chatClient = new ChatClient();
            chatClient.Disconnected += ChatClient_Disconnected;
            chatClient.MessageReceived += ChatClient_MessageReceived;
        }

        /// <summary>
        /// Occurs when the <see cref="ChatClient"/> is disconnected.
        /// </summary>
        private void ChatClient_Disconnected(object sender, EventArgs e)
        {
            btnConnect.Text = "Connect";
            btnSend.Enabled = false;
            txtIPString.Enabled = true;
            txtUsername.Enabled = true;
            txtMessage.Enabled = false;
            txtLog.AppendText(txtUsername.Text + " has left the chat.\n");
        }


        /// <summary>
        /// Occurs when the <see cref="ChatClient"/> has received message.
        /// </summary>
        private void ChatClient_MessageReceived(object sender, string e)
        {
            txtLog.AppendText(e + '\n');
        }


        /// <summary>
        /// Occurs when the <see cref="Button"/> is clicked.
        /// </summary>
        private void Button_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;

            switch (button.Name)
            {
                case "btnConnect":
                    ConnectChatClient();
                    break;
                case "btnSend":
                    chatClient.SendMessage(txtMessage.Text);
                    txtMessage.Text = "";
                    break;
            }
        }

        /// <summary>
        /// Connects to chat server if chat client is not connected else disconnect the chat client.
        /// </summary>
        private async void ConnectChatClient()
        {
            if (chatClient.Connected)
            {
                chatClient.Close();
            }
            else
            {
                pnlChat.Enabled = false;

                if (IPAddress.TryParse(txtIPString.Text, out IPAddress address))
                {
                    try
                    {
                        if (await chatClient.Connect(txtUsername.Text, address))
                        {
                            btnConnect.Text = "Disconnect";
                            btnSend.Enabled = true;
                            txtIPString.Enabled = false;
                            txtUsername.Enabled = false;
                            txtMessage.Enabled = true;
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                    }
                }

                pnlChat.Enabled = true;
            }
        }
    }
}
