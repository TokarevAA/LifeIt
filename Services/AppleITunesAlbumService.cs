using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using LifeIt.Dtos;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LifeIt.Services
{
	public class AppleITunesAlbumService : IAlbumService
	{
		private readonly HttpClient _httpClient;
		private readonly ILogger    _logger;


		public AppleITunesAlbumService(HttpClient httpClient, ILogger<AppleITunesAlbumService> logger)
		{
			_httpClient = httpClient;
			_logger = logger;
		}


		public async Task<IEnumerable<string>?> FetchAlbums(string artist)
		{
			string? artistId = await GetArtistId(artist);
			
			if (artistId == null)
			{
				_logger.LogWarning($"Unable to resolve artist id for '{artist}'");
				return null;
			}

			return await GetArtistAlbums(artistId);
		}
		
		
		private async Task<string?> GetArtistId(string artist)
		{
			string normalizedArtist = artist.Replace(" ", "+");
			Wrapper[]? wrappers = await ProcessQuery($"search?term={normalizedArtist}&limit=1");

			return wrappers?.FirstOrDefault()?.ArtistId;
		}

		private async Task<IEnumerable<string>?> GetArtistAlbums(string artistId)
		{
			Wrapper[]? wrappers = await ProcessQuery($"lookup?id={artistId}&entity=album");

			return wrappers?
				.Where(w => w.CollectionType == "Album")
				.Select(w => w.CollectionName);
		}

		private async Task<Wrapper[]?> ProcessQuery(string query)
		{
			HttpResponseMessage searchResponse = await _httpClient.GetAsync(query);

			if (!searchResponse.IsSuccessStatusCode)
			{
				return null;
			}

			await using Stream searchResponseContent = await searchResponse.Content.ReadAsStreamAsync();
			
			using var streamReader = new StreamReader(searchResponseContent);
			using var jsonTextReader = new JsonTextReader(streamReader);

			JsonSerializer serializer = new JsonSerializer();
			var result = serializer.Deserialize<Result>(jsonTextReader);

			return result!.Results;
		}
	}
}