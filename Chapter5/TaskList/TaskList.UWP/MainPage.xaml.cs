namespace TaskList.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            this.InitializeComponent();
            LoadApplication(new TaskList.App());
        }

        public MainPage(string picture)
        {
            this.InitializeComponent();
            LoadApplication(new TaskList.App(picture));
        }
    }
}
