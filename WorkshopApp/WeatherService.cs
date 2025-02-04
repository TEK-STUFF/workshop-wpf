using System;
using System.Configuration;
using System.Net.Http;
using System.Text.Json;
using System.Globalization;

namespace WorkshopApp
{
    internal struct WeatherData
    {
        public string City { get; set; }
        public string Description { get; set; }
        public double Temp { get; set; }
        public double FeelsLike { get; set; }
        public int Humidity { get; set; }
        public double WindSpeed { get; set; }
        public int WindDirection { get; set; }
    }

    internal class WeatherService
    {
        private readonly string ApiKey;
        private readonly HttpClient Client;
        private const string BaseWeatherUrl = "https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&appid={ApiKey}";
        private const string BaseGeoUrl = "http://api.openweathermap.org/geo/1.0/direct?q={cityName}&limit=1&appid={ApiKey}";

        public WeatherService()
        {
            ApiKey = ConfigurationManager.AppSettings["OpenWeatherMapApiKey"] ?? string.Empty;
            if (string.IsNullOrEmpty(ApiKey))
            {
                throw new Exception("OpenWeatherMap API key not found in App.config");
            }
            Client = new HttpClient();
        }

        private double ToCelcius(double kelvin)
        {
            return kelvin - 273.15;
        }

        public (string, string) ValidateCity(string city)
        {
            if (string.IsNullOrEmpty(city))
            {
                throw new Exception("Please enter a city name");
            }
            string requestUri = BaseGeoUrl.Replace("{cityName}", city).Replace("{ApiKey}", ApiKey);
            HttpResponseMessage response = Client.GetAsync(requestUri).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("City not found");
            }
            string content = response.Content.ReadAsStringAsync().Result;
            JsonDocument json = JsonDocument.Parse(content);
            double lat = json.RootElement[0].GetProperty("lat").GetDouble();
            double lon = json.RootElement[0].GetProperty("lon").GetDouble();
            string latStr = lat.ToString();
            string lonStr = lon.ToString();
            return (latStr, lonStr);
        }

        public WeatherData GetWeather(string city)
        {
            if (string.IsNullOrEmpty(city))
            {
                throw new Exception("Please enter a city name");
            }
            (string lat, string lon) = ValidateCity(city);
            if (string.IsNullOrEmpty(lat) || string.IsNullOrEmpty(lon))
            {
                throw new Exception("Invalid city coordinates");
            }
            string requestUri = BaseWeatherUrl.Replace("{lat}", lat).Replace("{lon}", lon).Replace("{ApiKey}", ApiKey);
            HttpResponseMessage response = Client.GetAsync(requestUri).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Weather data not found");
            }
            string content = response.Content.ReadAsStringAsync().Result;
            JsonDocument json = JsonDocument.Parse(content);
            JsonElement currentData = json.RootElement;
            JsonElement numericalData = currentData.GetProperty("main");
            JsonElement windData = currentData.GetProperty("wind");
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            string desc = currentData.GetProperty("weather")[0].GetProperty("description").GetString() ?? "Description not found";
            desc = textInfo.ToTitleCase(desc);
            double temp = ToCelcius(numericalData.GetProperty("temp").GetDouble());
            double feelsLike = ToCelcius(numericalData.GetProperty("feels_like").GetDouble());
            int humidity = numericalData.GetProperty("humidity").GetInt16();
            double windSpeed = windData.GetProperty("speed").GetDouble();
            int windDirection = windData.GetProperty("deg").GetInt32();
            WeatherData data = new()
            {
                City = city,
                Description = desc,
                Temp = temp,
                FeelsLike = feelsLike,
                Humidity = humidity,
                WindSpeed = windSpeed,
                WindDirection = windDirection
            };
            return data;
        }
    }
}
