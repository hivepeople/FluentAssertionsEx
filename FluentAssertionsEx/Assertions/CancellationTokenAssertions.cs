using System.Threading;
using FluentAssertions;
using FluentAssertions.Execution;

namespace HivePeople.FluentAssertionsEx.Assertions
{
    public class CancellationTokenAssertions
    {
        public CancellationToken Subject { get; private set; }

        internal CancellationTokenAssertions(CancellationToken actualCancellationToken)
        {
            this.Subject = actualCancellationToken;
        }

        public AndConstraint<CancellationTokenAssertions> BeEmpty(string because = "", params object[] reasonArgs)
        {
            Execute.Assertion
                .ForCondition(Subject.Equals(CancellationToken.None))
                .BecauseOf(because, reasonArgs)
                .FailWith("Expected {context:cancellation token} to be empty (equal default(CancellationToken){reason}.");

            return new AndConstraint<CancellationTokenAssertions>(this);
        }

        public AndConstraint<CancellationTokenAssertions> NotBeEmpty(string because = "", params object[] reasonArgs)
        {
            Execute.Assertion
                .ForCondition(!Subject.Equals(CancellationToken.None))
                .BecauseOf(because, reasonArgs)
                .FailWith("Did not expect {context:cancellation token} to be empty (equal default(CancellationToken){reason}.");

            return new AndConstraint<CancellationTokenAssertions>(this);
        }

        public AndConstraint<CancellationTokenAssertions> BeCancellable(string because = "", params object[] reasonArgs)
        {
            Execute.Assertion
                .ForCondition(Subject.CanBeCanceled)
                .BecauseOf(because, reasonArgs)
                .FailWith("Expected {context:cancellation token} to be cancellable{reason}.");

            return new AndConstraint<CancellationTokenAssertions>(this);
        }

        public AndConstraint<CancellationTokenAssertions> NotBeCancellable(string because = "", params object[] reasonArgs)
        {
            Execute.Assertion
                .ForCondition(!Subject.CanBeCanceled)
                .BecauseOf(because, reasonArgs)
                .FailWith("Did not expect {context:cancellation token} to be cancellable{reason}.");

            return new AndConstraint<CancellationTokenAssertions>(this);
        }

        public AndConstraint<CancellationTokenAssertions> BeCancelled(string because = "", params object[] reasonArgs)
        {
            Execute.Assertion
                .ForCondition(Subject.IsCancellationRequested)
                .BecauseOf(because, reasonArgs)
                .FailWith("Expected {context:cancellation token} to be cancelled{reason}.");

            return new AndConstraint<CancellationTokenAssertions>(this);
        }

        public AndConstraint<CancellationTokenAssertions> NotBeCancelled(string because = "", params object[] reasonArgs)
        {
            Execute.Assertion
                .ForCondition(!Subject.IsCancellationRequested)
                .BecauseOf(because, reasonArgs)
                .FailWith("Did not expect {context:cancellation token} to be cancelled{reason}.");

            return new AndConstraint<CancellationTokenAssertions>(this);
        }
    }

}
