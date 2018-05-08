using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace Messenger
{
    public partial class MessageForm : Form
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Program));

        private string machineName = Environment.MachineName;

        private MqttClient client;
        public MessageForm()
        {
            InitializeComponent();
            log4net.Config.XmlConfigurator.Configure();
 
        }

        private void OnTypeTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !this.typeTextBox.Text.Equals(""))
            {
                //messageRichTextBox.AppendText(machineName+"："+typeTextBox.Text);
                byte[] byt = Encoding.GetEncoding("Big5").GetBytes(ConfigurationManager.AppSettings["myname"] + "：" + typeTextBox.Text);
                client.Publish("chat", byt);
                typeTextBox.Clear();
            }
        }

        private void OnTypeTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if ((int)e.KeyCode == 13)
            {
                e.SuppressKeyPress = true;
            }

            if (e.KeyCode == Keys.Enter)
            {
                return;
            }
        }

        private void OnMessageRichTextBox_TextChanged(object sender, EventArgs e)
        {
            messageRichTextBox.SelectionStart = messageRichTextBox.TextLength;

            messageRichTextBox.ScrollToCaret();
        }

        private void OnEnterButton_Click(object sender, EventArgs e)
        {
            // messageRichTextBox.AppendText(machineName + "：" + typeTextBox.Text);
            byte[] byt = Encoding.GetEncoding("Big5").GetBytes(ConfigurationManager.AppSettings["myname"] + "：" + typeTextBox.Text);
            client.Publish("chat", byt);
            typeTextBox.Clear();
        }

        private void MessageForm_Load(object sender, EventArgs e)
        {
            this.ActiveControl = typeTextBox;

            try
            {
                client = new MqttClient(ConfigurationManager.AppSettings["broker"]);

                string clientId = Guid.NewGuid().ToString();
                logger.Info("START CONNECT");
                client.Connect(clientId);
                logger.Info("CONNECT SUCCEED");
                client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;
                // subscribe to the topic "/home/temperature" with QoS 2
                logger.Info("SUBSCRIBE START");
                client.Subscribe(new string[] { "chat" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
                logger.Info("SUBSCRIBE END");
            }
            catch(Exception ex)
            {
                MessageBox.Show("error : " + ex.Message);
                Environment.Exit(0);
            }         
        }

        private void Client_MqttMsgPublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e)
        {
            string msg = System.Text.Encoding.GetEncoding("Big5").GetString(e.Message);
            messageRichTextBox.Invoke(
                new EventHandler(
                    delegate
                    {
                        messageRichTextBox.AppendText(msg + "\n");
                    }
                    )
                    );

        }

        private void MessageForm_FormClosed(object sender, FormClosedEventArgs e)
        {

            client.Disconnect();
            Environment.Exit(0);
        }

    }
}
