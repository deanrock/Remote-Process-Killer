using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace client_Client
{
    public partial class Form1 : Form
    {
        TcpClient client;

        NetworkStream stream;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            client.Connect(IPAddress.Parse(textBox1.Text), 7654);

            stream = client.GetStream();
      //      MessageBox.Show(client.Connected.ToString());

            button1.Enabled = false;
            label2.Text = "CONNECTING";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            client = new TcpClient();

            label2.Text = "";   
        }

        private string Request(string command)
        {
            if (client.Connected)
            {
                label2.Text = "";

                ASCIIEncoding encoder = new ASCIIEncoding();

                byte[] buffer = encoder.GetBytes(command);

                stream.Write(buffer, 0, buffer.Length);
                stream.Flush();
                
               
                //read data from server
                int bytesRead = 0;
                byte[] message = new byte[4096];
                string message_s = "";

                do
                {
                    bytesRead = 0;

                    try
                    {
                        bytesRead = stream.Read(message, 0, 4096);
                    }
                    catch
                    {
                        break; //step out of the while loop
                    }

                    if (bytesRead == 0)
                    {
                        break; //we have finished reading
                    }

                    message_s += encoder.GetString(message, 0, bytesRead);
                } while (stream.DataAvailable);
             
               return message_s;
            }

             return null;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            string r = Request("status");

            if (r != null)
            {
                string[] programs = Regex.Split(r, "::::");

                listBox1.Items.Clear();

                foreach (string p in programs)
                {
                    string[] p_t = Regex.Split(p, "::");

                    if (p_t.Length == 2)
                    {
                        string x;

                        if (p_t[1] == "-1")
                        {
                            //program closed
                            x = "closed";
                        }
                        else
                        {
                            x = p_t[1];
                        }
                        listBox1.Items.Add(p_t[0] + " - " +x);
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listBox1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                

                string program = listBox1.Items[listBox1.SelectedIndex].ToString();
                program = program.Substring(0, program.LastIndexOf('-')-1);
                if (MessageBox.Show(this, "Do you want to delete " + program + " from kill_it list?", "Delete item", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                {
                    //delete selected element
                    
                    Request("remove " + program);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string program = textBox2.Text;
            Request("add " + program);
            textBox2.Clear();
        }
    }
}
