namespace IntellectualPropertyRightsSearch
{
	/// <summary>
	/// A registration information regarding a <see cref="SearchText"/>.
	/// </summary>
	public class RegistrationInfo
	{
		/// <summary>
		/// The text that has been checked on the <see href="http://iprs.cbp.gov/">IPRS</see> website.
		/// </summary>
		public string SearchText
		{
			get;
		}
		public string SearchUrl
		{
			get;
		}
		/// <summary>
		/// Indicates if there are any registration records.
		/// </summary>
		public bool CanBeRegistered
			=> 0 != this.RecordsCount;
		/// <summary>
		/// The amount of found registration records.
		/// </summary>
		public int RecordsCount
		{
			get;
			set;
		}

		/// <summary>
		/// Initializes the info.
		/// </summary>
		/// <param name="searchText">The text that has been checked on the <see href="http://iprs.cbp.gov/">IPRS</see> website.</param>
		/// <param name="searchUrl">The URL that the <paramref name="searchText"/> was checked on.</param>
		public RegistrationInfo(string searchText, string searchUrl)
		{
			this.SearchText	= searchText;
			this.SearchUrl	= searchUrl;
		}
	}
}