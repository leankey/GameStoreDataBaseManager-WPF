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
using static System.Net.Mime.MediaTypeNames;

namespace ProjectGameStore
{
    /// <summary>
    /// Логика взаимодействия для MsgBoxInfo.xaml
    /// </summary>
    public partial class MsgBoxInfo : Window
    {
        public MsgBoxInfo(string text)
        {

            
            InitializeComponent();
            TextLabel.Content = text;
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        public static void Show(string message)
        {
            var msgBox = new MsgBoxInfo(message);
            msgBox.ShowDialog();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ChangedButton == MouseButton.Left)
                    this.DragMove();
            }
            catch (System.InvalidOperationException)
            { }

        }
    }
}
