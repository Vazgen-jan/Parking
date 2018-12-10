using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;
namespace TCP_Client_v._1
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Settings_Window : Window
    {
        public Settings_Window()
        {
            InitializeComponent();
            Loaded += Settings_Window_Loaded;
            Closing += Settings_Window_Closing;
        }

        private void Settings_Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Owner.Show();
        }

        private void close_but_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void save_but_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.inip1 = textBox1.Text;
            Properties.Settings.Default.inip2 = textBox2.Text;
            Properties.Settings.Default.outip1 = textBox5.Text;
            Properties.Settings.Default.outip2 = textBox6.Text;
            Properties.Settings.Default.tarif1 = Convert.ToInt16(textBox_1.Text);
            Properties.Settings.Default.tarif2 = Convert.ToInt16(textBox_2.Text);
            Properties.Settings.Default.tarif3 = Convert.ToInt16(textBox_3.Text);
           // Properties.Settings.Default.tarif4 = Convert.ToInt16(textBox_4.Text);


            Properties.Settings.Default.Save();
        }
        private void Settings_Window_Loaded(object sender, RoutedEventArgs args)
        {
            textBox1.Text = Properties.Settings.Default.inip1;
            textBox2.Text = Properties.Settings.Default.inip2;
           // textBox3.Text = Properties.Settings.Default.inip3;
           // textBox4.Text = Properties.Settings.Default.inip4;
            textBox5.Text = Properties.Settings.Default.outip1;
            textBox6.Text = Properties.Settings.Default.outip2;
           // textBox7.Text = Properties.Settings.Default.outip3;
           // textBox8.Text = Properties.Settings.Default.outip4;
            textBox_1.Text = Properties.Settings.Default.tarif1.ToString();
            textBox_2.Text = Properties.Settings.Default.tarif2.ToString();
            textBox_3.Text = Properties.Settings.Default.tarif3.ToString();
          //  textBox_4.Text = Properties.Settings.Default.tarif4.ToString();
        }
        private void check_butt_Click(object sender, RoutedEventArgs e)
        {
            SerStat.Background = Brushes.White; SerStat.Content = ""; check_butt1.IsEnabled = true;
            // if (checkBox.IsChecked == true)
            // {
            if (textBox1.Text != "")
            {
                check_butt1.IsEnabled = false;
                TcpClient Client = new TcpClient();
                try
                {
                    Client.Connect(IPAddress.Parse(textBox1.Text), 1500);
                    Client.ReceiveBufferSize = 25;
                    if (Client.Connected)
                    {
                        NetworkStream netStream = Client.GetStream();
                        if (netStream.CanRead)
                        {

                            byte[] bytes = new byte[Client.ReceiveBufferSize];
                            netStream.Read(bytes, 0, Client.ReceiveBufferSize);
                            string returndata = Encoding.UTF8.GetString(bytes);
                            Dispatcher.Invoke(new Action(() =>
                            {
                                bytes = Encoding.ASCII.GetBytes(returndata);
                                if (bytes[0] == 0x0E)
                                {

                                }
                            }));
                        }
                        netStream.Close();
                        Client.Close();
                    }
                    Dispatcher.Invoke(new Action(() => { SerStat.Background = Brushes.Green; SerStat.Content = "ԳՈՐԾՈՒՄ Է"; check_butt1.IsEnabled = true; }));
                }
                catch
                {
                    Dispatcher.Invoke(new Action(() => { SerStat.Background = Brushes.Red; SerStat.Content = "ՉԻ ԳՈՐԾՈՒՄ"; check_butt1.IsEnabled = true; }));
                }
                Dispatcher.Invoke(new Action(() => { check_butt1.IsEnabled = true; }));
            }
        }

    }
}
