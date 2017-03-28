using Backend.Controllers;
using Xunit;

namespace Backend.Tests.Controllers
{
    public class ValuesControllerTests
    {
        [Fact]
        public void Get_Name_Works()
        {
            var controller = new ValuesController();
            var result = controller.Get("adrian");
            Assert.Equal("Hello adrian!", result);
        }
    }
}

