//using Microsoft.Azure.Functions.Extensions.DependencyInjection;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using NLog.Extensions.Logging;
//using System.IO;
//using System.Reflection;

//namespace SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure
//{
//    public static class EsfaLoggingExtension
//    {
//        public static void ConfigureLogging(this IFunctionsHostBuilder builder)
//        {
//            builder.Services.AddLogging(ConfigureLogging);
//        }

//        public static void ConfigureLogging(this ILoggingBuilder logBuilder)
//        {
//            // all logging is filtered out by defualt
//            logBuilder.AddFilter(typeof(Startup).Namespace, LogLevel.Information);
//            var rootDirectory = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, ".."));
//            var files = Directory.GetFiles(rootDirectory, "nlog.config", SearchOption.AllDirectories)[0];
//            logBuilder.AddNLog(files);
//        }
//    }
//}