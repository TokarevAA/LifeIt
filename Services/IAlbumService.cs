using System.Threading.Tasks;

namespace LifeIt.Services
{
	public interface IAlbumService
	{
		Task<(Status, string[])> FetchAlbumsAsync(string artist);
	}
}