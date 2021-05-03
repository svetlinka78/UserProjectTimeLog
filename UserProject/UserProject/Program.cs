using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using static UserProject.UserProjectTimeLog;

namespace UserProject
{
    public static class Program
    {
        public static void ConfigureServices(this IServiceCollection services)
        {
            var m_strMySQLConnectionString =
                 ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();

            services
                    //.AddLogging(configure => configure.AddConsole())
                    .AddTransient<Seeder>()
                    .AddScoped<IBusinessLayer, BusinessLayer>()
                    .AddSingleton<IUserProjectTimeLog, UserProjectTimeLog>()
                    .AddDbContext<UserProjectTimeLogContext>(options =>
                    {
                        options.UseSqlServer(m_strMySQLConnectionString);
                    });

        }
        static async Task Main(string[] args)
        {

            var services = new ServiceCollection();
            ConfigureServices(services);
            using (ServiceProvider serviceProvider =
                                   services.BuildServiceProvider())
            {
                var app = serviceProvider.GetService<Seeder>();
                app.Clean();
                app.Seed();

                try
                {
                    var date = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ");
                    //await app.SeedAsync(DateTime.ParseExact(date, "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.CurrentCulture));
                    await app.SeedAsyncBC(DateTime.ParseExact(date, "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.CurrentCulture));
                    Console.WriteLine("Success");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error Occured");
                }

                
            }

        }


    }
   
}
