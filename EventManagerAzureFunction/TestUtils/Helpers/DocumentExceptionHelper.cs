using Microsoft.Azure.Documents;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;

namespace TestUtils.Helpers
{
    public static class DocumentExceptionHelper
    {
        public static DocumentClientException Create(Error error, HttpStatusCode httpStatusCode)
        {
            var type = typeof(DocumentClientException);

            var documentClientExceptionInstance = type.Assembly.CreateInstance(type.FullName,
                false, BindingFlags.Instance | BindingFlags.NonPublic, null,
                new object[] { error, (HttpResponseHeaders)null, httpStatusCode }, null, null);

            return (DocumentClientException)documentClientExceptionInstance;
        }
    }
}
