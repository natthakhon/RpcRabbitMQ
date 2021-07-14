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

namespace RabbitMq.Servicer
{
    public partial class Form1 : Form
    {
        private ConnectionFactory factory;
        private IConnection con;
        private IModel channel;
        private delegate void SetTextCallback(string payload,string response);
        private RabbitMq.Servicer<string, string> servicer;

        public Form1()
        {
            InitializeComponent();
            this.FormClosed += Form1_FormClosed;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            channel.Close();
            con.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            factory = new ConnectionFactory() { HostName = "localhost" };
            con = factory.CreateConnection();
            channel = con.CreateModel();

            this.servicer = new Servicer<string, string>(channel
                , RabbitMq.Config.Config.sentTo
                , payload =>   // once request recived, reverse the received string then return it
                {
                    char[] chars = payload.ToCharArray();
                    char[] result = new char[chars.Length];
                    for (int i = 0, j = payload.Length - 1; i < payload.Length; i++, j--)
                    {
                        result[i] = chars[j];
                    }
                    return new string(result);
                });
            
            servicer.Servicing += Servicer_Servicing;
            servicer.Service();            
        }

        private void Servicer_Servicing(string payload, string response)
        {
            if (this.label1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(Servicer_Servicing);
                this.Invoke(d, new object[] { payload,response });
            }
            else
            {
                this.label1.Text = payload;
                this.label2.Text = response;                
            }
        }

    }
}
