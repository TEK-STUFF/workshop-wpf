using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace WorkshopApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly WeatherService WeatherService = new();
        private readonly List<string> Cities =
        [
            "London",
            "Paris",
            "Berlin",
            "New York",
            "Tokyo",
            "Moscow",
            "Beijing",
            "Cairo",
            "Sydney",
            "Rio de Janeiro"
        ];
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            CitiesListView.ItemsSource = Cities;
        }

        private void SetWeatherInfo(WeatherData weatherData)
        {
            Title = $"Weather in {weatherData.City}";
            TxtCityName.Text = $"Weather in {weatherData.City}";
            TxtDescription.Text = weatherData.Description;
            TxtTemperature.Text = $"{double.Round(weatherData.Temp)}°C";
            TxtFeelsLike.Text = $"Feels like {double.Round(weatherData.FeelsLike)}°C";
            TxtHumidity.Text = $"{weatherData.Humidity}% humidity";
            TxtWind.Text = $"Wind is {weatherData.WindSpeed} m/s";
            TxtWindDirection.Text = $"{weatherData.WindDirection}°";
        }

        private void WeatherInfoQuery(string city)
        {
            WeatherData weatherData;
            try
            {
                weatherData = WeatherService.GetWeather(city);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            SetWeatherInfo(weatherData);
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            WeatherInfoQuery(TxtCity.Text);
        }

        private void CitiesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CitiesListView.SelectedItem is string city)
            {
                TxtCity.Text = city;
                WeatherInfoQuery(city);
            }
        }
    }
}
