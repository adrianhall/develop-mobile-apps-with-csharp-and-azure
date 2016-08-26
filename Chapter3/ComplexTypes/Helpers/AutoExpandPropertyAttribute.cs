using System;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace ComplexTypes.Helpers
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class AutoExpandPropertyAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// The expand parameter we want to modify
        /// </summary>
        const string expandParam = "$expand";

        /// <summary>
        /// The comparison predicate for the expand parameter
        /// </summary>
        Predicate<string> paramComparer = new Predicate<string>(
            (value) => value.StartsWith($"{expandParam}=", StringComparison.Ordinal)
        );

        /// <summary>
        /// The property name we are modifying
        /// </summary>
        string propertyName;

        /// <summary>
        /// Create a new AutoExpandProperty attribute with a specific property name
        /// </summary>
        /// <param name="propertyName"></param>
        public AutoExpandPropertyAttribute(string propertyName)
        {
            this.propertyName = propertyName;
        }

        /// <summary>
        /// Runs before a HTTP request processor to adjust the query parameters
        /// </summary>
        /// <param name="actionContext">The context</param>
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            base.OnActionExecuting(actionContext);

            // Split out the query params into their individual query params using normal HTTP logic
            var uriBuilder = new UriBuilder(actionContext.Request.RequestUri);
            var queryParams = uriBuilder.Query
                .TrimStart('?')
                .Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            // If we already have an expand property, append to it - otherwise create it
            var expandIndex = queryParams.FindIndex(paramComparer);
            if (expandIndex < 0)
            {
                queryParams.Add($"{expandParam}={propertyName}");
            }
            else
            {
                queryParams[expandIndex] = queryParams[expandIndex] + "," + propertyName;
            }

            // Rebuild the query string and the URI
            uriBuilder.Query = string.Join("&", queryParams);
            actionContext.Request.RequestUri = uriBuilder.Uri;
        }
    }
}