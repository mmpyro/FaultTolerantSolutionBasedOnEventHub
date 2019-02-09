using Common.Classifier;
using Microsoft.Azure.Documents;
using Microsoft.WindowsAzure.Storage;
using Polly;
using System;

namespace Common.Policy
{
    public class PolicyRegistry : IPolicyRegistry
    {
        public IAsyncPolicy[] CreateAsyncPolicies()
        {
            return new IAsyncPolicy[]
            {
                Polly.Policy.Handle<DocumentClientException>(ex => ExceptionClassifier.IsTimeout(ex))
                    .WaitAndRetryAsync(5, (retry) => TimeSpan.FromSeconds(1*retry)),
                Polly.Policy.Handle<DocumentClientException>(ex => ExceptionClassifier.IsTooManyRequests(ex))
                    .WaitAndRetryAsync(new []{ TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2)}),
                Polly.Policy.Handle<DocumentClientException>(ex => ExceptionClassifier.IsServiceUnavaiable(ex))
                    .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(2)),
                Polly.Policy.Handle<DocumentClientException>(ex => ExceptionClassifier.IsInternalServerError(ex))
                    .RetryAsync(1),
                Polly.Policy.Handle<StorageException>()
                    .RetryAsync(1)
            };
        }

        public Polly.Policy[] CreatePolicies()
        {
            return new Polly.Policy[]
            {
                Polly.Policy.Handle<DocumentClientException>(ex => ExceptionClassifier.IsTimeout(ex))
                    .WaitAndRetry(5, (retry) => TimeSpan.FromSeconds(1*retry)),
                Polly.Policy.Handle<DocumentClientException>(ex => ExceptionClassifier.IsTooManyRequests(ex))
                    .WaitAndRetry(new []{ TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2)}),
                Polly.Policy.Handle<DocumentClientException>(ex => ExceptionClassifier.IsServiceUnavaiable(ex))
                    .WaitAndRetryForever(_ => TimeSpan.FromSeconds(2)),
                Polly.Policy.Handle<DocumentClientException>(ex => ExceptionClassifier.IsInternalServerError(ex))
                    .Retry(1),
                Polly.Policy.Handle<StorageException>()
                    .Retry(1)
            };
        }
    }
}
