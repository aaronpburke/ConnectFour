using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace ConnectFour.Api
{
    /// <summary>
    /// Program
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main program.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Creates Web Host Builder.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        /// <returns>IWebHostBuilder object.</returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
