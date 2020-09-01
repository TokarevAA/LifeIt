using System.Collections.Generic;
using System.Threading.Tasks;

namespace LifeIt.Services
{
	public interface IAlbumService
	{
		Task<IEnumerable<string>?> FetchAlbums(string artist);
	}
}