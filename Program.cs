using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace WeatherInfoTool
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                // Read the city input from the user
                Console.Write("Enter the city: ");
                string city = Console.ReadLine();

                // Fetch the latitude and longitude from the data storage
                string filePath = "city_data.csv";
                string[] cityDataLines = await File.ReadAllLinesAsync(filePath);

                string[] cityData = cityDataLines
                    .Skip(1) // Skip the header line
                    .Select(line => line.Split(',')) // Split each line by comma
                    .FirstOrDefault(data => data[1].Trim().Equals(city, StringComparison.OrdinalIgnoreCase)); // Find the matching city

                    foreach (string data in cityData)
{
    Console.WriteLine(data);
} 
                Console.Write("My name is Prakash: ");
                if (cityData != null)
                {
                    decimal latitude = decimal.Parse(cityData[2]);
                    decimal longitude = decimal.Parse(cityData[3]);

                    // Create the HTTP client
                    HttpClient client = new HttpClient();

                    // Fetch weather information using the latitude and longitude
                    string endpoint = $"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}&current_weather=true";
                    HttpResponseMessage response = await client.GetAsync(endpoint);

                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        dynamic weatherData = JsonConvert.DeserializeObject(json);

                        double temperature = weatherData.current_temperature;
                        double windSpeed = weatherData.current_wind_speed;

                        Console.WriteLine($"Temperature: {temperature}°C");
                        Console.WriteLine($"Wind Speed: {windSpeed} m/s");
                    }
                    else
                    {
                        Console.WriteLine("Failed to retrieve weather information. Please try again later.");
                    }
                }
                else
                {
                    Console.WriteLine("City not found in the data storage. Incovenience is deeply regreteds");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
