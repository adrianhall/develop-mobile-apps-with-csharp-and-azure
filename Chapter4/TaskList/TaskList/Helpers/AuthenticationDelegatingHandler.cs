using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TaskList.Abstractions;

namespace TaskList.Helpers
{
    public class AuthenticationDelegatingHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var clone = await CloneHttpRequestMessageAsync(request);
            var response = await base.SendAsync(request, cancellationToken);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var user = await ServiceLocator.Get<ICloudService>().LoginAsync();
                clone.Headers.Remove("X-ZUMO-AUTH");
                clone.Headers.Add("X-ZUMO-AUTH", user.MobileServiceAuthenticationToken);
                response = await base.SendAsync(clone, cancellationToken);
            }

            return response;
        }

        /// <summary>
        /// Clone the incoming HttpRequestMessage without affecting it.
        /// </summary>
        /// <param name="request">The request message</param>
        /// <returns>A copy of the request message</returns>
        private async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri);

            var ms = new MemoryStream();
            if (request.Content != null)
            {
                await request.Content.CopyToAsync(ms).ConfigureAwait(false);
                ms.Position = 0;
                clone.Content = new StreamContent(ms);

                if (request.Content.Headers != null)
                {
                    foreach (var contentHeader in request.Content.Headers)
                    {
                        clone.Content.Headers.Add(contentHeader.Key, contentHeader.Value);
                    }
                }
            }

            clone.Version = request.Version;
            foreach (KeyValuePair<string, object> property in request.Properties)
            {
                clone.Properties.Add(property);
            }

            foreach(KeyValuePair<string, IEnumerable<string>> header in request.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return clone;
        }
    }
}
