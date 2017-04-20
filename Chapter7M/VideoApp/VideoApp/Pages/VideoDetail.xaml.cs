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

            var htmlSource = new HtmlWebViewSource();
            var sourceInfo = @"
<html>
    <head>
        <title>Test</title>
        <link href=""https://amp.azure.net/libs/amp/1.8.3/skins/amp-default/azuremediaplayer.min.css"" rel=""stylesheet"">
        <script src=""https://amp.azure.net/libs/amp/1.8.3/azuremediaplayer.min.js""></script>

     </head>
    <body>
        <video id=""azuremediaplayer"" class=""azuremediaplayer amp-default-skin amp-big-play-centered"" tabindex=""0""></video>
        <script>
var myOptions = {
	""nativeControlsForTouch"": false,
    controls: true,
	autoplay: true,
	width: ""640"",
	height: ""400"",
};
myPlayer = amp(""azuremediaplayer"", myOptions);
myPlayer.src([
    {
    src: ""{Binding Source}"",
    type: ""application/vnd.ms-sstr+xml""
    }
]);
        </script>
    </body>
</html>
";
            htmlSource.Html = sourceInfo.Replace("{Binding Source}", video.VideoUri);
            browser.Source = htmlSource;
		}
	}
}
