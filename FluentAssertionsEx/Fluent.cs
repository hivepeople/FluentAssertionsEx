using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions.Common;
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

        public static void Init()
        {
            var activeContext = SubstitutionContext.Current;

            if (activeContext is FluentQuerySubstitutionContext)
                return;  // Nothing to do

            // TODO: This is not thread-safe since SubstitutionContext.Current is shared global state
            // See: https://github.com/nsubstitute/NSubstitute/issues/220
            // Until resolved, tests that use Fluent.Init are not parallelizable with other tests using NSubstitute
            SubstitutionContext.Current = new FluentQuerySubstitutionContext(activeContext);
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
    }
}
