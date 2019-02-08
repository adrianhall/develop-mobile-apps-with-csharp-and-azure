using Backend.Controllers;
using Backend.Tests.Utilities;
using System;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Web.Http;
using Xunit;

namespace Backend.Tests.Controllers
{
    public class TodoItemControllerTests
    {
        [Fact]
        public void UserId_With_Correct_Claims()
        {
            var controller = new TodoItemController();
            controller.User = new TestPrincipal(
                new Claim(ClaimTypes.NameIdentifier, "testuser"),
                new Claim("sub", "foo")
            );
            var result = controller.UserId;

            Assert.NotNull(result);
            Assert.Equal("testuser", result);
        }

        [Fact]
        public void UserId_With_Incomplete_Claims()
        {
            var controller = new TodoItemController();
            Thread.CurrentPrincipal = new TestPrincipal(
                new Claim("sub", "foo")
            );
            var result = controller.UserId;

            Assert.Null(result);
        }

        [Fact]
        public void UserId_With_Null_Claims()
        {
            var controller = new TodoItemController();
            controller.User = null;
            var ex = Assert.Throws<HttpResponseException>(() => { var result = controller.UserId; });
            Assert.Equal(HttpStatusCode.Unauthorized, ex.Response.StatusCode);
        }
    }
}
