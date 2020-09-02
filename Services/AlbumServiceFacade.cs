using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace LifeIt.Services
{
	public class AlbumServiceFacade
	{
		private const char SEPARATOR_CHAR = ',';


		private static readonly string _separatorString = SEPARATOR_CHAR.ToString();
		
		
		private readonly IAlbumService _albumService;
		private readonly IDistributedCache _persistentCache;


		public AlbumServiceFacade(IAlbumService albumService, IDistributedCache persistentCache)
		{
			_persistentCache = persistentCache;
			_albumService = albumService;
		}


		public async Task<string[]> FetchAlbums(string artist)
		{
			(Status status, string[]? albums) = await _albumService.FetchAlbumsAsync(artist);

			switch (status)
			{
				case Status.Ok:
					string cachedString = string.Join(_separatorString, albums);
					await _persistentCache.SetStringAsync(artist, cachedString);
					return albums;
				
				case Status.Failure:
					string? cachedAlbums = await _persistentCache.GetStringAsync(artist);
					return cachedAlbums == null ? Array.Empty<string>() : cachedAlbums.Split(SEPARATOR_CHAR);
				
				case Status.NoData:
					return Array.Empty<string>();
				
				default:
					throw new ArgumentOutOfRangeException(nameof(status));
					
			}
		}
	}
}