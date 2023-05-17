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

namespace ProjectGameStore
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Получаем имя пользователя и пароль из соответствующих полей ввода
            string username = Username.Text;
            string password = Password.Password;

            // Создаем контекст базы данных
            using (var db = new DbUsersEntities())
            {
                // Ищем пользователя в базе данных с указанным именем пользователя и паролем
                var user = db.DataBaseUsers.SingleOrDefault(u => u.Username == username && u.Password == password);

                // Если пользователь найден, открываем главное окно приложения
                if (user != null)
                {
                    Window DataBaseWindow = new Window1(user.UserRole, user.Username);
                    DataBaseWindow.Show();
                    this.Close();
                }
                else
                {
                    // Если пользователь не найден, выводим сообщение об ошибке
                    MessageBox.Show("Неверное имя пользователя или пароль.");
                }
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
