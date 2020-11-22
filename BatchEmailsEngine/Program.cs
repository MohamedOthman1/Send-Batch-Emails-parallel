using BatchEmailsEngine.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace BatchEmailsEngine
{
    class Program
    {
        static void Main(string[] args)
        {
            IConfiguration Configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();

            var serviceProvider = new ServiceCollection()
           .AddLogging()
           .AddSingleton<ISendingEmails, SendingEmails>()
           .AddSingleton<IConfiguration>(Configuration)
           .AddSingleton<ILogHelper, LogHelper>()
           .AddLogging(configure => configure.AddLog4Net())
           .BuildServiceProvider();

            IServiceScope scope = serviceProvider.CreateScope();
            scope.ServiceProvider.GetRequiredService<ISendingEmails>().Run();
        }
    }
}
