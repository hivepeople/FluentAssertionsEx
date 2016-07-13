using System;
using System.Threading.Tasks;
using FluentAssertions.Common;
using FluentAssertions.Execution;
using FluentAssertionsEx.Support;
using HivePeople.FluentAssertionsEx.NSubstitute.Query;
using NSubstitute.Core;
using NSubstitute.Core.Arguments;

namespace HivePeople.FluentAssertionsEx
{
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
                bool success = false;

                // This is stupid: because AssertionScope provides no ability to peek inside and because we need to
                // preserve any failures if we are querying, we must perform the assertion twice

                // First we determine if the assertion succeeds without triggering exceptions
                using (var assertScope = new AssertionScope())
                {
                    assertion((T)argument);
                    var failures = assertScope.Discard();
                    success = failures.Length == 0;
                }

                // Then we rerun it to either throw an exception or report error to an enclosing scope
                assertion((T)argument);

                return success;
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
                    if (FluentExceptionType.Value.IsInstanceOfType(e))
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


        /// <summary>The type of exception thrown by FA on this platform. Necessary because it varies with platform.</summary>
        internal static Lazy<Type> FluentExceptionType = new Lazy<Type>(() =>
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

        public static IDisposable EnterQueryScope()
        {
            // TODO: This is not thread-safe since SubstitutionContext.Current is shared global state
            // See: https://github.com/nsubstitute/NSubstitute/issues/220
            // Until resolved, tests that use this method are not parallelizable with other tests using NSubstitute

            var oldContext = SubstitutionContext.Current;

            if (oldContext is FluentQuerySubstitutionContext)
            {
                // Already in query scope, leave current context alone
                return new ActionDisposable(() => { });  // Do nothing on dispose
            }
            else
            {
                SubstitutionContext.Current = new FluentQuerySubstitutionContext(oldContext);
                return new ActionDisposable(() => SubstitutionContext.Current = oldContext);  // Restore old context on dispose
            }
        }

        public static T Match<T>(Action<T> action)
        {
            var matcher = new FluentAssertionMatcher<T>(action);
            var innerArgumentSpecification = new ArgumentSpecification(typeof(T), matcher);
            var fluentArgumentSpecification = new FluentArgumentSpecification(innerArgumentSpecification, matcher);

            SubstitutionContext.Current.EnqueueArgumentSpecification(fluentArgumentSpecification);

            return default(T);
        }

        public static void ReceivedInOrder(Action calls)
        {
            var fluentContext = SubstitutionContext.Current as FluentQuerySubstitutionContext;
            if (fluentContext == null)
                throw new InvalidOperationException("Must call Fluent.Init() before creating mocks when using Fluent.ReceivedInOrder");

            var query = fluentContext.RunFluentQuery(calls);
            query.VerifyExactCallOrder();
        }

        public static async Task ReceivedInOrder(Func<Task> asyncCalls)
        {
            var fluentContext = SubstitutionContext.Current as FluentQuerySubstitutionContext;
            if (fluentContext == null)
                throw new InvalidOperationException("Must call Fluent.Init() before creating mocks when using Fluent.ReceivedInOrder");

            var query = await fluentContext.RunFluentQueryAsync(asyncCalls);
            query.VerifyExactCallOrder();
        }

        public static void ReceivedInAnyOrder(Action calls)
        {
            var fluentContext = SubstitutionContext.Current as FluentQuerySubstitutionContext;
            if (fluentContext == null)
                throw new InvalidOperationException("Must call Fluent.Init() before creating mocks when using Fluent.ReceivedInAnyOrder");

            var query = fluentContext.RunFluentQuery(calls);
            query.VerifyAnyCallOrder();
        }

        public static async Task ReceivedInAnyOrder(Func<Task> asyncCalls)
        {
            var fluentContext = SubstitutionContext.Current as FluentQuerySubstitutionContext;
            if (fluentContext == null)
                throw new InvalidOperationException("Must call Fluent.Init() before creating mocks when using Fluent.ReceivedInAnyOrder");

            var query = await fluentContext.RunFluentQueryAsync(asyncCalls);
            query.VerifyAnyCallOrder();
        }
    }
}
