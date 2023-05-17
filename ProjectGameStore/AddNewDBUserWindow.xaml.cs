using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static DevExpress.XtraEditors.Mask.MaskSettings;

namespace ProjectGameStore
{
    /// <summary>
    /// Логика взаимодействия для AddNewDBUserWindow.xaml
    /// </summary>
    public partial class AddNewDBUserWindow : Window
    {
        public DataBaseUsers NewUser { get; set; }
        public AddNewDBUserWindow()
        {
            InitializeComponent();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Проверка валидности логина
            if (string.IsNullOrWhiteSpace(loginDBUserTextBox.Text) || !Regex.IsMatch(loginDBUserTextBox.Text, "^[a-zA-Zа-яА-Я]+[a-zA-Zа-яА-Я0-9]*$"))
            {
                MessageBox.Show("Логин не может содержать только цифры или специальные символы / Поле не должно оставаться пустым.");
                return;
            }

            // Проверка валидности пароля
            if (string.IsNullOrWhiteSpace(passwordDBUserTextBox.Text))
            {
                MessageBox.Show("Пароль не может быть пустым.");
                return;
            }

            // Проверка выбранной роли
            if (DbUserRoleComboBox.SelectedItem == null)
            {
                MessageBox.Show("Роль должна быть обязательно выбрана.");
                return;
            }

            // Создание нового пользователя и добавление его в базу данных
            using (var context = new DbUsersEntities())
            {
                var lastUserId = context.DataBaseUsers.Any() ? context.DataBaseUsers.Max(u => u.UserID) : -1;
                var newUserId = Enumerable.Range(0, lastUserId + 2).Except(context.DataBaseUsers.Select(u => u.UserID)).First();

                var newUser = new DataBaseUsers()
                {
                    UserID = newUserId,
                    Username = loginDBUserTextBox.Text,
                    Password = passwordDBUserTextBox.Text,
                    UserRole = DbUserRoleComboBox.Text
                };
                NewUser = newUser; 
                context.DataBaseUsers.Add(newUser);
                context.SaveChanges();
            }

            // Оповещение о успешном добавлении пользователя
            MessageBox.Show("Пользователь успешно добавлен.");
            
            this.Close();
        }



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DbUserRoleComboBox.SelectedIndex = 2;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
