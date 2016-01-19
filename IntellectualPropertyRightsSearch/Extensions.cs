using System.Net.Http;

namespace IntellectualPropertyRightsSearch
{
	/// <summary>
	/// Custom extensions.
	/// </summary>
	static class Extensions
	{
		/// <summary>
		/// Loads the content of the <paramref name="url"/> using the GET method synchronously.
		/// </summary>
		/// <param name="http"><see cref="HttpClient"/>.</param>
		/// <param name="url">The URL to get.</param>
		/// <returns>Content.</returns>
		public static string Get(this HttpClient http, string url)
		{
			return	http.GetAsync( url )
						.Result.Content.ReadAsStringAsync()
						.Result;
		}
	}
}
