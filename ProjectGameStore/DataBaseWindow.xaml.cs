using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Globalization;
using DevExpress.XtraReports.UI;
using System.IO;
using System.Data.Entity;
using System.Data.SqlClient;
using Microsoft.Win32;
using System.Data;
using Syncfusion.Linq;
using DevExpress.Mvvm.Native;
using ProjectGameStore.Reports;
using DevExpress.Xpf.Printing;
using LiveCharts;
using LiveCharts.Wpf;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace ProjectGameStore
{
  
    public partial class Window1 : MetroWindow
    {
        public SeriesCollection ChartSeriesCollection { get; set; }
        public string[] Labels { get; set; } // Добавили объявление переменной Labels
        // Создаем коллекции для хранения объектов Games и DataBaseUsers
        ObservableCollection<Games> _games;
        ObservableCollection<DataBaseUsers> _dbusers;

        // Создаем объекты CollectionViewSource для отображения коллекций в окне
        CollectionViewSource gamesViewSource;
        CollectionViewSource dataBaseUsersViewSource;
      
        // Хранит имя пользователя, который авторизовался в системе
        string LoggedUser;

       




        public Window1(string role, string loggedUser)
        {

            InitializeComponent();
            // Устанавливаем контекст данных для окна
            DataContext = this;

            // Настраиваем источники данных для CollectionViewSource
            gamesViewSource = (CollectionViewSource)(FindResource("gamesViewSource"));
            gamesViewSource.Source = _games;

            dataBaseUsersViewSource = (CollectionViewSource)(FindResource("dataBaseUsersViewSource"));
            dataBaseUsersViewSource.Source = _dbusers;

            // Проверяем роль пользователя и настраиваем интерфейс в зависимости от этого
            CheckUserRole(role);

            // Сохраняем имя пользователя, который авторизовался в системе
            LoggedUser = loggedUser;

            // Инициализация SeriesCollection
            ChartSeriesCollection = new SeriesCollection();

            // Привязка SeriesCollection к свойству Series элемента PieChart
            GenresChart.Series = ChartSeriesCollection;


        }
        
       
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ChangedButton == MouseButton.Left)
                    this.DragMove();
            } catch(System.InvalidOperationException)
            {  }

          
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            // Загружаем данные об играх в коллекцию _games
            _games = LoadGames();
            // Загружаем данные о пользователях в коллекцию _dbusers
            _dbusers = LoadDataBaseUsers();

            // Загружаем записи логов из файла
            LoadLogsFromFile();
            // Добавляем запись в логгер о входе пользователя в базу данных
            Log("Пользователь вошёл в базу данных");

            // Получаем ссылку на объект gamesViewSource и указываем источник данных для отображения списка игр
            System.Windows.Data.CollectionViewSource gamesViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("gamesViewSource")));
            gamesViewSource.Source = _games;

            // Получаем ссылку на объект dataBaseUsersViewSource и указываем источник данных для отображения списка пользователей
            System.Windows.Data.CollectionViewSource dataBaseUsersViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("dataBaseUsersViewSource")));
            dataBaseUsersViewSource.Source = _dbusers;


            // Установка отчета по умолчанию для просмотра при загрузке окна
            ListOfGameStoreUsersReport report = new ListOfGameStoreUsersReport();
            ReportViewer.DocumentSource = report;
            report.CreateDocument();
        }

        /*Метод LoadGames использует контекст базы данных gamestoreEntities и загружает все объекты из таблицы Games в базе данных. 
         Он затем создает новую коллекцию ObservableCollection и заполняет ее полученными объектами. Это позволяет создавать биндинг 
         данных между элементами управления и объектами базы данных.
         Метод LoadDataBaseUsers похож на LoadGames, но использует другой контекст базы данных DbUsersEntities для загрузки данных
         из таблицы DataBaseUsers в базе данных.*/
        private ObservableCollection<Games> LoadGames()
        {
            using (var context = new gamestoreEntities())
            {
                return new ObservableCollection<Games>(context.Games.ToList());
            }
        }
        private ObservableCollection<DataBaseUsers> LoadDataBaseUsers()
        {
            using (var pcontext = new DbUsersEntities())
            {
                return new ObservableCollection<DataBaseUsers>(pcontext.DataBaseUsers.ToList());
            }
        }

        //Кнопка закрытия
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        //Данный метод заполняет TextBox'ы данными текущей выделенной строки DataGrid
        private void gamesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (gamesDataGrid.SelectedItem != null)
            {
                // Получить выбранный элемент
                var game = gamesDataGrid.SelectedItem as Games;

                // Установить значения TextBox
                gamesIDTextBox.Text = game.GameID.ToString();
                gamesTitleTextBox.Text = game.Title;
                gamesDeveloperTextBox.Text = game.Developer;
                gamesGenreTextBox.Text = game.Genre;
                gamesPriceTextBox.Text = game.Price.ToString();
                gamesPublisherTextBox.Text = game.Publisher;
                gamesReleaseYearTextBox.Text = game.ReleaseYear.ToString();
            }
        }

        // Обработчик события клика по кнопке сохранения изменений в базе данных
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Создаем экземпляр контекста базы данных
            using (var context = new gamestoreEntities())
            {
                // Получаем ID игры из текстового поля
                int gameId = int.Parse(gamesIDTextBox.Text);
                // Получаем игру из базы данных по ID
                var game = context.Games.FirstOrDefault(g => g.GameID == gameId);

                // Создаем копию старых данных об игре для логирования изменений
                var oldGameData = new Games()
                {
                    GameID = game.GameID,
                    Title = game.Title,
                    Developer = game.Developer,
                    Genre = game.Genre,
                    Price = game.Price,
                    Publisher = game.Publisher,
                    ReleaseYear = game.ReleaseYear
                };

                // Если игра найдена в базе данных
                if (game != null)
                {
                    // Обновляем данные игры из текстовых полей
                    game.Title = gamesTitleTextBox.Text;
                    game.Developer = gamesDeveloperTextBox.Text;
                    game.Genre = gamesGenreTextBox.Text;
                    game.Price = decimal.Parse(gamesPriceTextBox.Text, CultureInfo.InvariantCulture);
                    game.Publisher = gamesPublisherTextBox.Text;
                    game.ReleaseYear = int.Parse(gamesReleaseYearTextBox.Text);
                }
                else // Если игра не найдена в базе данных
                {
                    // Создаем новую игру из текстовых полей
                    game = new Games()
                    {
                        GameID = int.Parse(gamesIDTextBox.Text),
                        Title = gamesTitleTextBox.Text,
                        Developer = gamesDeveloperTextBox.Text,
                        Genre = gamesGenreTextBox.Text,
                        Price = decimal.Parse(gamesPriceTextBox.Text),
                        Publisher = gamesPublisherTextBox.Text,
                        ReleaseYear = int.Parse(gamesReleaseYearTextBox.Text)
                    };

                    // Добавляем новую игру в контекст базы данных
                    context.Games.Add(game);
                }


                try
                {
                    // Сохраняем изменения в базе данных
                    context.SaveChanges();

                    // Формируем строку для логирования изменений
                    string log = $"Измененны данные об игре ID={game.GameID} || ";
                    if (oldGameData.Title != game.Title) log += $" Название: {oldGameData.Title} >>> {game.Title}";
                    if (oldGameData.Developer != game.Developer) log += $" Разработчик: {oldGameData.Developer} >>> {game.Developer}";
                    if (oldGameData.Publisher != game.Publisher) log += $" Издатель: {oldGameData.Publisher} >>> {game.Publisher}";
                    if (oldGameData.ReleaseYear != game.ReleaseYear) log += $" Год выпуска: {oldGameData.ReleaseYear} >>> {game.ReleaseYear}";
                    if (oldGameData.Genre != game.Genre) log += $" Жанр: {oldGameData.Genre} >>> {game.Genre}";
                    if (oldGameData.Price != game.Price) log += $" Цена: {oldGameData.Price} >>> {game.Price}";

                    // Записываем лог в файл
                    Log(log);
                }
                catch (Exception ex)
                {
                    // Выводим сообщение об ошибке сохранения данных
                    MessageBox.Show($"Save error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            //Обновление и загрузка списка игр в DataGrid
            loadGamesData();
        }

        /*Метод loadDBUsersData() загружает данные о пользователях из базы данных и обновляет соответствующее
         представление данных в пользовательском интерфейсе*/
        private void loadDBUsersData()
        {
            // Создаем контекст базы данных для работы с таблицей "DataBaseUsers"
            using (var pcontext = new DbUsersEntities())
            {
                // Получаем список всех пользователей из базы данных
                var dbusers = pcontext.DataBaseUsers.ToList();

                // Создаем новую наблюдаемую коллекцию и заполняем ее данными о пользователях
                _dbusers = new ObservableCollection<DataBaseUsers>(dbusers);

                // Обновляем источник данных для соответствующего представления в пользовательском интерфейсе
                dataBaseUsersViewSource.Source = _dbusers;
            }
        }

        /*Метод loadGamesData() загружает данные об играх из базы данных
         и обновляет соответствующее представление данных в пользовательском интерфейсе.*/
        private void loadGamesData()
        {
            // Создаем контекст базы данных для работы с таблицей "Games"
            using (var context = new gamestoreEntities())
            {
                // Получаем список всех игр из базы данных
                var games = context.Games.ToList();

                // Создаем новую наблюдаемую коллекцию и заполняем ее данными об играх
                _games = new ObservableCollection<Games>(games);

                // Обновляем источник данных для соответствующего представления в пользовательском интерфейсе
                gamesViewSource.Source = _games;
            }
        }




        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            using (var context = new gamestoreEntities())
            {
                // Получаем ID игры, которую нужно удалить
                int gameID = int.Parse(gamesIDTextBox.Text);

                // Получаем объект Games, который нужно удалить
                var game = context.Games.FirstOrDefault(g => g.GameID == gameID);

                // Если объект найден, удаляем его из базы данных
                if (game != null)
                {
                    //Удаление соотвуствующей игры из базы данных
                    context.Games.Remove(game);

                    try
                    {
                        //Пытаемся сохранить изменения
                        context.SaveChanges();
                        //Формирование строки лога 
                        Log($"Игра {game.Title} ID={game.GameID} удалена");
                    }
                    catch (Exception ex)
                    {
                       
                        MessageBox.Show($"Delete error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    //Обновление списка игр
                    loadGamesData();
                }
            }
        }


        //Метод который добавляет данные о новой игре в асортименте, получая данные от дочернего окна с текстбоксами AddNewGameWindow();
        private void AddNewButton_Click(object sender, RoutedEventArgs e)
        {
            //Создание экземпляра окна AddNewGameWindow();
            AddNewGameWindow addNewGameWindow = new AddNewGameWindow();

            // Открытие дочернего окна и ожидание закрытия его пользователем
            addNewGameWindow.ShowDialog();

            // Получение данных из дочернего окна
            Games newGame = addNewGameWindow.NewGame;

            // Если данные не равны null, то добавляем новую игру в базу данных
            if (newGame != null)
            {
                using (var context = new gamestoreEntities())
                {
                    // Получаем последний GameId
                    int lastGameId = context.Games.OrderByDescending(x => x.GameID).FirstOrDefault()?.GameID ?? 0;

                    // Генерируем новый GameId
                    newGame.GameID = lastGameId + 1;

                    // Добавляем новую игру в базу данных
                    context.Games.Add(newGame);
                    context.SaveChanges();
                    Log($"Добавлена новая игра {newGame.Title} ID={newGame.GameID}");
                }
                MessageBox.Show("Игра добавлена успешно!");
            }
            //обновление списка игр
            loadGamesData();

        }

        private void gamesPriceTextBox_TextInput(object sender, TextCompositionEventArgs e)    {     }

        private void gamesPriceTextBox_PreviewKeyDown(object sender, KeyEventArgs e) { }

        //Валидация введенной цены для текстбокса gamesPriceTextBox
        private void gamesPriceTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Проверка наличия точки в тексте
            bool hasDot = ((TextBox)sender).Text.Contains(".");

            // Если введенный символ не является цифрой или точкой, отменяем его ввод
            if (!char.IsDigit(e.Text, 0) && e.Text != "." && e.Text!=","|| (e.Text == "." && hasDot))
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

        private void gamesGenreTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
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


        public class GamesViewModel
        {
            public List<string> Fields { get; } = new List<string>
            {
                "Название",
                "Жанр",
                "Разработчик",
                "Издатель",
                "Год выпуска"
            };
        }

        //Применение фильтрации при изменении текстового значения поля SearchBox
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {

            ApplyGamesFilter();
        }

        // Метод, который применяет фильтры к данным игр
        private void ApplyGamesFilter()
        {
            // Получаем значение строки поиска, выбранный критерий поиска и значения фильтров цены и года выпуска
            string searchText = gamesSearchBox.Text.ToLower();
            string selectedCriteria = (gamesSearchComboBox.SelectedItem as ComboBoxItem)?.Content as string;
            int priceFilterValue = (int)PriceFilterSlider.Value;
            int releaseYearFilterValue = (int)ReleaseYearFilterSlider.Value;

            // Инициализируем значения фильтров по цене и году выпуска
            bool filterByPrice = true;
            bool filterByYear = true;

            // Устанавливаем фильтр по списку игр
            gamesViewSource.View.Filter = game =>
            {
                // Получаем текущую игру
                var currentGame = game as Games;

                // Если включен фильтр по цене
                if (IsEnabledPrice.IsChecked == true)
                {
                    if (IsCloserValuePriceCheck.IsChecked == true)
                    {
                        // Фильтруем по приблизительным значениям цены с погрешностью 5
                        filterByPrice = (currentGame.Price >= priceFilterValue - 5) && (currentGame.Price <= priceFilterValue + 5);
                    }
                    else
                    {
                        // Фильтруем по точному значению цены
                        filterByPrice = currentGame.Price == priceFilterValue;
                    }
                }

                // Если включен фильтр по году выпуска
                if (IsEnabledReleaseYear.IsChecked == true)
                {
                    if (IsCloserValueReleaseYearCheck.IsChecked == true)
                    {
                        // Фильтруем по приблизительным значениям года выпуска с погрешностью 1
                        filterByYear = (currentGame.ReleaseYear >= releaseYearFilterValue - 1) && (currentGame.ReleaseYear <= releaseYearFilterValue + 1);
                    }
                    else
                    {
                        // Фильтруем по точному значению года выпуска
                        filterByYear = currentGame.ReleaseYear == releaseYearFilterValue;
                    }
                }

                // Если текущая игра не существует
                if (currentGame == null)
                    return false;

                // Если нет поискового запроса и значения фильтров цены и года выпуска не изменены
                if (string.IsNullOrEmpty(searchText) && filterByPrice && filterByYear)
                    return true;

                // Если не выбран критерий поиска
                if (string.IsNullOrEmpty(selectedCriteria))
                {
                    // Применяем фильтр ко всем полям таблицы
                    return (currentGame.Title.ToLower().Contains(searchText) ||
                            currentGame.Genre.ToLower().Contains(searchText) ||
                            currentGame.Developer.ToLower().Contains(searchText) ||
                            currentGame.Publisher.ToLower().Contains(searchText) ||
                            currentGame.ReleaseYear.ToString().Contains(searchText))
                            && filterByPrice && filterByYear;
                }
                else
                {
                    // иначе применяем фильтр только к выбранному полю
                    switch (selectedCriteria)
                    {
                        case "Название":
                            return currentGame.Title.ToLower().Contains(searchText) && filterByPrice && filterByYear;
                        case "Жанр":
                            return currentGame.Genre.ToLower().Contains(searchText) && filterByPrice && filterByYear;
                        case "Разработчик":
                            return currentGame.Developer.ToLower().Contains(searchText) && filterByPrice && filterByYear;
                        case "Издатель":
                            return currentGame.Publisher.ToLower().Contains(searchText) && filterByPrice && filterByYear;
                        case "Год Выпуска":
                            return currentGame.ReleaseYear.ToString().Contains(searchText) && filterByPrice && filterByYear;
                        default:
                            return filterByPrice && filterByYear;
                    }
                };
            };
        }


        private void gamesPublisherTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
           
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

        private void ResultsButton_Click(object sender, RoutedEventArgs e)
        {
            // Создаем новый контекст данных
            using (var context = new gamestoreEntities())
            {
                // Выбираем разработчиков и количество игр для каждого из них в порядке убывания количества игр
                var gameQuery = from g in context.Games
                                group g by g.Developer into devGroup
                                orderby devGroup.Count() descending
                                select new { Developer = devGroup.Key, GameCount = devGroup.Count() };

                // Выбираем издателей и количество игр для каждого из них в порядке убывания количества игр
                var publisherQuery = from g in context.Games
                                     group g by g.Publisher into pubGroup
                                     orderby pubGroup.Count() descending
                                     select new { Publisher = pubGroup.Key, GameCount = pubGroup.Count() };


                // Выбираем года выпуска и количество игр для каждого года в порядке убывания количества игр
                var yearQuery = from g in context.Games
                                group g by g.ReleaseYear into yearGroup
                                orderby yearGroup.Count() descending
                                select new { ReleaseYear = yearGroup.Key, GameCount = yearGroup.Count() };

                // Выбираем жанры и количество игр для каждого из них в порядке убывания количества игр
                var genreQuery = from g in context.Games
                                 group g by g.Genre into genreGroup
                                 orderby genreGroup.Count() descending
                                 select new { Genre = genreGroup.Key, GameCount = genreGroup.Count() };

                // Формируем строку с результатами запросов
                string result = $"Разработчик с наибольшим количеством игр - {gameQuery.First().Developer}\n";
                result += $"Издатель с наибольшим количеством игр - {publisherQuery.First().Publisher}\n";
                result += $"Год, в котором было выпущено больше всего игр - {yearQuery.First().ReleaseYear}\n";
                result += $"Самый популярный жанр - {genreQuery.First().Genre}";

                // Отображаем результаты запросов в диалоговом окне сообщения
                MsgBoxInfo.Show(result);
            }

        }

        private void StatisticButton_Click(object sender, RoutedEventArgs e)
        {
            // Открываем соединение с базой данных "gamestoreEntities"
            using (var context = new gamestoreEntities())
            {
                // Получаем общее количество игр в базе данных
                int totalGames = context.Games.Count();
                // Получаем общее количество уникальных разработчиков в базе данных
                int totalDevelopers = context.Games.Select(g => g.Developer).Distinct().Count();

                // Получаем общее количество уникальных издателей в базе данных
                int totalPublishers = context.Games.Select(g => g.Publisher).Distinct().Count();

                // Формируем строку с результатами запроса
                string result = $"Общее количество игр: {totalGames}\nОбщее количество разработчиков: {totalDevelopers}\nОбщее количество издателей: {totalPublishers}";

                // Отображаем сообщение с результатами запроса
                MsgBoxInfo.Show(result);
            }
        }

        private void AverageButton_Click(object sender, RoutedEventArgs e)
        {
            // Открываем соединение с базой данных "gamestoreEntities"
            using (var context = new gamestoreEntities())
            {
                // Вычисляем среднюю цену всех игр в базе данных
                double averagePrice = Convert.ToDouble(context.Games.Average(g => g.Price));
                // Форматируем среднюю цену игр с двумя знаками после запятой
                string formattedPrice = averagePrice.ToString("F2");

                // Формируем строку с результатами запроса
                string result = $"Средняя цена игр: {formattedPrice} $";

                // Отображаем сообщение с результатами запроса
                MsgBoxInfo.Show(result);
            }
        }

        private void MinimumButton_Click(object sender, RoutedEventArgs e)
        {
            using (var context = new gamestoreEntities())
            {
                // Самая дешевая игра
                var cheapestGame = context.Games.OrderBy(g => g.Price).First();
                string cheapestGameTitle = cheapestGame.Title;
                decimal cheapestGamePrice = cheapestGame.Price;

                // Самая ранняя игра по году выпуска
                var earliestGame = context.Games.OrderBy(g => g.ReleaseYear).First();
                string earliestGameTitle = earliestGame.Title;
                int earliestGameReleaseYear = earliestGame.ReleaseYear;

                // Отображаем сообщение с результатами запроса
                MsgBoxInfo.Show($"Самая дешевая игра: {cheapestGameTitle} ({cheapestGamePrice:C})\n" +
                                 $"Самая ранняя игра по году выпуска: {earliestGameTitle} ({earliestGameReleaseYear})");
            }
        }

        private void MaximumButton_Click(object sender, RoutedEventArgs e)
        {
            // Открываем соединение с базой данных "gamestoreEntities"
            using (var context = new gamestoreEntities())
            {
                // Находим самую дорогую игру
                var mostExpensiveGame = context.Games.OrderByDescending(g => g.Price).FirstOrDefault();
                string mostExpensiveGameTitle = mostExpensiveGame?.Title ?? "Нет данных";
                decimal mostExpensiveGamePrice = mostExpensiveGame?.Price ?? 0;

                // Находим самый свежий год выпуска игр
                int latestReleaseYear = context.Games.Max(g => g.ReleaseYear);

                MsgBoxInfo.Show($"Самая дорогая игра: {mostExpensiveGameTitle} - {mostExpensiveGamePrice:F2}\nСамый свежий год выпуска игр: {latestReleaseYear}");
            }
        }

        private void ReportButton_Click(object sender, RoutedEventArgs e)
        {
            // Создаем новый экземпляр отчета
            var report = new GamesDataReport();

            // Открываем отчет в окне просмотра отчетов
            ReportPrintTool printTool = new ReportPrintTool(report);
            printTool.ShowPreviewDialog();
        }


        private void Filter_Click(object sender, RoutedEventArgs e)
        {
            // Открываем всплывающее окно с фильтрами
            filterPopup.IsOpen = true;
        }


        private void PriceFilterSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Обновляем значение текстового поля с выбранной ценой
            PriceValueFilter.Text = e.NewValue.ToString("F2");
        }



        private void ReleaseYearFilterSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Обновляем значение текстового поля с выбранным годом
            ReleaseYearValueFilter.Text = e.NewValue.ToString();
        }


        private void ApplyFilterButton_Click(object sender, RoutedEventArgs e)
        {
            // Закрываем всплывающее окно с фильтрами
            filterPopup.IsOpen = false;
            // Применяем выбранный фильтр к играм
            ApplyGamesFilter();
        }


        private void CancelFilterButton_Click(object sender, RoutedEventArgs e)
        {
            // Закрываем всплывающее окно с фильтрами
            filterPopup.IsOpen = false;
            // Загружаем все игры без фильтрации
            loadGamesData();
        }


        


        private void TabItem_KeyDown(object sender, KeyEventArgs e) {    }
        private void purchasesSearchBox_TextChanged(object sender, TextChangedEventArgs e){}


        //Распределитель ролей
        private void CheckUserRole(string role)
        {
            if (role == "admin")
            {
                // Для пользователя с ролью "admin" все элементы управления должны быть доступны, поэтому ничего не делаем
            }
            else if (role == "manager")
            {
                // Отключаем соответствующие элементы управления: Админ-панель, ТекстБоксы, удаление и изменение
                adminPanelTabItem.Visibility = Visibility.Collapsed;
                gamesDeveloperTextBox.IsEnabled = false;
                gamesIDTextBox.IsEnabled = false;
                gamesGenreTextBox.IsEnabled = false;
                gamesPriceTextBox.IsEnabled = false;
                gamesPublisherTextBox.IsEnabled = false;
                gamesReleaseYearTextBox.IsEnabled = false;
                gamesTitleTextBox.IsEnabled = false;
                SaveButton.IsEnabled = false;
                SaveButton.Background = new SolidColorBrush(Color.FromArgb(255, 186, 186, 186)); 
                DeleteButton.IsEnabled = false;
                DeleteButton.Background = new SolidColorBrush(Color.FromArgb(255, 186, 186, 186));
            }

            if(role == "user")
            {
                // Отключаем соответствующие элементы управления: Админ-панель, ТекстБоксы, и кнопки манипуляции с данными
                adminPanelTabItem.Visibility = Visibility.Collapsed;
                gamesDeveloperTextBox.IsEnabled = false;
                gamesIDTextBox.IsEnabled = false;
                gamesGenreTextBox.IsEnabled = false;
                gamesPriceTextBox.IsEnabled = false;
                gamesPublisherTextBox.IsEnabled = false;
                gamesReleaseYearTextBox.IsEnabled = false;
                gamesTitleTextBox.IsEnabled = false;
                SaveButton.IsEnabled = false;
                SaveButton.Background = new SolidColorBrush(Color.FromArgb(255, 186, 186, 186));
                DeleteButton.IsEnabled = false;
                DeleteButton.Background = new SolidColorBrush(Color.FromArgb(255, 186, 186, 186));
                AddNewButton.IsEnabled = false;
                AddNewButton.Background = new SolidColorBrush(Color.FromArgb(255, 186, 186, 186));
            }
        }


        //Выход из окна БД
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            //Добавление лога
            Log("Пользователь завершил сессию");

            //Отрытие окна авторизации
            Window LoginWindow = new MainWindow();
            LoginWindow.Show();
            
            //Закрытие текущего окна 
            this.Close();
        }

        //Сброс фильтров
        private void gamesReset_Click(object sender, RoutedEventArgs e)
        {
            //Обновление и вывод всего списка игр без фильтрации
            loadGamesData();
            //Очистка SearchBox и SearchComboBox
            gamesSearchBox.Text = "";
            gamesSearchComboBox.Text = "";
        }


        //Ключение фильтрации по цене
        private void IsEnabledPrice_Checked(object sender, RoutedEventArgs e)
        {
            PriceFilterSlider.IsEnabled = true;
            IsCloserValuePriceCheck.IsEnabled = true;
            priceLabelFilterPopup.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000000"));
        }


        //Выключение фильтрации по цене
        private void IsEnabledPrice_Unchecked(object sender, RoutedEventArgs e)
        {
            PriceFilterSlider.IsEnabled = false;
            IsCloserValuePriceCheck.IsEnabled = false;
            IsCloserValuePriceCheck.IsChecked = false;
            priceLabelFilterPopup.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#616161"));

        }


        //Включение фильтрации по году выпуска
        private void IsEnabledReleaseYear_Checked(object sender, RoutedEventArgs e)
        {
            ReleaseYearFilterSlider.IsEnabled = true;
            IsCloserValueReleaseYearCheck.IsEnabled = true;
            releaseYearLabelFilterPopup.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000000"));
        }


        //Выключение фильтрации по году выпуска
        private void IsEnabledReleaseYear_Unchecked(object sender, RoutedEventArgs e)
        {
            ReleaseYearFilterSlider.IsEnabled = false;
            IsCloserValueReleaseYearCheck.IsEnabled = false;
            IsCloserValueReleaseYearCheck.IsChecked = false;
            releaseYearLabelFilterPopup.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#616161"));
        }


        //Заполнение текстбоксов данными из выделенной строки 
        private void dataBaseUsersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Если строка выделена, т.е. не равна null
            if (dataBaseUsersDataGrid.SelectedItem != null)
            {
                // Получить выбранный элемент
                var dbUser = dataBaseUsersDataGrid.SelectedItem as DataBaseUsers;

                if (dbUser != null)
                {
                    // Установить значения TextBox
                    loginDBUserTextBox.Text = dbUser.Username;
                    passwordDBUserTextBox.Text = dbUser.Password;
                    DbUserRoleComboBox.Text = dbUser.UserRole;
                }
            }
        }

        //Включение редактирования данных 
        private void editDBUserBtn_Click(object sender, RoutedEventArgs e)
        {
            //Включение полей для изменений информации
            loginDBUserTextBox.IsReadOnly = false;
            passwordDBUserTextBox.IsReadOnly=false;
            DbUserRoleComboBox.IsReadOnly=false;
            DbUserRoleComboBox.IsEnabled=true;
            //Включение кнопок сохранения и отмены
            SaveUserDBBtn.IsEnabled = true;
            CancelUserDBBtn.IsEnabled=true;
            //Выключение кнопки редактирования
            editDBUserBtn.IsEnabled=false;
        }

        private void SaveUserDBBtn_Click(object sender, RoutedEventArgs e)
        {
            //Выключение полей для редактирования
            loginDBUserTextBox.IsReadOnly = true ;
            passwordDBUserTextBox.IsReadOnly = true;
            DbUserRoleComboBox.IsReadOnly = true;
            DbUserRoleComboBox.IsEnabled =false;
            //Выключение кнопок сохранения изменений и отмены
            SaveUserDBBtn.IsEnabled = false;
            CancelUserDBBtn.IsEnabled = false;
            //Включение кнопки редактирования
            editDBUserBtn.IsEnabled = true;

           //Переменная для хранения старых данных об пользователе, до их изменения, используется для логгера
          var oldUserData = new DataBaseUsers();
            

            // Получить выбранный объект пользователя
            var selectedUser = dataBaseUsersDataGrid.SelectedItem as DataBaseUsers;
            //Если пользователь не выбран
            if (selectedUser == null)
            {
                MessageBox.Show("Выделите пользователя для изменения.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            //Присванивание страх данных об пользователе 
            oldUserData.Username =selectedUser.Username;
            oldUserData.UserRole = selectedUser.UserRole;
            oldUserData.Password = selectedUser.Password;

           // Обновить свойства объекта пользователя данными из текстбоксов и комбобокса
           selectedUser.Username = loginDBUserTextBox.Text;
            selectedUser.Password = passwordDBUserTextBox.Text;
            selectedUser.UserRole = DbUserRoleComboBox.Text;

            // Предупредить пользователя о сохранении изменений
            var result = MessageBox.Show("Хотите сохранить изменения?", "Сохранить", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
            {
                // Если пользователь отказался сохранять изменения, просто обновляем данные на экране
                loadDBUsersData();
                return;
            }

            // Сохранить изменения в базе данных
            // Открываем соединение с базой данных "DbUsersEntities"
            using (var context = new DbUsersEntities())
            {
                // Изменяем состояние объекта выбранного пользователя на "изменено"
                context.Entry(selectedUser).State = EntityState.Modified;
                try
                {
                    // Сохраняем изменения в базу данных
                    context.SaveChanges();
                    // Выводим сообщение об успехе
                    MessageBox.Show("Пользователь успешно изменен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    // Формируем строку для логирования изменений
                    string log = $"Изменение данных пользователя ID={selectedUser.UserID}  ||  ";

                    // Проверяем, было ли изменено свойство "Username"
                    if (oldUserData.Username != selectedUser.Username)
                    {
                        // Добавляем измененное значение свойства "Username" в строку для логирования
                        log += $"Логин: {oldUserData.Username}>>>{selectedUser.Username}";
                    }
                    // Проверяем, было ли изменено свойство "Password"
                    if (oldUserData.Password != selectedUser.Password)
                    {
                        // Добавляем измененное значение свойства "Password" в строку для логирования
                        log += $"Пароль: {oldUserData.Password}>>>{selectedUser.Password}";
                    }
                    // Проверяем, было ли изменено свойство "UserRole"
                    if (oldUserData.UserRole != selectedUser.UserRole)
                    {
                        // Добавляем измененное значение свойства "UserRole" в строку для логирования
                        log += $"Роль: {oldUserData.UserRole}>>>{selectedUser.UserRole}";
                    }
                    // Вызываем метод логирования и передаем ему сформированную строку для логирования
                    Log(log);
                }
                catch (Exception ex)
                {
                    // Выводим сообщение об ошибке, если сохранение произошло с ошибкой
                    MessageBox.Show($"Save error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            // Обновить данные на экране
            loadDBUsersData();
        }


        //Отмена изменений данных о пользователе
        private void CancelUserDBBtn_Click(object sender, RoutedEventArgs e)
        {
            //Выключение полей для редактирования
            loginDBUserTextBox.IsReadOnly = true;
            passwordDBUserTextBox.IsReadOnly = true;
            DbUserRoleComboBox.IsReadOnly = true;
            DbUserRoleComboBox.IsEnabled = false;
            //Выключение кнопок сохранения изменений и отмены
            SaveUserDBBtn.IsEnabled = false;
            CancelUserDBBtn.IsEnabled = false;
            //Включение кнопки редактирования
            editDBUserBtn.IsEnabled = true;


        }

        private void addNewDBUserBtn_Click(object sender, RoutedEventArgs e)
        {
            // Создаем новое окно для добавления нового пользователя
            AddNewDBUserWindow addnewUserWindow = new AddNewDBUserWindow();
            // Отображаем окно как диалоговое, блокируя взаимодействие пользователя с главным окном, пока не закроется диалоговое
            addnewUserWindow.ShowDialog();

            // Получаем нового пользователя из окна
            DataBaseUsers NewUser = addnewUserWindow.NewUser;

            // Логируем добавление нового пользователя с его ID
            Log($"Пользователь ID = {NewUser.UserID} добавлен");

            // Загружаем данные о пользователях в таблицу
            loadDBUsersData();
        }

        private void deleteDBUserBtn_Click(object sender, RoutedEventArgs e)
        {
            // Получаем выбранный пользователь в таблице
            var selectedUser = dataBaseUsersDataGrid.SelectedItem as DataBaseUsers;

            if (selectedUser == null)
            {
                MessageBox.Show("Пользователь не выбран.");
                return;
            }

            using (var context = new DbUsersEntities())
            {
                // Получаем объект пользователя, который нужно удалить
                var user = context.DataBaseUsers.FirstOrDefault(u => u.UserID == selectedUser.UserID);

                // Если объект найден, удаляем его из базы данных
                if (user != null)
                {
                    context.DataBaseUsers.Remove(user);

                    try
                    {
                        context.SaveChanges();
                        Log($"Пользователь ID = {user.UserID} удален");
                        
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Delete error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Обновляем отображение таблицы пользователей
                    loadDBUsersData();

                }
            }
           
        }

        private void createBackupBtn_Click(object sender, RoutedEventArgs e)
        {
            // Создание диалогового окна выбора пути сохранения бэкапа
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "backup"; // Имя по умолчанию
            dlg.DefaultExt = ".bak"; // Расширение файла
            dlg.Filter = "Backup Files (.bak)|*.bak"; // Фильтр файлов
            Nullable<bool> result = dlg.ShowDialog(); // Открытие диалогового окна

            // Если пользователь выбрал путь для сохранения
            if (result == true)
            {
                // Получение пути к файлу
                string filename = dlg.FileName;

                // Создание строки подключения
                string connectionString = "Data Source=HOME-PC\\SQLEXPRESS;Initial Catalog=gamestore;Integrated Security=True";

                // Создание экземпляра класса SqlConnection с использованием строки подключения
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    // Создание команды бэкапа
                    string backupQuery = string.Format("BACKUP DATABASE [{0}] TO DISK='{1}'", connection.Database, filename);
                    using (SqlCommand command = new SqlCommand(backupQuery, connection))
                    {
                        try
                        {
                            // Открытие соединения
                            connection.Open();

                            // Выполнение команды бэкапа
                            command.ExecuteNonQuery();

                            // Закрытие соединения
                            connection.Close();

                            // Оповещение об успешном создании бэкапа
                            MessageBox.Show("Бэкап успешно создан.", "Создание бэкапа", MessageBoxButton.OK, MessageBoxImage.Information);
                            Log($"Создан бэкап по пути: {filename} ");
                        }
                        catch (Exception ex)
                        {
                            // Оповещение об ошибке создания бэкапа
                            MessageBox.Show("Ошибка создания бэкапа: " + ex.Message, "Создание бэкапа", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }

        private void restoreDataBaseBtn_Click(object sender, RoutedEventArgs e)
        {
            // Открываем диалог выбора файла
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Backup files (*.bak)|*.bak|All files (*.*)|*.*",
                Title = "Select backup file"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                // Получаем путь к выбранному файлу
                var backupFilePath = openFileDialog.FileName;

                // Закрываем все соединения с базой данных
                using (var context = new gamestoreEntities())
                {
                    var connection = context.Database.Connection;
                    if (connection.State.HasFlag(ConnectionState.Open))
                    {
                        connection.Close();
                    }
                }

                // Выполняем операцию восстановления из файла бэкапа
                var restoreQuery = $"USE master RESTORE DATABASE gamestore FROM DISK = '{backupFilePath}' WITH REPLACE;";
                using (var context = new gamestoreEntities())
                {
                    context.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, restoreQuery);
                }

                // Оповещаем пользователя об успешном восстановлении базы данных
                MessageBox.Show("База данных успешно восстановлена.");
                Log($"Загружен бэкап по пути: {backupFilePath} ");
            }

            loadDBUsersData();
            loadGamesData();
        }

        private void searchDBUserBtn_Click(object sender, RoutedEventArgs e)
        {
            searchUserPopup.IsOpen = true;
        }

        private void ApplySearchUserButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedField;

            // Получаем выбранное поле
            if (searchDBUserFieldsComboBox.Text == "")
            {
                selectedField = null;
            }
            else
            {
                selectedField = ((ComboBoxItem)searchDBUserFieldsComboBox.SelectedItem).Content.ToString();
            }

            // Получаем искомый текст
            string searchText = searchUserTextBox.Text;

            // Формируем запрос к базе данных с условием фильтрации по выбранному полю и искомому тексту
            using (var context = new DbUsersEntities())
            {
                var filteredUsers = context.DataBaseUsers.ToList();

                filteredUsers = context.DataBaseUsers.Where(user => (selectedField == null || selectedField == "ID") && user.UserID.ToString().Contains(searchText) ||
                                                                     (selectedField == null || selectedField == "Логин") && user.Username.Contains(searchText) ||
                                                                     (selectedField == null || selectedField == "Роль") && user.UserRole.Contains(searchText)).ToList();

                // Присваиваем результат запроса DataGrid.ItemsSource
                dataBaseUsersDataGrid.ItemsSource = filteredUsers;
            }

            // Закрываем popup
            searchUserPopup.IsOpen = false;
        }



        private void Grid_ContextMenuClosing(object sender, ContextMenuEventArgs e)
        {

        }

        private void CancelSearchUserButton_Click(object sender, RoutedEventArgs e)
        {
            // Загружаем данные пользователей из базы данных
            loadDBUsersData();
            // Устанавливаем источник данных для элемента управления таблицей dataBaseUsersDataGrid
            dataBaseUsersDataGrid.ItemsSource = _dbusers;

            // Закрываем всплывающее окно поиска пользователей
            searchUserPopup.IsOpen = false;

            // Очищаем текстовое поле поиска пользователей
            searchUserTextBox.Text = "";

            // Очищаем выпадающий список полей поиска пользователей
            searchDBUserFieldsComboBox.Text = "";
        }

        private void Log (string message)
        {
            var timestamp = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
            var logEntry = $"{timestamp} | {LoggedUser} | {message}{Environment.NewLine}";

            // Проверяем, есть ли такая запись уже в файле
            var logFile = "logs.txt";
            var existingLogs = File.ReadAllText(logFile);
            if (existingLogs.Contains(logEntry))
            {
                return;
            }

            // Добавляем запись в файл
            File.AppendAllText(logFile, logEntry);

            // Добавляем запись в RichTextBox
            var paragraph = new Paragraph();
            var run = new Run(logEntry);
            paragraph.Inlines.Add(run);
            Logger.Document.Blocks.Add(paragraph);
        }



        private void LoadLogsFromFile()
        {
            string path = "logs.txt";
            if (File.Exists(path))
            {
                string[] lines = File.ReadAllLines(path);
                foreach (string line in lines)
                {
                    Paragraph paragraph = new Paragraph(new Run(line));
                    Logger.Document.Blocks.Add(paragraph);
                }
            }
        }

        

        private void UsersReport_Checked(object sender, RoutedEventArgs e)
        {
            // Создание экземпляра отчета "Список пользователей"
            ListOfGameStoreUsersReport usersReport = new ListOfGameStoreUsersReport();

            // Установка отчета для просмотра
            ReportViewer.DocumentSource = usersReport;
            usersReport.CreateDocument();
        }

        private void GamesReport_Checked(object sender, RoutedEventArgs e)
        {
            // Создание экземпляра отчета "Список игр"
            GamesDataReport gamesReport = new GamesDataReport();

            // Установка отчета для просмотра
            ReportViewer.DocumentSource = gamesReport;
            gamesReport.CreateDocument();
        }








        private void ReportViewer_Loaded(object sender, RoutedEventArgs e)
        {
          
        }

       

        private void purchasesReport_Checked(object sender, RoutedEventArgs e)
        {
            UsersPurchasesReport purchasesReport = new UsersPurchasesReport();

            ReportViewer.DocumentSource = purchasesReport;
            purchasesReport.CreateDocument();
        }





        private void ShowGamesComparisonByGenre()
        {
            using (var context = new gamestoreEntities())
            {
                ChartSeriesCollection.Clear(); // Очистка коллекции перед добавлением новых данных

                var gamesByGenre = context.Games
                    .GroupBy(g => g.Genre)
                    .Select(g => new { Genre = g.Key, Count = g.Count() })
                    .OrderByDescending(g => g.Count);

                foreach (var genre in gamesByGenre)
                {
                    ChartSeriesCollection.Add(new PieSeries
                    {
                        Title = genre.Genre,
                        Values = new ChartValues<int> { genre.Count },
                        DataLabels = true
                    });
                }
            }
        }

        private void GamesChartsDiagramm_Checked(object sender, RoutedEventArgs e)
        {
            ShowGamesComparisonByGenre();
        }

        private void ShowGamesComparisonByYear()
        {
            using (var context = new gamestoreEntities())
            {
                ChartSeriesCollection.Clear(); // Очистка коллекции перед добавлением новых данных

                var gamesByYear = context.Games
                    .GroupBy(g => g.ReleaseYear)
                    .Select(g => new { Year = g.Key, Count = g.Count() })
                    .OrderBy(g => g.Year);

                foreach (var year in gamesByYear)
                {
                    ChartSeriesCollection.Add(new PieSeries
                    {
                        Title = year.Year.ToString(),
                        Values = new ChartValues<int> { year.Count },
                        DataLabels = true
                    });
                }
            }
        }
        private void GamesCountYearDiagramm_Checked(object sender, RoutedEventArgs e)
        {
            ShowGamesComparisonByYear();
        }

        private void DiagramsTabItem_Loaded(object sender, RoutedEventArgs e)
        {
            ShowGamesComparisonByGenre();
        }
    }
}
