using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace Laba5_sem2
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Socket hhtpServer;
        private int serverPort;
        private Thread thread;
        private readonly SynchronizationContext synchronizationContext;
        public MainWindow()
        {
            InitializeComponent();
            bStop.IsEnabled = false;
            synchronizationContext = SynchronizationContext.Current;
            tbPort.Text = "8080";
        }

        private void bStop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                hhtpServer.Close();
                thread.Abort();
                bStart.IsEnabled = true;
                bStop.IsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw;
            }
        }
        private void startListeningForConnection()
        {
            while (true)
            {
                DateTime time = DateTime.Now;

                string data = "";
                byte[] bytes = new byte[2048];

                Socket client = hhtpServer.Accept();
                while (true)
                {
                    int numBytes = client.Receive(bytes);
                    data+= Encoding.ASCII.GetString(bytes, 0, numBytes);
                    if (data.IndexOf("\r\n") > -1)
                        break;
                }

                synchronizationContext.Post((o) =>
                {
                    tbOutput.Text += "\r\n\r\b";
                    tbOutput.Text += data;
                    tbOutput.Text += "\n\n-------- End of Request-------";
                }, null);

                string message = "";

                tbMessage.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate { message = tbMessage.Text; });

                string reshead = "HTTP/1.1 200 Everything is fine\nServer: my_sharp_server" + "\nContext-Type: text/html;charset: UTF-8\n\n";
                string resBody = "<!DOCTYPE html>" +
                    "<html>" +
                        "<head>" +
                            "<title>My Server</title>" +
                              "</head>" +
                                "<body>" +
                                  "<p>" + message + "</p>" +
                                  "</body>" +
                                    "</html>";
                string serStr = reshead + resBody;
                byte[] resData = Encoding.ASCII.GetBytes(serStr);
                client.SendTo(resData, client.RemoteEndPoint);
                client.Close();
            }
        }
        private void connectionThreadMethod()
        {
            try
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, serverPort);
                hhtpServer.Bind(endPoint);
                hhtpServer.Listen(1);
                startListeningForConnection();
            }
            catch (Exception)
            {

                throw;
            }
        }
        private void bStart_Click(object sender, RoutedEventArgs e)
        {
            tbOutput.Text = "";
            try
            {
                hhtpServer = new Socket(SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    serverPort = int.Parse(tbPort.Text);
                    if (serverPort >= 49000 || serverPort <= 1050)
                        throw new Exception("server port not within the range");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
                thread = new Thread(new ThreadStart(connectionThreadMethod));
                thread.Start();
                bStart.IsEnabled = false;
                bStop.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }
    }
}
