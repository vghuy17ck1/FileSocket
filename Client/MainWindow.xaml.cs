using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        LoginWindow loginUI = null;
        private ClientServiceClient msl = new ClientServiceClient();
        private string serverIP = "127.0.0.1";
        private string serverPort = "10001";

        public MainWindow()
        {
            InitializeComponent();
            personalInitalize();
        }

        public bool tryConnect(string _IP, string _port)
        {
            if (string.IsNullOrEmpty(_IP) || string.IsNullOrEmpty(_port))
                return false;

            if (!msl.SetIPAddress(_IP))
            {
                return false;
            }

            try
            {
                msl.StartClient(_IP, _port).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                return false;
            }

            serverIP = _IP;
            serverPort = _port;
            lbl_IP.Content = serverIP;
            lbl_port.Content = serverPort;

            return true;
        }
        

        private void personalInitalize()
        {
            loginUI = new LoginWindow(this);
            loginUI.ShowDialog();
        }



        // events of UI
    }
}
