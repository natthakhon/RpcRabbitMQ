using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RabbitMq.Requestor
{
    public partial class Form1 : Form
    {
        private ConnectionFactory factory;
        private IConnection con;
        private IModel channel;
        private delegate void SetTextCallback(string response);
        private RabbitMq.Requestor<string, string> requestor;
        public Form1()
        {
            InitializeComponent();
            this.createConnection();
            this.FormClosed += Form1_FormClosed;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.requestor.deleteQueue();
            channel.Close();
            con.Close();
        }

        private void createConnection()
        {
            var sentto = RabbitMq.Config.Config.sentTo;
            var replyto = RabbitMq.Config.Config.replyTo;

            factory = new ConnectionFactory() { HostName = RabbitMq.Config.Config.host };
            con = factory.CreateConnection();
            channel = con.CreateModel();
            this.requestor = new Requestor<string, string>(
                    channel
                    , sentto
                    , replyto);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox1.Text))
            {   
                requestor.Request(textBox1.Text);
                requestor.Received += Requestor_Received;
            }
        }

        private void Requestor_Received(string response)
        {
            if (this.label2.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(Requestor_Received);
                this.Invoke(d, new object[] { response });
            }
            else
            {
                this.label2.Text = response;                
            }
        }
    }
}
