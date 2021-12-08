using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HomeWork_09_SKP
{
    /// <summary>
    /// Класс получения данных о погоде
    /// </summary>
    static class WeatherHerald
    {
        static string url = "http://api.openweathermap.org/data/2.5/weather?q=";

        static string keyAPI = "6fa095c114c44b8983cf448560847507";

        /// <summary>
        /// Запрос погоды по http
        /// </summary>
        /// <param name="city"></param>
        /// <returns></returns>
        static public string WeatherRequest(string city)
        {
           
            string urlByUser = $"{url}{city}&units=metric&appid={keyAPI}";

            string response;

            HttpWebRequest httpWebRequest;

            HttpWebResponse httpWebResponse;

            try
            {
                httpWebRequest = (HttpWebRequest)WebRequest.Create(urlByUser);

                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            }
            catch (Exception)
            {
                return "Invalid input";
            }

            using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
            {
                response = streamReader.ReadToEnd();
            }

            WeatherResponse weatherResponse = JsonConvert.DeserializeObject<WeatherResponse>(response);

            return $"Temperature in {weatherResponse.Name}: {weatherResponse.Main.Temp} °C";

        }

    }
        
    public class TemperatureInfo
    {
        public float Temp { get; set; }
    }
        
    public class WeatherResponse
    {
        public TemperatureInfo Main { get; set; }

        public string Name { get; set; }
    }
}
