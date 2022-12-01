using System;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;

namespace ParcelRegistry.Tests.BackOffice.Lambda
{
    internal class FakeRetryPolicy : ICustomRetryPolicy
    {
        public Task Retry(Func<Task> functionToRetry)
        {
            return functionToRetry();
        }
    }
}
