using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Data.Entity.Migrations;
using System.Security.Cryptography;
using System.Text.RegularExpressions;


namespace ProjectGameStore
{
    /// <summary>
    /// Логика взаимодействия для Window2.xaml
    /// </summary>
    public partial class AddNewGameWindow : Window 
    {
        // Объект для передачи данных в родительскую форму
        public Games NewGame { get; set; }

        public AddNewGameWindow()
        {
            InitializeComponent();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {

            // Проверяем валидность цены
            decimal price;
            if (!decimal.TryParse(gamesPriceTextBox.Text, out price))
            {
                if ( price < 1 && price > 500)
                MessageBox.Show("Некорректное значение цены.");
                return;
            }

            // Проверяем валидность года выпуска
            int releaseYear;
            if (!int.TryParse(gamesReleaseYearTextBox.Text, out releaseYear ))
            {
                if(releaseYear < 1990 && releaseYear> 2024)
                MessageBox.Show("Некорректное значение года выпуска.");
                return;
            }

            // Создание нового объекта Game и заполнение его данными из текстбоксов
            Games newGame = new Games
            {
                
                Developer = gamesDeveloperTextBox.Text,
                Genre = gamesGenreTextBox.Text,
                Price = decimal.Parse(gamesPriceTextBox.Text),
                Publisher = gamesPublisherTextBox.Text,
                ReleaseYear = int.Parse(gamesReleaseYearTextBox.Text),
                Title = gamesTitleTextBox.Text
            };

            // Передача данных в родительскую форму
            NewGame = newGame;

            // Закрытие дочернего окна
            Close();
        }

        private void gamesPriceTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Проверка наличия точки в тексте
            bool hasDot = ((TextBox)sender).Text.Contains(".");

            // Если введенный символ не является цифрой или точкой, отменяем его ввод
            if (!char.IsDigit(e.Text, 0) && e.Text != "." || (e.Text == "." && hasDot))
            {
                e.Handled = true;
                return;
            }

            // Если введена точка и в тексте уже есть точка, отменяем ее ввод
            if (e.Text == "." && hasDot)
            {
                e.Handled = true;
                return;
            }

            // Если введена точка и она является первым символом в тексте, отменяем ее ввод
            if (e.Text == "." && ((TextBox)sender).Text.Length == 0)
            {
                e.Handled = true;
                return;
            }

            // Если после точки уже есть 2 знака, отменяем ввод
            if (hasDot && ((TextBox)sender).Text.Split('.')[1].Length >= 2)
            {
                e.Handled = true;
                return;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void gamesTitleTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

            // проверяем, что длина текста не превышает 100 символов
            string newText = (sender as TextBox).Text + e.Text;
            if (newText.Length > 100)
            {
                e.Handled = true;
                return;
            }
        }

        private void gamesPublisherTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // проверяем, является ли введенный символ буквой или пробелом на латинице или кириллице
            if (!Regex.IsMatch(e.Text, "^[a-zA-Zа-яА-Я ]+$"))
            {
                // разрешаем только нажатие клавиши Backspace
                if (e.Text != "\b")
                {
                    e.Handled = true;
                    return;
                }
            }

            // проверяем, что длина текста не превышает 20 символов
            string newText = (sender as TextBox).Text + e.Text;
            if (newText.Length > 50)
            {
                e.Handled = true;
                return;
            }
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
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

