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
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.ComponentModel;
using MySql.Data.MySqlClient;
/*
 public Thread StartTheThread(SomeType param1, SomeOtherType param2) {
  var t = new Thread(() => RealStart(param1, param2));
  t.Start();
  return t;
}

private static void RealStart(SomeType param1, SomeOtherType param2) {
  ...
}*/





namespace TCP_Client_v._1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        int req = 0;
        bool AppClosing;
        
        short con1timeout = 0;
        short con2timeout = 0;
        short con3timeout = 0;
        short con4timeout = 0;
        /*Thread t1;
        Thread t2;
        Thread t3;
        Thread t4;*/

        private readonly Brush _LedRed = new RadialGradientBrush(Color.FromArgb(0xFF, 249, 50, 50), Color.FromArgb(0xFF, 255, 0, 0));
        private readonly Brush _Ledoff = new RadialGradientBrush(Color.FromArgb(0xFF, 127, 127, 127), Color.FromArgb(0xFF, 79, 79, 79));
        private readonly Brush _LedGreen = new RadialGradientBrush(Color.FromRgb(129, 255, 129), Color.FromRgb(12, 255, 0));
        private readonly Brush _LedYellow = new RadialGradientBrush(Color.FromRgb(255, 255, 0), Color.FromRgb(180, 180, 0));
        public MainWindow()
        {
            InitializeComponent();
            System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 150);
            timer.Start();
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;

        }
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            AppClosing = true;
            Properties.Settings.Default.Save();
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            AppClosing = false;
            led_1.Fill = _Ledoff; led_1.Stroke = null;
            led_2.Fill = _Ledoff; led_2.Stroke = null;
            led_3.Fill = _Ledoff; led_3.Stroke = null;
            led_4.Fill = _Ledoff; led_4.Stroke = null;
            /*t1 = new Thread(() => tcp_listen(0));
            t1.Start();
            t2 = new Thread(() => tcp_listen(1));
            t2.Start();
            t3 = new Thread(() => tcp_listen(2));
            t3.Start();
            t4 = new Thread(() => tcp_listen(3));
            t4.Start();*/

        }
        private void tcp_listen(int connectionId)
        {
            Int32 Card_ID;
            IPAddress ip = IPAddress.Parse("127.0.0.0");
            long price=0;
            try
            {
                switch (connectionId)
                {
                    case 0:
                        ip = IPAddress.Parse(Properties.Settings.Default.inip1);
                        break;
                    case 1:
                        ip = IPAddress.Parse(Properties.Settings.Default.inip2);
                        break;
                    case 2:
                        ip = IPAddress.Parse(Properties.Settings.Default.outip1);
                        break;
                    case 3:
                        ip = IPAddress.Parse(Properties.Settings.Default.outip2);
                        break;
                    default: break;
                }
            }
            catch { }

            while (!AppClosing)
            {
                TcpClient Client = new TcpClient();
                Client.ReceiveBufferSize = 10;

                try
                {
                    if (!Client.Connected)
                        Client.Connect(ip, 1500);
                }
                catch { }
                if (Client.Connected)
                {
                    NetworkStream netStream = Client.GetStream();
                    if (netStream.CanRead)
                    {
                        byte crc = 0x00;
                        byte[] bytes = new byte[Client.ReceiveBufferSize];
                        //try{

                        netStream.Read(bytes, 0, Client.ReceiveBufferSize);
                        //}
                        //  catch{ }
                        crc = 0; for (int i = 0; i < 7; i++) crc ^= bytes[i];// incoming crc calc -----------
                                                                             //Dispatcher.Invoke(new Action(() =>{
                                                                             //  Led_ON_OFF(connectionId, bytes[5]);
                        if (bytes[0] == 0x0E && bytes[7] == crc)//=================================== ВХОД =========================
                        {
                            Card_ID = Convert.ToInt32(bytes[1]); Card_ID <<= 8;
                            Card_ID += Convert.ToInt32(bytes[2]); Card_ID <<= 8;
                            Card_ID += Convert.ToInt32(bytes[3]); Card_ID <<= 8;
                            Card_ID += Convert.ToInt32(bytes[4]);
                            delete_from_table(Card_ID.ToString());
                            //MessageBox.Show(bytes[0].ToString());
                            if (crete_entrance_table(Card_ID.ToString()))
                            {
                                bytes[0] = 0xAE; bytes[1] = 0x66; bytes[2] = 0x0E; bytes[7] = 0;
                                for (int i = 0; i < 7; i++) bytes[7] ^= bytes[i];
                                Card_ID = 0;
                            }
                        }
                        else if (bytes[0] == 0x2A && bytes[7] == crc)//================================ ВЫХОД ======================
                        {
                            Card_ID = Convert.ToInt32(bytes[1]); Card_ID <<= 8;
                            Card_ID += Convert.ToInt32(bytes[2]); Card_ID <<= 8;
                            Card_ID += Convert.ToInt32(bytes[3]); Card_ID <<= 8;
                            Card_ID += Convert.ToInt32(bytes[4]);
                            int res = find_max_unpaid_index(Card_ID.ToString());
                            if (res >= 0)
                            {
                                string[] inf = find_by_index(res);
                                int inf_len = inf.Length;
                                if (inf_len >= 5)
                                {
                                    DateTime dateTime_now = new DateTime();
                                    TimeSpan timeSpan = new TimeSpan();
                                    dateTime_now = System.DateTime.Now;
                                    DateTime e_DateTime = Convert.ToDateTime(inf[2]);
                                    timeSpan = dateTime_now - e_DateTime;
                                    if (timeSpan.TotalMinutes < 60)
                                    {
                                        price = Properties.Settings.Default.tarif1;
                                    }
                                    else if (timeSpan.TotalMinutes < 120)
                                    {
                                        price = Properties.Settings.Default.tarif2;
                                    }
                                    else if (timeSpan.TotalMinutes < 240)
                                    {
                                        price = Properties.Settings.Default.tarif3;
                                    }
                                    else
                                    {
                                        price = Properties.Settings.Default.tarif4;
                                    }
                                    bytes[0] = 0x2A;
                                    //--------------- Date Time ---------------------
                                    bytes[1] = Convert.ToByte(e_DateTime.Year & 0xFF);
                                    int temp_int = e_DateTime.Month;
                                    temp_int <<= 4;
                                    bytes[2] = Convert.ToByte(temp_int | dateTime_now.Month);//------ in month|out_month
                                    bytes[3] = Convert.ToByte(e_DateTime.Day);
                                    bytes[4] = Convert.ToByte(dateTime_now.Day);
                                    bytes[5] = Convert.ToByte(e_DateTime.Hour);
                                    bytes[6] = Convert.ToByte(dateTime_now.Hour);
                                    bytes[7] = Convert.ToByte(e_DateTime.Minute);
                                    bytes[8] = Convert.ToByte(dateTime_now.Minute);
                                    //----------------- Price ---------------------
                                    bytes[9] = Convert.ToByte(price / 100);
                                    bytes[10] = 0;
                                    for (int i = 0; i < 9; i++) bytes[10] ^= bytes[i];//------------ CRC CALC ------------
                                }
                                Card_ID = 0;
                            }
                            else
                            {
                                bytes[0] = 0x0B; bytes[1] = 0x66; bytes[2] = 0x0E;
                                bytes[7] = 0;
                                for (int i = 0; i < 7; i++) bytes[7] ^= bytes[i];
                                Card_ID = 0;
                            }
                        }
                        else if (bytes[0] == 0x0C && bytes[7] == crc)//================================ Платеж =======================
                        {
                            Card_ID = Convert.ToInt32(bytes[1]); Card_ID <<= 8;
                            Card_ID += Convert.ToInt32(bytes[2]); Card_ID <<= 8;
                            Card_ID += Convert.ToInt32(bytes[3]); Card_ID <<= 8;
                            Card_ID += Convert.ToInt32(bytes[4]);
                            int res = find_max_unpaid_index(Card_ID.ToString());
                            if (res >= 0)
                            {
                                string[] inf = find_by_index(res);
                                int inf_len = inf.Length;
                                if (inf_len >= 5)
                                {
                                    DateTime dateTime_now = new DateTime();
                                    TimeSpan timeSpan = new TimeSpan();
                                    int cash = Convert.ToInt16(bytes[6]) * 100;
                                    Mark_as_Paid(res, cash, DateTime.Now);
                                    bytes[0] = 0x1C;
                                    bytes[7] = 0;
                                    for (int i = 0; i < 7; i++) bytes[7] ^= bytes[i];
                                    //------------------- ListView Update ---------------------
                                    ListViewItem newitem = new ListViewItem();
                                    newitem.HorizontalContentAlignment = HorizontalAlignment.Center;
                                    newitem.BorderThickness = new Thickness { Bottom = 1, Top = 0 };
                                    newitem.Foreground = Brushes.Black;
                                    uint temp = Convert.ToUInt32(inf[0]);
                                    if ((temp & 0x000001) == 1)
                                        newitem.Background = Brushes.LightYellow;
                                    else
                                        newitem.Background = Brushes.LightGreen;
                                    newitem.BorderBrush = Brushes.White;
                                    newitem.IsEnabled = false;
                                    newitem.Content = new { listview_index = inf[0].ToString(), listview_time1 = inf[2].ToString(), listview_time2 = dateTime_now.ToString(), listview_timespan = timeSpan.ToString(), listview_price = price.ToString() };
                                    listView.Items.Insert(0, newitem);
                                }
                            }
                        }
                        else if (bytes[0] == 0x0D && bytes[7] == crc)//========================== Удалить ======================
                        {
                            Card_ID = Convert.ToInt32(bytes[1]); Card_ID <<= 8;
                            Card_ID += Convert.ToInt32(bytes[2]); Card_ID <<= 8;
                            Card_ID += Convert.ToInt32(bytes[3]); Card_ID <<= 8;
                            Card_ID += Convert.ToInt32(bytes[4]);
                            if (delete_from_table(Card_ID.ToString()))
                            {
                                bytes[0] = 0xAD; bytes[1] = 0x66; bytes[2] = 0x0E;
                                bytes[7] = 0;
                                for (int i = 0; i < 7; i++) bytes[7] ^= bytes[i];
                                Card_ID = 0;
                            }
                        }
                        else
                        {
                            bytes[7] = 0;
                            for (int i = 0; i < 7; i++)
                            {
                                bytes[i] = 0x11;
                                bytes[7] ^= bytes[i];
                            }
                        }
                        if (netStream.CanWrite)
                        {

                            netStream.Write(bytes, 0, 10);

                        }
                        //  }));
                    }
                    netStream.Flush();
                    netStream.Close();
                    Client.Close();
                }

                //  while (Client.Connected)

            }
            Thread.CurrentThread.Abort();
        }
        private void tcp_work(int connectionId)
        {
            Int32 Card_ID;
            TcpClient Client = new TcpClient();
            IPAddress ip = IPAddress.Parse("127.0.0.0");
            
            try
            {
                switch (connectionId)
                {
                    case 0:
                        ip = IPAddress.Parse(Properties.Settings.Default.inip1);
                        break;
                    case 1:
                        ip = IPAddress.Parse(Properties.Settings.Default.inip2);
                        
                        break;
                    case 2:
                        ip = IPAddress.Parse(Properties.Settings.Default.outip1);
                        break;
                    case 3:
                        ip = IPAddress.Parse(Properties.Settings.Default.outip2);
                        break;
                    default: break;
                }
                
                Client.Connect(ip, 1500);
            }
            catch
            {
                return;
            }
            Client.ReceiveBufferSize = 15;
            if (Client.Connected)
            {
                NetworkStream netStream = Client.GetStream();
               // MessageBox.Show(ip.ToString());
                if (netStream.CanRead)
                {
                    byte crc = 0x00;
                    TimeSpan timeSpan = new TimeSpan();
                    DateTime dateTime_now = new DateTime();
                    byte[] bytes = new byte[Client.ReceiveBufferSize];
                    try
                    {
                        netStream.Read(bytes, 0, Client.ReceiveBufferSize);
                    }
                    catch
                    {

                    }
                    crc = 0; for (int i = 0; i < 7; i++) crc ^= bytes[i];// incoming crc calc -----------

                    if (bytes[0] == 0x0E && bytes[7] == crc)//=================================== ВХОД =========================
                    {
                        // MessageBox.Show(bytes[0].ToString());
                        Card_ID = Convert.ToInt32(bytes[1]); Card_ID <<= 8;
                        Card_ID += Convert.ToInt32(bytes[2]); Card_ID <<= 8;
                        Card_ID += Convert.ToInt32(bytes[3]); Card_ID <<= 8;
                        Card_ID += Convert.ToInt32(bytes[4]);
                        delete_from_table(Card_ID.ToString());

                        if (crete_entrance_table(Card_ID.ToString()))
                        {
                            bytes[0] = 0xAE; bytes[1] = 0x66; bytes[2] = 0x0E;
                            bytes[7] = 0;
                            for (int i = 0; i < 7; i++) bytes[7] ^= bytes[i];
                            Card_ID = 0;
                        }
                    }
                    else if (bytes[0] == 0x2A && bytes[7] == crc)//================================ ВЫХОД ======================
                    {
                        Card_ID = Convert.ToInt32(bytes[1]); Card_ID <<= 8;
                        Card_ID += Convert.ToInt32(bytes[2]); Card_ID <<= 8;
                        Card_ID += Convert.ToInt32(bytes[3]); Card_ID <<= 8;
                        Card_ID += Convert.ToInt32(bytes[4]);
                        long price = 0 ;
                        
                        int res = find_max_unpaid_index(Card_ID.ToString());
                        //MessageBox.Show(Card_ID.ToString());
                       // MessageBox.Show(res.ToString());
                        if (res >= 0)
                        {
                            string[] inf = find_by_index(res);
                            int inf_len = inf.Length;
                            if (inf_len >= 5)
                            {
                                dateTime_now = System.DateTime.Now;
                                DateTime e_DateTime = Convert.ToDateTime(inf[2]);
                                timeSpan = dateTime_now - e_DateTime;
                                long total_minutes = Convert.ToInt64( timeSpan.TotalMinutes);
                                if (total_minutes <= 15)
                                {
                                    price = 0;
                                }
                                else if (total_minutes <= 60)
                                {
                                    total_minutes -= 60;
                                    price = Properties.Settings.Default.tarif2;
                                }
                                while(total_minutes>0)
                                {
                                    total_minutes -= 60;
                                    price += Properties.Settings.Default.tarif3;
                                }
                                bytes[0] = 0x2A;
                                
                               // MessageBox.Show(lv_price.ToString());
                                //=--------------- Date Time ---------------------
                                bytes[1] = Convert.ToByte(e_DateTime.Year & 0xFF);
                                bytes[2] = Convert.ToByte(dateTime_now.Year & 0xFF);
                                int temp_int = e_DateTime.Month;
                                temp_int <<= 4;
                                bytes[3] = Convert.ToByte(temp_int | dateTime_now.Month);
                                bytes[4] = Convert.ToByte(e_DateTime.Day);
                                bytes[5] = Convert.ToByte(dateTime_now.Day);
                                bytes[6] = Convert.ToByte(e_DateTime.Hour);
                                bytes[7] = Convert.ToByte(dateTime_now.Hour);
                                bytes[8] = Convert.ToByte(e_DateTime.Minute);
                                bytes[9] = Convert.ToByte(dateTime_now.Minute);
                                //=------------- Price ---------------------
                                bytes[10] = Convert.ToByte(price & 0x00FF);
                                price >>= 8;
                                bytes[11] = Convert.ToByte(price & 0x00FF);
                                for (int i = 0; i < 11; i++) bytes[12] ^= bytes[i];
                            }
                            Card_ID = 0;
                        }
                        else
                        {
                            bytes[0] = 0x0B; bytes[1] = 0x66; bytes[2] = 0x0E;
                            bytes[7] = 0;
                            for (int i = 0; i < 7; i++) bytes[7] ^= bytes[i];
                            Card_ID = 0;
                        }
                    }
                    else if (bytes[0] == 0x0C )//&& bytes[8] == crc)//================================ Платеж =======================
                    {
                        Card_ID = Convert.ToInt32(bytes[1]); Card_ID <<= 8;
                        Card_ID += Convert.ToInt32(bytes[2]); Card_ID <<= 8;
                        Card_ID += Convert.ToInt32(bytes[3]); Card_ID <<= 8;
                        Card_ID += Convert.ToInt32(bytes[4]);
                        int res = find_max_unpaid_index(Card_ID.ToString());
                        if (res >= 0)
                        {
                            string[] inf = find_by_index(res);
                            int inf_len = inf.Length;
                            if (inf_len >= 5)
                            {
                                int cash = Convert.ToInt16(bytes[7]);
                                cash <<= 8;
                                cash += Convert.ToInt16(bytes[6]);
                                dateTime_now = DateTime.Now;
                                Mark_as_Paid(res, cash, dateTime_now);
                                bytes[0] = 0x1C;
                                bytes[7] = 0;
                                for (int i = 0; i < 7; i++) bytes[7] ^= bytes[i];
                                
                                //------------------- ListView Update ---------------------
                                Dispatcher.Invoke(new Action(() =>
                                {
                                    ListViewItem newitem = new ListViewItem();
                                    newitem.HorizontalContentAlignment = HorizontalAlignment.Center;
                                    newitem.BorderThickness = new Thickness { Bottom = 1, Top = 0 };
                                    newitem.Foreground = Brushes.Black;
                                    uint temp = Convert.ToUInt32(inf[0]);
                                    if ((temp & 0x000001) == 1)
                                        newitem.Background = Brushes.LightYellow;
                                    else
                                        newitem.Background = Brushes.LightGreen;
                                    newitem.BorderBrush = Brushes.White;
                                    newitem.IsEnabled = false;
                                    newitem.Content = new { listview_index = inf[0].ToString(), listview_time1 = inf[2].ToString(),
                                        listview_time2 = dateTime_now.ToString(), listview_timespan = timeSpan.ToString(),
                                        listview_price = cash.ToString() };
                                    listView.Items.Insert(0, newitem);
                                }));
                            }
                        }
                    }
                    else if (bytes[0] == 0x0D && bytes[7] == crc)//========================== Удалить ======================
                    {
                        Card_ID = Convert.ToInt32(bytes[1]); Card_ID <<= 8;
                        Card_ID += Convert.ToInt32(bytes[2]); Card_ID <<= 8;
                        Card_ID += Convert.ToInt32(bytes[3]); Card_ID <<= 8;
                        Card_ID += Convert.ToInt32(bytes[4]);
                        if (delete_from_table(Card_ID.ToString()))
                        {
                            bytes[0] = 0xAD; bytes[1] = 0x66; bytes[2] = 0x0E;
                            bytes[7] = 0;
                            for (int i = 0; i < 7; i++) bytes[7] ^= bytes[i];
                            Card_ID = 0;
                        }
                    }
                    else
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            Led_ON_OFF(connectionId, bytes[5]);
                        }));
                        bytes[7] = 0;
                        for (int i = 0; i < 7; i++)
                        {
                            bytes[i] = 0x11;
                            bytes[7] ^= bytes[i];
                        }
                    }
                    if (netStream.CanWrite)
                    {
                        try
                        {
                            netStream.Write(bytes, 0, 14);
                        }
                        catch { }
                    }
                    //}));
                }

                netStream.Flush();
                netStream.Close();
            }
            while (Client.Connected)
                Client.Close();
            Thread.CurrentThread.Abort();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            Thread t1 = new Thread(() => tcp_work(req));
            t1.Start();
            if (req < 3) req++;
            else req = 0;
            /*Thread t1 = new Thread(() => tcp_work(0));
            t1.Start();
            Thread t2 = new Thread(() => tcp_work(1));
            t2.Start();
            Thread t3 = new Thread(() => tcp_work(2));
            t3.Start();
            Thread t4 = new Thread(() => tcp_work(3));
            t4.Start();*/
            //=============== Clock ==============
            calendar.Content = DateTime.Now.ToString("yyyy/MMM/dd HH:mm:ss");
            con1timeout++; if (con1timeout > 30) led_1.Fill = _Ledoff;
            con2timeout++; if (con2timeout > 30) led_2.Fill = _Ledoff;
            con3timeout++; if (con3timeout > 30) led_3.Fill = _Ledoff;
            con4timeout++; if (con4timeout > 30) led_4.Fill = _Ledoff;

        }
        private bool crete_entrance_table(string Card_ID)
        {
            string server_url = "server=127.0.0.1;uid=root;pwd=root1234;database=parking;";
            MySqlConnection mysql_conn = new MySqlConnection(server_url);
            MySqlCommand Command = new MySqlCommand
              ("INSERT INTO entrance(`card_id`,`payment`)   VALUES(@Card_ID,'0');", mysql_conn);
            Command.Parameters.AddWithValue("@card_id", Card_ID);
            try
            {
                mysql_conn.Open();
                Command.ExecuteNonQuery();
                mysql_conn.Close();
                return true;
            }
            catch //(MySqlException ex)
            {
                return false;
            }
            finally { }

        }
        private string[] find_by_index(int index)
        {
            string[] ret = new string[5];
            string server_url = "server=127.0.0.1;uid=root;pwd=root1234;database=parking;";
            MySqlConnection mysql_conn = new MySqlConnection(server_url);
            MySqlCommand Command = new MySqlCommand("SELECT * FROM entrance WHERE   `index` = '" + index + "'", mysql_conn);
            MySqlDataReader DataReader;
            try
            {
                mysql_conn.Open();
                DataReader = Command.ExecuteReader();
                while (DataReader.Read())
                {
                    for (int i = 0; i < DataReader.FieldCount; i++)
                    {
                        ret[i] = DataReader.GetString(i);
                        // MessageBox.Show(ret[i]);
                    }
                }
                DataReader.Close();
            }
            catch
            {

            }
            finally
            {
                mysql_conn.Close();
            }
            return ret;
        }
        private string[] find_in_entrance(string Card_ID)
        {
            string[] ret = new string[5];
            string server_url = "server=127.0.0.1;uid=root;pwd=root1234;database=parking;";
            MySqlConnection mysql_conn = new MySqlConnection(server_url);
            MySqlCommand Command = new MySqlCommand("SELECT * FROM entrance WHERE  ( card_id = '" + Card_ID + "'" + "and payment = '" + "0')", mysql_conn);

            MySqlDataReader DataReader;
            try
            {
                mysql_conn.Open();
                DataReader = Command.ExecuteReader();
                while (DataReader.Read())
                {
                    for (int i = 0; i < DataReader.FieldCount; i++)
                    {
                        ret[i] = DataReader.GetString(i);
                        //MessageBox.Show(ret[i]);
                    }
                }
                DataReader.Close();
            }
            catch
            {

            }
            finally
            {
                mysql_conn.Close();
            }
            return ret;
        }
        private int find_max_unpaid_index(string Card_ID)
        {
            int maxId;
            string server_url = "server=127.0.0.1;uid=root;pwd=root1234;database=parking;";
            MySqlConnection mysql_conn = new MySqlConnection(server_url);
            MySqlCommand Command = new MySqlCommand("SELECT max(`index`) FROM entrance WHERE  ( card_id = '" + Card_ID + "'" + "and payment = '" + "0')", mysql_conn);
            try
            {
                mysql_conn.Open();
                maxId = Convert.ToInt32(Command.ExecuteScalar());
                //MessageBox.Show(maxId.ToString());
            }
            catch
            {
                return -1;
            }
            finally
            {
                mysql_conn.Close();
            }
            return maxId;
        }
        private bool delete_from_table(string Card_ID)
        {
            string server_url = "server=127.0.0.1;uid=root;pwd=root1234;database=parking;";
            MySqlConnection mysql_conn = new MySqlConnection(server_url);
            MySqlCommand Command = new MySqlCommand
              ("DELETE  FROM entrance  where card_id = '" + Card_ID + "'" + "and payment = '" + "0'", mysql_conn);
            // Command.Parameters.AddWithValue("@card_id", Card_ID);
            try
            {
                mysql_conn.Open();
                Command.ExecuteNonQuery();
                mysql_conn.Close();
                return true;
            }
            catch //(MySqlException ex)
            {

                //  MessageBox.Show(ex.Number.ToString());
                return false;
            }
            finally { }

        }
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

            Dispatcher.Invoke(new Action(() => { }));
            Settings_Window SetWin = new Settings_Window();
            SetWin.Owner = Application.Current.MainWindow;
            SetWin.Owner.Hide();
            SetWin.Show();
        }
        private void Mark_as_Paid(int index, int payment, DateTime datetime)
        {
            //UPDATE `entrance` SET `payment`='2200', `out_datetime`='0000-03-00 00:21:00' WHERE `index`='347';
            string MySQLFormatDate = datetime.ToString("yyyy-MM-dd HH:mm:ss");
            string server_url = "server=127.0.0.1;uid=root;pwd=root1234;database=parking;";
            MySqlConnection mysql_conn = new MySqlConnection(server_url);
            MySqlCommand Command = new MySqlCommand("UPDATE `entrance` SET `payment`= '" + payment + "', `out_datetime`= '" + MySQLFormatDate + "' WHERE `index`= '" + index + "'", mysql_conn);
            MySqlDataReader DataReader;
            try
            {
                mysql_conn.Open();
                DataReader = Command.ExecuteReader();
                DataReader.Close();
               // MessageBox.Show(MySQLFormatDate);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                mysql_conn.Close();
            }
        }
        public void Led_ON_OFF(int n, byte w)
        {
            // Dispatcher.Invoke(new Action(() =>{
            //MessageBox.Show(w.ToString());
            try
            {
                if (n == 0)
                {
                    if (w == 0x20)
                    {
                        led_1.Fill = _LedYellow;
                    }
                    else if (w == 0x10)
                    {
                        led_1.Fill = _LedRed;
                    }
                    else if (w == 0xBA)
                    {
                        led_1.Fill = _LedGreen;
                    }
                    con1timeout = 0;
                }
                else if (n == 1)
                {
                    if (w == 0x20)
                    {
                        led_2.Fill = _LedYellow;
                    }
                    else if (w == 0x10)
                    {
                        led_2.Fill = _LedRed;
                    }
                    else if (w == 0xBA)
                    {
                        led_2.Fill = _LedGreen;
                    }
                    con2timeout = 0;
                }
                else if (n == 2)
                {
                    if (w == 0x20)
                    {
                        led_3.Fill = _LedYellow;
                    }
                    else if (w == 0x10)
                    {
                        led_3.Fill = _LedRed;
                    }
                    else if (w == 0xBA)
                    {
                        led_3.Fill = _LedGreen;
                    }
                    con3timeout = 0;
                }
                else if (n == 3)
                {
                    if (w == 0x20)
                    {
                        led_4.Fill = _LedYellow;
                    }
                    else if (w == 0x10)
                    {
                        led_4.Fill = _LedRed;
                    }
                    else if (w == 0xBA)
                    {
                        led_4.Fill = _LedGreen;
                    }
                    con4timeout = 0;

                }
            }
            catch { }
            //  }));
        }
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            History his_win = new History();
            his_win.Owner = Application.Current.MainWindow;
            his_win.Show();
        }
        private void About(object sender, RoutedEventArgs e)
        {

        }
        private void button_Click_1(object sender, RoutedEventArgs e)
        {
            //Led_ON_OFF(2, 0xBA);
        }
    }
}
