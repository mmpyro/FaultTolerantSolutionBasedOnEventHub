using Microsoft.Azure.Documents;
using System.Net;

namespace Common.Classifier
{
    public static class ExceptionClassifier
    {
        public static bool IsTimeout(DocumentClientException ex)
        {
            return ex.StatusCode == HttpStatusCode.RequestTimeout;
        }

        public static bool IsTooManyRequests(DocumentClientException ex)
        {
            return ex.StatusCode == HttpStatusCode.TooManyRequests;
        }

        public static bool IsServiceUnavaiable(DocumentClientException ex)
        {
            return ex.StatusCode == HttpStatusCode.ServiceUnavailable;
        }

        public static bool IsInternalServerError(DocumentClientException ex)
        {
            return ex.StatusCode == HttpStatusCode.InternalServerError;
        }
    }
}
