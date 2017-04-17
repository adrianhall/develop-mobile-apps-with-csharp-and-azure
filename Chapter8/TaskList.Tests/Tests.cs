using NUnit.Framework;
using System.Diagnostics;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace TaskList.Tests
{
    [TestFixture(Platform.Android)]
    //[TestFixture(Platform.iOS)]
    public class Tests
    {
        private IApp app;
        private readonly Platform platform;

        public Tests(Platform platform)
        {
            this.platform = platform;
        }

        [SetUp]
        public void BeforeEachTest()
        {
            app = AppInitializer.StartApp(platform);
        }

        [Test]
        public void AppLaunches()
        {
            app.Screenshot("First screen.");
        }

        [Test]
        public void NewTest()
        {
            app.Tap(x => x.Text("Login"));
            app.Screenshot("Logged in - initial list of items");
            app.Tap(x => x.Text("Add New Item"));
            app.Screenshot("Empty detail record");

            AppResult[] results = app.Query("entrytext");
            Assert.AreEqual(1, results.Length);
            Assert.AreEqual("New Item", results[0].Text);

            app.Tap(x => x.Text("Save"));
            app.Screenshot("Back at list of items");
        }

        [Test]
        public void Repl()
        {
            app.Tap(x => x.Text("Login"));
            app.Tap(x => x.Text("Add New Item"));
            app.Repl();
        }
    }
}

