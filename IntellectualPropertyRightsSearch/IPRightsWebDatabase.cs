using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;

using JetBrains.Annotations;

namespace IntellectualPropertyRightsSearch
{
	/// <summary>
	/// Searches for intellectual property rights on the <see href="http://iprs.cbp.gov/">IPRS</see> website.
	/// IPRS is a searchable database containing public versions of U.S. Customs and Border Protection 
	/// intellectual property rights recordations.
	/// </summary>
    public class IPRightsWebDatabase
		: IDisposable
    {
		/// <summary>
		/// Default URL to search for a IP right records mentioned in a title, description. Including already expired records.
		/// </summary>
		const string DefaultSearchUrlFormat = "http://iprs.cbp.gov/index.asp?ti=on&de=on&expdate=6&action=search&searcharg={0}";

		HttpClient _http;

		/// <summary>
		/// Initializes the database.
		/// </summary>
	    public IPRightsWebDatabase()
	    {
			// create and configure the HTTP client.
		    _http = CreateHttpClient();
	    }

		#region Private Methods
		/// <summary>
		/// Creates and configures the HTTP client that is ready to be used.
		/// </summary>
		/// <returns></returns>
		HttpClient CreateHttpClient()
		{
			var handler	= new HttpClientHandler { UseCookies = true, };
			var http	= new HttpClient( handler );
			// set default headers shared among requests.
			http.DefaultRequestHeaders.Host = "iprs.cbp.gov";
			http.DefaultRequestHeaders.Add( "Upgrade-Insecure-Requests", "1" );
			http.DefaultRequestHeaders.Add( "UserAgent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_10_5) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.106 Safari/537.36" );
			
			// without this request the search wont include expired registrations in the result.
			http.GetAsync( "http://iprs.cbp.gov/index.asp?expdate=6&action=search&searcharg=test" )
				.Result.Content.ReadAsStringAsync().Wait();

			// the HTTP client is created and configured.
			return	http;
		}
		/// <summary>
		/// Creates a URL of the page using the <see cref="DefaultSearchUrlFormat"/>. The page that should 
		/// has the registration information.
		/// </summary>
		/// <param name="searchText">The text to search for.</param>
		/// <returns>Page's content.</returns>
		string CreateUrl(string searchText)
		{
			// encode the text because it will be inserted into a URL.
			string searchArg = WebUtility.UrlEncode( searchText );
			return	string.Format( DefaultSearchUrlFormat, searchArg );
		}
		#endregion

		#region IDisposable
		/// <summary>
		/// Explisit disposing.
		/// </summary>
		public void Dispose()
	    {
		    GC.SuppressFinalize( this );
			Dispose( true );
	    }
		/// <summary>
		/// Either explisit or implisit disposing.
		/// </summary>
		/// <param name="expl"><b>True</b> when is being disposed explicitly; otherwise, <b>false</b>.</param>
		protected virtual void Dispose(bool expl)
		{
			if ( expl && null != _http ) {
				_http.Dispose(); _http = null;
			}
		}
	    #endregion

		/// <summary>
		/// Searches for any registration records on the IPRS website. Searches for any 
		/// mentions of the <paramref name="searchText"/> in a title or a description 
		/// including already expired registrations.
		/// </summary>
		/// <param name="searchText">The text to search for.</param>
		/// <returns><see cref="RegistrationInfo"/>.</returns>
		/// <exception cref="ArgumentException">The search text is not provided.</exception>
		/// <exception cref="Exception">Failed to parse registration info from the page.</exception>
		[NotNull]
		public RegistrationInfo SearchRegistration([NotNull]string searchText)
		{
			if ( string.IsNullOrWhiteSpace( searchText ) )
				throw new ArgumentException( "The search text is not provided.", paramName: nameof( searchText ) );

			// trim any spaces from the text.
			searchText			= searchText.Trim();
			// create a URL of the page that should contain information we need.
			string searchUrl	= CreateUrl( searchText );
			// load the page.
			string page			= _http.Get( searchUrl );

			// parse the registration information from the page.
			try {
				var reg = new Regex( @"Results \[\d+ - \d+\] of (\d+) record" );
				Match m = reg.Match( page );
				// create the info.
				var info = new RegistrationInfo( searchText, searchUrl );
				// check if there have been found any records.
				if ( m.Success )
					info.RecordsCount = int.Parse( m.Groups[ 1 ].Value );
				
				// done.
				return	info;
			} catch ( Exception ex ) {
				throw new Exception( "Failed to parse registration info from the page.", ex );
			}
		}
    }
}
