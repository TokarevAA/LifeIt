using Newtonsoft.Json;

namespace LifeIt.Dtos
{
	public class Wrapper
	{
		[JsonProperty("artistId")]       public string ArtistId = null!;
		[JsonProperty("collectionType")] public string CollectionType = null!;
		[JsonProperty("collectionName")] public string CollectionName = null!;
	}
}