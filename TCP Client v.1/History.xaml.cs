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
using MySql.Data.MySqlClient;
using System.Data;


namespace TCP_Client_v._1
{
    /// <summary>
    /// Interaction logic for hystory.xaml
    /// </summary>
    public partial class History : Window
    {
        public History()
        {
            InitializeComponent();
            Loaded += Hystory_Loaded;
        }
        private void Hystory_Loaded(object sender, RoutedEventArgs e)
        {
            string server_url = "server=127.0.0.1;uid=root;pwd=root1234;database=parking;";
            DataTable data = new DataTable();
            MySqlConnection connection = new MySqlConnection(server_url);
            try
            {

                connection.Open();
                MySqlCommand command = new MySqlCommand("SELECT * FROM entrance", connection);
                
                data.Load(command.ExecuteReader());
                ListViewItem newitem = new ListViewItem();
                newitem.HorizontalContentAlignment = HorizontalAlignment.Center;
                newitem.BorderThickness = new Thickness { Bottom = 1, Top = 0 };
                newitem.BorderBrush = Brushes.DarkGray;
                newitem.IsEnabled = true;

                foreach (DataRow row in data.Rows)
                {
                    
                    newitem.Content = new { listview_index = "i"};
                    listView.Items.Insert(0, newitem);
                }

            }
            catch// (Exception ex)
            {
                // handle exception here
            }
            finally
            {
                connection.Close();
            }
        }
    }
}
