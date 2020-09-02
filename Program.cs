using System;
using System.IO;
using System.Threading.Tasks;
using LifeIt.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NeoSmart.Caching.Sqlite;

namespace LifeIt
{
	internal class Program
	{
		private static async Task Main()
		{
			using IHost host = BuildHost();
			using IServiceScope servicesScope = host.Services.CreateScope();

			var logger = servicesScope.ServiceProvider
				.GetRequiredService<ILoggerFactory>()
				.CreateLogger<Program>();
			
			logger.LogDebug("Startup");
			
			try
			{
				string artist = ReadArtist();
			
				var albumService = servicesScope.ServiceProvider.GetRequiredService<IAlbumService>();
				IEnumerable<string>? albums = await albumService.FetchAlbums(artist);
			
				if (albums == null)
				{
					Console.WriteLine($"Unable to fetch albums for '{artist}'");
				}
				else
				{
					foreach (string album in albums)
					{
						Console.WriteLine(album);
					}
				}
			}
			catch (Exception ex)
			{
				logger.LogCritical("Unexpected error: " + ex);
			}
				
			logger.LogDebug("Shutdown");
		}


		private static IHost BuildHost() =>
			new HostBuilder()
				.ConfigureAppConfiguration((context, builder) =>
				{
					builder.SetBasePath(AppContext.BaseDirectory);
					builder.AddJsonFile("appsettings.json", false, true);
				})
				.ConfigureServices((context, services) =>
				{
					services.AddSqliteCache(options => { options.CachePath = Path.Combine(Path.GetTempPath(), "albums_cache"); });
					services.AddHttpClient<IAlbumService, AppleITunesAlbumService>(client =>
					{
						client.BaseAddress = new Uri(context.Configuration["ITunesEndpoint"]);
					});
				})
				.ConfigureLogging(builder => builder.AddConsole())
				.Build();


		private static string ReadArtist()
		{
			while (true)
			{
				Console.Write("Enter artist: ");
				string artist = Console.ReadLine();

				if (!string.IsNullOrWhiteSpace(artist))
				{
					return artist;
				}
				
				Console.SetCursorPosition(0, Console.CursorTop - 1);
			}
		}
	}
}