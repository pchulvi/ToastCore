using System.IO;
using Backend.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var configHosting = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("hosting.json", optional: true)
                .Build();

            ApplicationConfigurationsSettings settings = new ApplicationConfigurationsSettings();

            config.GetSection("ApplicationConfigurations").Bind(settings);

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls("http://*:5010") /*Esto es para que cuando utilicemos kestrel en linux nos de la dirección http://0.0.0.0:5010 */
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .Build();

            host.Run();
        }

        public class ApplicationConfigurationsSettings
        {
            public string ApplicationPort { get; set; }
        }

        public IOptions<ApplicationConfigurations> OptionsApplicationConfiguration { get; }
    }
}
