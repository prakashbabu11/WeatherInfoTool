using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace WeatherInfoTool.Tests
{
    public class ProgramTests
    {
        [Fact]
        public async Task Main_ValidCity_ReturnsWeatherInformation()
        {
            // Arrange
            string city = "Mumbai";
            string[] cityData = { "1,Mumbai,18.9667,72.8333" };
            string weatherData = "{ \"current_temperature\": 30, \"current_wind_speed\": 10 }";

            Mock<HttpClientHandler> mockHttpHandler = new Mock<HttpClientHandler>();
            mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(weatherData)
                });

            Mock<HttpClient> mockHttpClient = new Mock<HttpClient>(mockHttpHandler.Object);
            mockHttpClient
                .Setup(client => client.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .CallBase();

            Mock<FileService> mockFileService = new Mock<FileService>();
            mockFileService.Setup(service => service.ReadAllLinesAsync(It.IsAny<string>())).ReturnsAsync(cityData);

            using (mockHttpClient.Object)
            using (mockHttpHandler.Object)
            {
                // Act
                using (var consoleOutput = new ConsoleOutput())
                {
                    Console.SetIn(new StringReader(city));
                    Program.FileService = mockFileService.Object;
                    Program.HttpClientFactory = () => mockHttpClient.Object;
                    await Program.Main(null);
                    string output = consoleOutput.GetOutput();

                    // Assert
                    Assert.Contains("Temperature: 30°C", output);
                    Assert.Contains("Wind Speed: 10 m/s", output);
                }
            }
        }

        [Fact]
        public async Task Main_InvalidCity_DisplaysErrorMessage()
        {
            // Arrange
            string city = "InvalidCity";
            string[] cityData = { "1,Mumbai,18.9667,72.8333" };

            Mock<FileService> mockFileService = new Mock<FileService>();
            mockFileService.Setup(service => service.ReadAllLinesAsync(It.IsAny<string>())).ReturnsAsync(cityData);

            using (var consoleOutput = new ConsoleOutput())
            {
                Console.SetIn(new StringReader(city));
                Program.FileService = mockFileService.Object;

                // Act
                await Program.Main(null);
                string output = consoleOutput.GetOutput();

                // Assert
                Assert.Contains("City not found in the data storage", output);
            }
        }

        [Fact]
        public async Task Main_ApiError_DisplaysErrorMessage()
        {
            // Arrange
            string city = "Mumbai";
            string[] cityData = { "1,Mumbai,18.9667,72.8333" };

            Mock<HttpClientHandler> mockHttpHandler = new Mock<HttpClientHandler>();
            mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            Mock<HttpClient> mockHttpClient = new Mock<HttpClient>(mockHttpHandler.Object);
            mockHttpClient
                .Setup(client => client.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .CallBase();

            Mock<FileService> mockFileService = new Mock<FileService>();
            mockFileService.Setup(service => service.ReadAllLinesAsync(It.IsAny<string>())).ReturnsAsync(cityData);

            using (mockHttpClient.Object)
            using (mockHttpHandler.Object)
            using (var consoleOutput = new ConsoleOutput())
            {
                Console.SetIn(new StringReader(city));
                Program.FileService = mockFileService.Object;
                Program.HttpClientFactory = () => mockHttpClient.Object;

                // Act
                await Program.Main(null);
                string output = consoleOutput.GetOutput();

                // Assert
                Assert.Contains("Failed to retrieve weather information", output);
            }
        }
    }
}
