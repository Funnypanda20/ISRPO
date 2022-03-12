using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;

namespace Laba1_Sem2
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool done = true;
        private UdpClient client;
        private IPAddress groupAddress;
        private int localPort;
        private int remotePort;
        private int ttl;

        private IPEndPoint remotrEP;
        private UnicodeEncoding encoding = new UnicodeEncoding();
        private string name;
        private string message;
        private readonly SynchronizationContext _syncContext;

        public MainWindow()
        {
            InitializeComponent();
            try
            {
#pragma warning disable CS0618 // Тип или член устарел
                NameValueCollection configuration = ConfigurationSettings.AppSettings;
                groupAddress = IPAddress.Parse(configuration["GroupAddress"]);
                localPort = int.Parse(configuration["LocalPort"]);
                remotePort = int.Parse(configuration["Remoteport"]);
                ttl = int.Parse(configuration["TTL"]);
            }
            catch
            {
                MessageBox.Show(this, "ERROR");
                
            }
            _syncContext = SynchronizationContext.Current;
        }
        public  void btnSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                byte[] data = encoding.GetBytes(name + ": " + textMessage.Text);
                client.Send(data, data.Length,remotrEP);
                textMessage.Clear();
                textMessage.Focus();
            }
            catch (Exception)
            {
                MessageBox.Show(this.ToString());
                throw;
            }
        }

        public void btnStart_Click(object sender, RoutedEventArgs e)
        {
            name = textName.Text;
            textName.IsReadOnly = true;
            try
            {
                client = new UdpClient(localPort);
                client.JoinMulticastGroup(groupAddress, ttl);
                remotrEP = new IPEndPoint(groupAddress, remotePort);
                Thread recevier = new Thread(new ThreadStart(Listener));
                recevier.IsBackground = true;
                recevier.Start();

                byte[] data = encoding.GetBytes(name + "has joined the chat");
                client.Send(data, data.Length, remotrEP);

                btnStart.IsEnabled = false;
                btnStop.IsEnabled = true;
                btnSend.IsEnabled = true;
            }
            catch (SocketException ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }

        private void Listener()
        {
            done = false;
            try
            {
                while (!done)
                {
                    IPEndPoint ep = null;
                    byte[] buffer = client.Receive(ref ep);
                    message = encoding.GetString(buffer);

                    _syncContext.Post(o => DisplayReceivedMessage(), null);
                }
            }
            catch (System.Exception)
            {
                if (done)
                {
                    return;
                }
                else
                    MessageBox.Show(this.ToString());
            }
        }

        public void DisplayReceivedMessage()
        {
            string time = DateTime.Now.ToString("t");
            textMessages.Text = time + " " + message + "\r\n" + textMessages.Text;
        }

        public void btnStop_Click(object sender, RoutedEventArgs e)
        {
            StopListener();
        }
        public void StopListener()
        {
            byte[] data = encoding.GetBytes(name + "has left the chart");
            client.Send(data, data.Length, remotrEP);

            client.DropMulticastGroup(groupAddress);
            client.Close();

            done = true;

            btnStart.IsEnabled = true;
            btnStop.IsEnabled = false;
            btnSend.IsEnabled = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!done)
            {
                StopListener();
            }
        }
    }
}
