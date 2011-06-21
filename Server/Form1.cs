using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;


namespace Server
{
    public partial class Form1 : Form
    {
        Dictionary<string,double> to_kill;
        System.Random r;

        //Server
        TcpListener listener;
        Thread listeningThread;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            to_kill = new Dictionary<string,double>();

            //Randomize
            r = new System.Random();
            
            //Start server
            listener = new TcpListener(IPAddress.Any, 7654);
            listeningThread = new Thread(new ThreadStart(Listen));
            listeningThread.Start();
        }

        private void Listen()
        {
            listener.Start();
           
            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();

                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClient));
                clientThread.Start(client);
            }
        }

        //Client thread
        private void HandleClient(object client_o)
        {
            TcpClient client = (TcpClient)client_o;
            if (client.Connected)
            {
                NetworkStream clientStream = client.GetStream();

                while (true)
                {
                    int bytesRead = 0;
                    byte[] message = new byte[4096];
                    string message_s = "";
                    ASCIIEncoding encoder = new ASCIIEncoding();

                    do
                    {
                        bytesRead = 0;

                        try
                        {
                            bytesRead = clientStream.Read(message, 0, 4096);
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

                    } while (clientStream.DataAvailable);
                    

                    clientStream.Flush();
                    //parse message
                    string return_s = "";


                    //allowed comamnds:
                    //1.) status
                    //2.) add name
                    //3.) remove name



                    if (message_s == "status")
                    {
                        //return all recognized processes and times
                        //format: name::time||name2::time2



                        foreach (KeyValuePair<string, double> x in to_kill)
                        {
                            if (return_s.Length != 0)
                            {
                                return_s += "::::";

                            }
                            return_s += x.Key + "::" + x.Value;
                        }
                    }
                    else if (Regex.IsMatch(message_s, "^remove (.*)"))
                    {
                        Match x = Regex.Match(message_s, "^remove (.*)");
                        if (x.Length > 0 && x.Length>0)
                        {
                            
                            if (to_kill.ContainsKey(x.Groups[1].ToString()))
                            {
                                to_kill.Remove(x.Groups[1].ToString());
                            }
                        }
                    }else if (Regex.IsMatch(message_s, "^add (.*)"))
                    {
                        Match x = Regex.Match(message_s, "^add (.*)");
                        if (x.Length > 0 && x.Length > 0)
                        {

                            if (!to_kill.ContainsKey(x.Groups[1].ToString()))
                            {
                                to_kill.Add(x.Groups[1].ToString(),-1);
                            }
                        }
                    }
                    else if (Regex.IsMatch(message_s, "^run (.*)"))
                    {
                        Match x = Regex.Match(message_s, "^run (.*)");
                        if (x.Length > 0 && x.Length > 0)
                        {

                            string name = x.Groups[1].ToString();

                            
                        }
                    }

                    if (return_s == "") return_s = "ok";

                    byte[] buffer = encoder.GetBytes(return_s);
                    try
                    {
                        clientStream.Write(buffer, 0, buffer.Length);
                        clientStream.Flush();
                    }
                    catch (Exception) { client.Close(); break; }
                    //close client request
                   
                }
            }

            client.Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Process[] processlist = Process.GetProcesses();
            listBox1.Items.Clear();
            foreach(Process theprocess in processlist){
                string name = theprocess.ProcessName;
                if(to_kill.ContainsKey(name)) {
                    
                    if (to_kill[theprocess.ProcessName] > 0)
                    {
                        to_kill[theprocess.ProcessName] -= timer1.Interval;
                        listBox1.Items.Add(theprocess.ProcessName+" " + to_kill[theprocess.ProcessName]);
                    }
                    else if (to_kill[theprocess.ProcessName] == -1)
                    {
                        //set random time
                        to_kill[theprocess.ProcessName] = r.Next(5000, 15000);
                        listBox1.Items.Add(theprocess.ProcessName + " START");
                    }
                    else if (to_kill[theprocess.ProcessName] <= 0)
                    {
                        //kill process
                        to_kill[theprocess.ProcessName] = -1;
                        listBox1.Items.Add(theprocess.ProcessName + " kill");
                        try
                        {
                            theprocess.Kill();
                            //theprocess.WaitForExit();
                        }
                        catch (Win32Exception)
                        {
                        }
                        catch (InvalidOperationException)
                        {
                        }
                    }
               
                }
                
            }
        }
    }
}
