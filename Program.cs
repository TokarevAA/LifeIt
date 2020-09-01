using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LifeIt.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace LifeIt
{
	internal class Program
	{
		private static readonly IConfiguration _configuration =
			new ConfigurationBuilder()
				.SetBasePath(AppContext.BaseDirectory)
				.AddJsonFile("appsettings.json", false, true)
				.Build();
		
		
		private static async Task Main()
		{
			await using ServiceProvider serviceProvider = BuildServiceProvider();
			
			var logger = serviceProvider
				.GetRequiredService<ILoggerFactory>()
				.CreateLogger<Program>();

			logger.LogDebug("Startup");

			try
			{
				string artist = ReadArtist();

				var albumService = serviceProvider.GetRequiredService<IAlbumService>();
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


		private static ServiceProvider BuildServiceProvider()
		{
			var result = new ServiceCollection();

			result.AddLogging(config => config.AddConsole());
			result.AddHttpClient<IAlbumService, AppleITunesAlbumService>(client =>
			{
				client.BaseAddress = new Uri(_configuration["ITunesEndpoint"]);
			});

			return result.BuildServiceProvider();
		}
		
		
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