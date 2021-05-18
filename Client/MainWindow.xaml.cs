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
using System.Threading;
using FileSocket;

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
        private int fileServerCount = 0;
        private string selectedFileServerIP = "";
        private string selectedFileServerPort = "";
        private SocketFileInfo selectedFile = null;

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

        private void btn_refresh_Click(object sender, RoutedEventArgs e)
        {
            unselectChoice();
            //send to server signal 'refresh' then receive the list of file after then
            if (msl.SendMessageSignal("refresh") == true)
            {
                MessageBox.Show("Request sent successfully");
                string tmp = msl.refreshSignalThrowBack();
                //load list to UI

                if (tmp == "OK")
                {
                    fileServerCount = msl.socketFileManagers.Count();
                    for(int i = 0; i < fileServerCount; i++)
                    {
                        cbb_fileserver.Items.Add(msl.socketFileManagers[0].ServerAddress);
                    }

                    if(fileServerCount > 0)
                    {
                        dg_main.ItemsSource = msl.socketFileManagers[0].FileList;
                        cbb_fileserver.SelectedIndex = 0;
                        setIPAndPort(msl.socketFileManagers[0].ServerAddress);
                    }
                }
            }
            else
            {
                MessageBox.Show("Failed to send request");
            }
            
        }

        private void cbb_fileserver_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dg_main.ItemsSource = msl.socketFileManagers[cbb_fileserver.SelectedIndex].FileList;
            setIPAndPort(cbb_fileserver.SelectedItem.ToString());
        }

        private void dg_main_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedFile = msl.socketFileManagers[cbb_fileserver.SelectedIndex].FileList[dg_main.SelectedIndex];
            btn_download.IsEnabled = true;
            btn_deselect.IsEnabled = true;
        }

        private void setIPAndPort(string fullIP)
        {
            string[] arrListStr = fullIP.Split(':');
            if(arrListStr.Length > 1)
            {
                selectedFileServerIP = arrListStr[0];
                selectedFileServerPort = arrListStr[1];
            }
        }

        private void unselectChoice()
        {
            btn_download.IsEnabled = false;
            btn_deselect.IsEnabled = false;
            selectedFile = null;
        }

        private void btn_deselect_Click(object sender, RoutedEventArgs e)
        {
            unselectChoice();
        }

        private void btn_download_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Thông tin file đang được chọn để tải:\n " +
                "\n- File Server IP: " + selectedFileServerIP +
                "\n- File server Port: " + selectedFileServerPort +
                "\n- File info:" +
                selectedFile.ID.ToString() + ", " +
                selectedFile.Name + ", " +
                selectedFile.Size + ", " +
                selectedFile.Path + ", " +
                selectedFile.Type + "," +
                selectedFile.MD5);
        }
    }
}
