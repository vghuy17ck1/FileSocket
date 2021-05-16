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
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private MainWindow mainUI = null;
        private bool isConnectOK = false;

        public LoginWindow()
        {
            InitializeComponent();
        }
        public LoginWindow(MainWindow mainUI_in)
        {
            InitializeComponent();
            mainUI = mainUI_in;
        }


        private void TryLogin()
        {
            if(mainUI.tryConnect(tb_ip.Text, tb_port.Text))
            {
                MessageBox.Show("Connect successfully!");
                isConnectOK = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Fail to connect!");
            }
        }

        /// Bellow are events of UI

        //login button
        private void btn_login_click(object sender, RoutedEventArgs e)
        {
            TryLogin();
        }

        private void LoginForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(isConnectOK == false)
            {
                mainUI.Close();
            }
        }
    }
}
