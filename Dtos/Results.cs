using Newtonsoft.Json;

namespace LifeIt.Dtos
{
	public class Result
	{
		[JsonProperty("results")]
		public Wrapper[] Results = null!;
	}
}