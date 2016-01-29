using System.Threading;
using System.Threading.Tasks;
using HivePeople.FluentAssertionsEx.Assertions;

namespace HivePeople.FluentAssertionsEx.Extensions
{
    public static class FluentAssertionsExtensions
    {
        public static CancellationTokenAssertions Should(this CancellationToken actualValue)
        {
            return new CancellationTokenAssertions(actualValue);
        }

        public static TaskAssertions Should(this Task actualValue)
        {
            return new TaskAssertions(actualValue);
        }
    }
}
