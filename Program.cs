using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace #NAME_SPACE#
{
    public class Program
    {
        private static string getValor(string nome)
        {
            string? v = Environment.GetEnvironmentVariable(nome);
            if (v == null)
                return "";
            else
                return v;
        }
        public static void Main(string[] args)
        {
            Console.WriteLine("Iniciando:");
            Console.WriteLine("ASPNETCORE_URLS: {0}", getValor("ASPNETCORE_URLS"));
            Console.WriteLine("PORTA: {0}", getValor("PORT"));
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseContentRoot(Path.Combine(Directory.GetCurrentDirectory()));
                    webBuilder.UseIISIntegration();
                    webBuilder.UseStartup<Startup>();
                });
    }
}
