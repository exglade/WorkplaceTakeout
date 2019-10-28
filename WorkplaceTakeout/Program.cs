using Facebook;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using WorkplaceTakeout.CustomTakeout;

namespace WorkplaceTakeout
{
    internal static class Program
    {
        public static IConfigurationRoot Configuration;

        private static void Main(string[] args)
        {
            Startup();

            var client = new FacebookClient(Configuration["facebook:accessToken"]);

            DoTakeout(client);
        }

        private static void DoTakeout(FacebookClient client)
        {
            var takeout = new Extractor(client, Configuration["takeout:OutputFolderPath"]);

            var task = takeout.RunAsync();

            task.Wait();
        }

        private static void RunTestRun(FacebookClient client)
        {
            // For test run

            SimpleWorkplaceGraphApiDemo.OutputFolderPath = Configuration["demo:OutputFolderPath"];
            SimpleWorkplaceGraphApiDemo.GroupId = Configuration["demo:GroupId"];
            SimpleWorkplaceGraphApiDemo.PostId = Configuration["demo:PostId"];
            SimpleWorkplaceGraphApiDemo.MemberId = Configuration["demo:MemberId"];
            SimpleWorkplaceGraphApiDemo.SkillId = Configuration["demo:SkillId"];
            SimpleWorkplaceGraphApiDemo.EventId = Configuration["demo:EventId"];
            SimpleWorkplaceGraphApiDemo.RunAllEdges(client);
        }

        private static void Startup()
        {
            var environmentName = Environment.GetEnvironmentVariable("ENVIRONMENT");

            Console.WriteLine($"Bootstrapping application using environment {environmentName}");

            // Build config

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true);
            Configuration = builder.Build();
        }
    }
}