using System;
using System.Collections.Generic;
using FluentAssertions;
using FluentAssertions.Common;
using NSubstitute;
using NSubstitute.Core;
using NSubstitute.Core.Arguments;
using NSubstitute.Core.SequenceChecking;
using NSubstitute.Exceptions;

namespace HivePeople.FluentAssertionsEx
{
    // NSubstitute.Core contains a duplicate definition of the Enumerable.Zip extension method, which would normally
    // cause compile errors, since the compiler cannot decide which one to use. But if we move one using declaration
    // to an inner namespace, we effectively prioritize extension methods from that using declarations namespace.
    // See: http://codeblog.jonskeet.uk/2010/11/03/using-extension-method-resolution-rules-to-decorate-awaiters/
    using System.Linq;

    /// <summary>
    /// Bridges the gap between NSubstitute and Fluent Assertions. Inspired by a blog post by Rory Primrose, but this
    /// version circumvents the NSubstitute exception catching machinery to directly throw exceptions from FA, so you
    /// don't have to check trace output to see the error messages.
    /// </summary>
    /// <see cref="http://www.neovolve.com/2014/10/07/bridging-between-nsubstitute-and-fluentassertions/"/>
    public static class Fluent
    {
        private class FluentAssertionMatcher<T> : IArgumentMatcher
        {
            private readonly Action<T> assertion;

            public FluentAssertionMatcher(Action<T> assertion)
            {
                this.assertion = assertion;
            }

            public bool IsSatisfiedBy(object argument)
            {
                assertion((T)argument);
                return true;
            }
        }

        private class FluentArgumentSpecification : IArgumentSpecification
        {
            private readonly IArgumentSpecification innerSpecification;
            private readonly IArgumentMatcher matcher;

            public FluentArgumentSpecification(IArgumentSpecification innerSpecification, IArgumentMatcher matcher)
            {
                this.innerSpecification = innerSpecification;
                this.matcher = matcher;
            }


            public IArgumentSpecification CreateCopyMatchingAnyArgOfType(Type requiredType)
            {
                return new FluentArgumentSpecification(innerSpecification.CreateCopyMatchingAnyArgOfType(requiredType),
                    new AnyArgumentMatcher(requiredType));
            }

            public string DescribeNonMatch(object argument)
            {
                return innerSpecification.DescribeNonMatch(argument);
            }

            public Type ForType
            {
                get { return innerSpecification.ForType; }
            }

            public string FormatArgument(object argument)
            {
                return innerSpecification.FormatArgument(argument);
            }

            public bool IsSatisfiedBy(object argument)
            {
                if (!argument.IsCompatibleWith(ForType))
                    return false;

                try
                {
                    return matcher.IsSatisfiedBy(argument);
                }
                catch (Exception e)
                {
                    if (fluentExceptionType.Value.IsInstanceOfType(e))
                    {
                        // We explicitly let exceptions from Fluent Assertions escape
                        throw e;
                    }
                    else
                    {
                        // Follow normal NSubstitute behaviour where exception from matcher => non-match
                        return false;
                    }
                }
            }

            public void RunAction(object argument)
            {
                innerSpecification.RunAction(argument);
            }
        }

        // TODO: Figure out how to hook this up to NSubstitute, to enable call order verification with fluent assertions
        private class FluentQuery
        {
            private readonly List<CallSpecAndTarget> querySpec = new List<CallSpecAndTarget>();

            public void Add(ICallSpecification callSpecification, object target)
            {
                querySpec.Add(new CallSpecAndTarget(callSpecification, target));
            }

            public void VerifyExactCallOrder()
            {
                List<ICall> calls = new List<ICall>();
                foreach (var csat in querySpec)
                    calls.AddRange(csat.Target.ReceivedCalls());

                var callOrderChecks = calls.OrderBy(call => call.GetSequenceNumber()).Zip(querySpec.AsEnumerable(), (call, spec) =>
                {
                    return ReferenceEquals(call.Target(), spec.Target) && spec.CallSpecification.IsSatisfiedBy(call);
                });

                try
                {
                    if (callOrderChecks.Any(match => !match))
                        throw new CallSequenceNotFoundException(GetCallOrderExceptionMessage(querySpec, calls, ""));
                }
                catch (Exception e)
                {
                    // Only exceptions thrown by FA should propagate to this point
                    e.Should().BeOfType(fluentExceptionType.Value);

                    throw new CallSequenceNotFoundException(GetCallOrderExceptionMessage(querySpec, calls, GetFluentErrorMessage(e)));
                }
            }

            /// <summary>This method is heavily inspired by NSubstitute.Core.SequenceChecking.SequenceInOrderAssertion.GetExceptionMessage</summary>
            /// <see cref="https://github.com/nsubstitute/NSubstitute/blob/master/Source/NSubstitute/Core/SequenceChecking/SequenceInOrderAssertion.cs#L62"/>
            private string GetCallOrderExceptionMessage(IEnumerable<CallSpecAndTarget> querySpec, IEnumerable<ICall> callsInOrder, string fluentError)
            {
                const string callDelimiter = "\n    ";
                var formatter = new SequenceFormatter(callDelimiter, querySpec.ToArray(), callsInOrder.ToArray());

                return String.Format("\nExpected to receive these calls in order:\n{0}{1}\n" +
                    "\nActually received calls to target instances in this order:\n{0}{2}\n\n{3}{4}",
                    callDelimiter, formatter.FormatQuery(), formatter.FormatActualCalls(),
                    "*** Note: calls to property getters are not considered part of the query. ***",
                    fluentError);
            }

            private string GetFluentErrorMessage(Exception exception)
            {
                return "\n\nFluent Assertions said:\n" + exception.Message;
            }
        }

        // NOTE: This is only to emulate behaviour of ArgumentSpecificationQueue, may not be needed
        private static readonly ISubstitutionContext substitutionContextAtStart = SubstitutionContext.Current;

        /// <summary>The type of exception thrown by FA on this platform. Necessary because it varies with platform.</summary>
        private static Lazy<Type> fluentExceptionType = new Lazy<Type>(() =>
        {
            try
            {
                Services.ThrowException("Ignore this exception");
            }
            catch (Exception e)
            {
                return e.GetType();
            }

            // Surely we cannot get here? ;-)
            throw new Exception("Services.ThrowException did not throw an exception!");
        });

        public static T Match<T>(Action<T> action)
        {
            var matcher = new FluentAssertionMatcher<T>(action);
            var innerArgumentSpecification = new ArgumentSpecification(typeof(T), matcher);
            var fluentArgumentSpecification = new FluentArgumentSpecification(innerArgumentSpecification, matcher);

            substitutionContextAtStart.EnqueueArgumentSpecification(fluentArgumentSpecification);

            return default(T);
        }
    }
}
