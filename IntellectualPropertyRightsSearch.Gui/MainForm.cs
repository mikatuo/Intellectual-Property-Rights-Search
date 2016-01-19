using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IntellectualPropertyRightsSearch.Gui
{
	public partial class MainForm
		: Form
	{
		static readonly string NewLine = Environment.NewLine;

		readonly IPRightsWebDatabase _IpRightsWebDatabase;
		int _foundRegistrations;
		int _totalSearchItems;
		int _processedSearchItems;

		/// <summary>
		/// An option that indicates if each search token needs to be broken by spaces.
		/// </summary>
		public bool BreakBySpaces
		{
			get { return breakBySpaces.Checked; }
			set { breakBySpaces.Checked = value; }
		}

		public MainForm()
		{
			InitializeComponent();
			_IpRightsWebDatabase = new IPRightsWebDatabase();
			ResetState();
		}

		void ClearInput()
		{
			this.Input.Text = string.Empty;
		}
		void ClearOutput()
		{
			flowPanel.ResetText(); flowPanel.Controls.Clear();
		}
		void button1_Click(object sender, EventArgs e)
		{
			Disable( button1 );
			var tokens = GetInput();

			Task.Run( () => {
				Log( $"*** SEARCHING ***{NewLine}{NewLine}" );
				foreach ( string token in tokens ) {
					RegistrationInfo info = _IpRightsWebDatabase.SearchRegistration( token );
					OnItemProcessed();
					if ( !info.CanBeRegistered )
						continue;
					OnRegistrationInfoFound( info );
				}
			} ).ContinueWith( task => {
				Enable( button1 );
				ToggleVisibility( this.button1 );
				ToggleVisibility( this.button2 );
				Log( $"{NewLine}{NewLine}*** DONE ***" );
			} );
		}
		IEnumerable<string> GetInput()
		{
			var delimiters = new List<string> { NewLine, ",", "/" };
			if ( this.BreakBySpaces ) delimiters.Add( " " );
			var searchEntries = this.Input.Text.Split( delimiters.ToArray(), StringSplitOptions.RemoveEmptyEntries );
			ClearInput();
			_totalSearchItems = searchProgress.Maximum = searchEntries.Length;
			
			return	searchEntries;
		}
		void ResetState()
		{
			_foundRegistrations		= 0;
			_processedSearchItems	= 0;
			searchProgress.Maximum	= searchProgress.Value = 0;
		}
		void OnItemProcessed()
		{
			InvokeAction( () => {
				searchProgress.Value = ++_processedSearchItems;
			} );
		}
		void OnRegistrationInfoFound(RegistrationInfo info)
		{
			var delimiter = _foundRegistrations != 0 ? ", " : string.Empty;
			Log( $"{delimiter}{info.SearchText}" );
			InvokeAction( () => {
				LinkLabel link = CreateLinkLabel(
									text:	$"{info.SearchText} ( {info.RecordsCount} )",
									url:	info.SearchUrl );
				flowPanel.Controls.Add( link );
			} );
			++_foundRegistrations;
		}
		void Log(string text)
		{
			InvokeAction( () => {
				this.Input.Text += text;
			} );
		}
		void ToggleVisibility(Control control)
		{
			InvokeAction( () => { control.Visible = !control.Visible; } );
		}
		void Enable(Control control)
		{
			InvokeAction( () => control.Enabled = true );
		}
		void Disable(Control control)
		{
			InvokeAction( () => control.Enabled = false );
		}
		void button2_Click(object sender, EventArgs e)
		{
			this.Input.Text = string.Empty;
			ClearOutput(); ResetState();
			ToggleVisibility( this.button1 );
			ToggleVisibility( this.button2 );
		}
		void InvokeAction(Action act)
		{
			if ( InvokeRequired ) {
				Invoke( act );
			} else {
				act();
			}
		}
		LinkLabel CreateLinkLabel(string text, string url)
		{
			var link = new LinkLabel { Text = text };
			link.Links.Add( 0, link.Text.Length, url );
			link.LinkClicked += (o, arg) => {
				var sInfo = new ProcessStartInfo( arg.Link.LinkData.ToString() );
				Process.Start( sInfo );
			};
			return	link;
		}
	}
}
