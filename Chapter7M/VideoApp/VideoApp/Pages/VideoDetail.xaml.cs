using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace VideoApp.Pages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class VideoDetail : ContentPage
	{
		public VideoDetail (Models.Video video)
		{
			InitializeComponent ();
		}
	}
}
