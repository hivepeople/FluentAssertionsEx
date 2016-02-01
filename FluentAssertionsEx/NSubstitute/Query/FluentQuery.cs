using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using FluentAssertions;
using NSubstitute;
using NSubstitute.Core;
using NSubstitute.Core.SequenceChecking;
using NSubstitute.Exceptions;


namespace HivePeople.FluentAssertionsEx.NSubstitute.Query
{
    // NSubstitute.Core contains a duplicate definition of the Enumerable.Zip extension method, which would normally
    // cause compile errors, since the compiler cannot decide which one to use. But if we move one using declaration
    // to an inner namespace, we effectively prioritize extension methods from that using declarations namespace.
    // See: http://codeblog.jonskeet.uk/2010/11/03/using-extension-method-resolution-rules-to-decorate-awaiters/
    using System.Linq;

    public class FluentQuery
    {
        class ReferenceEqualityComparer : IEqualityComparer<object>
        {
            public new bool Equals(object x, object y)
            {
                return Object.ReferenceEquals(x, y);
            }

            public int GetHashCode(object obj)
            {
                return RuntimeHelpers.GetHashCode(obj);
            }
        }

        private readonly List<CallSpecAndTarget> querySpec = new List<CallSpecAndTarget>();

        public void Add(ICallSpecification callSpecification, object target)
        {
            querySpec.Add(new CallSpecAndTarget(callSpecification, target));
        }

        public void VerifyExactCallOrder()
        {
            var callsInOrder = querySpec
                .Select(csat => csat.Target)
                .Distinct(new ReferenceEqualityComparer())
                .SelectMany(tgt => tgt.ReceivedCalls())
                .OrderBy(call => call.GetSequenceNumber());
            
            var callOrderChecks = callsInOrder.Zip(querySpec, (call, spec) =>
            {
                return ReferenceEquals(call.Target(), spec.Target) && spec.CallSpecification.IsSatisfiedBy(call);
            });

            try
            {
                if (callOrderChecks.Any(match => !match))
                    throw new CallSequenceNotFoundException(GetCallOrderExceptionMessage(querySpec, callsInOrder, ""));
            }
            catch (CallSequenceNotFoundException)
            {
                // Not the exception we are looking for, rethrow
                throw;
            }
            catch (Exception e)
            {
                // Only exceptions thrown by FA should propagate to this point
                e.Should().BeOfType(Fluent.FluentExceptionType.Value);

                throw new CallSequenceNotFoundException(GetCallOrderExceptionMessage(querySpec, callsInOrder, GetFluentErrorMessage(e)));
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
}
