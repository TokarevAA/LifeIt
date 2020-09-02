using System;
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


		public async Task<(Status, string[])> FetchAlbumsAsync(string artist)
		{
			(Status Status, string? Id) artistResult = await GetArtistId(artist);
			
			if (artistResult.Status != Status.Ok)
			{
				_logger.LogWarning($"Unable to resolve artist id for '{artist}'");
				return (artistResult.Status, Array.Empty<string>());
			}

			return await GetArtistAlbums(artistResult.Id!);
		}
		
		
		private async Task<(Status, string?)> GetArtistId(string artist)
		{
			string normalizedArtist = artist.Replace(" ", "+");
			(bool success, Wrapper[] wrappers) = await ProcessQuery($"search?term={normalizedArtist}&limit=1");

			if (success)
			{
				Status status = wrappers.Length > 0 ? Status.Ok : Status.NoData;
				return (status, wrappers.ElementAtOrDefault(0)?.ArtistId);
			}

			return (Status.Failure, null);
		}

		private async Task<(Status, string[])> GetArtistAlbums(string artistId)
		{
			(bool success, Wrapper[] wrappers) = await ProcessQuery($"lookup?id={artistId}&entity=album");

			if (success)
			{
				Status status = wrappers.Length > 0 ? Status.Ok : Status.NoData;
				string[] albums = wrappers
					.Where(w => w.CollectionType == "Album")
					.Select(w => w.CollectionName)
					.ToArray();

				return (status, albums);
			}

			return (Status.Failure, Array.Empty<string>());
		}

		private async Task<(bool, Wrapper[])> ProcessQuery(string query)
		{
			HttpResponseMessage searchResponse;
			
			try
			{
				searchResponse = await _httpClient.GetAsync(query);
			}
			catch (HttpRequestException ex)
			{
				_logger.LogCritical("Exception during http request: " + ex);
				return (false, Array.Empty<Wrapper>());
			}

			if (!searchResponse.IsSuccessStatusCode)
			{
				_logger.LogWarning("Unsuccessful status code: " + searchResponse.StatusCode);
				return (false, Array.Empty<Wrapper>());
			}

			await using Stream searchResponseContent = await searchResponse.Content.ReadAsStreamAsync();
			
			using var streamReader = new StreamReader(searchResponseContent);
			using var jsonTextReader = new JsonTextReader(streamReader);

			JsonSerializer serializer = new JsonSerializer();
			var result = serializer.Deserialize<Result>(jsonTextReader);

			return (true, result!.Results);
		}
	}
}