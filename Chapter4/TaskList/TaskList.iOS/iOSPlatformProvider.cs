using TaskList.Abstractions;

[assembly: Xamarin.Forms.Dependency(typeof(TaskList.iOS.iOSPlatformProvider))]
namespace TaskList.iOS
{
    public class iOSPlatformProvider : IPlatformProvider
    {
    }
}
