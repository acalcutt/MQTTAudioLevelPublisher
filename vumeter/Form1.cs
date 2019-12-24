using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using System.Collections.Specialized;

// including the NAudio Library
using NAudio;
using NAudio.CoreAudioApi;

// including the M2Mqtt Library
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Diagnostics;

namespace vumeter
{
    public partial class Form1 : Form
    {
        MqttClient client;
        string clientId;
        string MQTTserver;
        string MQTTusername;
        string MQTTpassword;
        string MQTTtopic;
        string MQTTinterval;

        public Form1()
        {
            InitializeComponent();
            MQTTserver = ConfigurationManager.AppSettings.Get("MQTTserver");
            MQTTusername = ConfigurationManager.AppSettings.Get("MQTTusername");
            MQTTpassword = ConfigurationManager.AppSettings.Get("MQTTpassword");
            MQTTtopic = ConfigurationManager.AppSettings.Get("MQTTtopic");
            MQTTinterval = ConfigurationManager.AppSettings.Get("MQTTinterval");
            txtMQTTsvr.Text = MQTTserver;
            txtMQTTuser.Text = MQTTusername;
            txtMQTTpass.Text = MQTTpassword;
            txtTopicPublish.Text = MQTTtopic;
            txtInterval.Text = MQTTinterval;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            MMDeviceEnumerator de = new MMDeviceEnumerator();
            MMDevice device = de.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            float volume = (float)device.AudioMeterInformation.MasterPeakValue * 100;
            int volume_round = (int)Math.Round(volume);
            progressBar1.Value = (int)volume_round;

            string Topic = txtTopicPublish.Text;

            // publish a message with QoS 2
            client.Publish(Topic, Encoding.UTF8.GetBytes(volume_round.ToString()), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, true);
        }


        private void btnSendAudioMQTT_Click(object sender, EventArgs e)
        {
            if (timer1.Enabled)
            {
                try
                    { client.Disconnect();}
                catch
                    { }

                timer1.Stop();
                btnSendAudioMQTT.Text = "Publish Audio Level";
            }
            else
            {
                string BrokerAddress = txtMQTTsvr.Text;

                client = new MqttClient(BrokerAddress);

                // use a unique id as client id, each time we start the application
                clientId = Guid.NewGuid().ToString();

                client.Connect(clientId, txtMQTTuser.Text, txtMQTTpass.Text);

                int interval = 0;
                bool success = Int32.TryParse(txtInterval.Text, out interval);
                if (!success)
                {
                    interval = 1000;
                }

                Debug.WriteLine(interval);
                timer1.Interval = interval;
                timer1.Start();
                btnSendAudioMQTT.Text = "Stop Publishing";
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
