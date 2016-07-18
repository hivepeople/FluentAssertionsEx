using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Collections;
using FluentAssertions.Primitives;
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

        /// <summary>
        /// Ascertains that the given collection only contains elements of type <typeparamref name="T"/> and returns a
        /// constraint that provides typed access to the collection.
        /// </summary>
        /// <typeparam name="T">Expected element type.</typeparam>
        /// <param name="assertions">Collection assertions to chain to.</param>
        /// <param name="because">Reason for this expectation.</param>
        /// <param name="reasonArgs">Arguments to the reason message.</param>
        /// <returns>Constraint for further chaining.</returns>
        public static CollectionWhichConstraint<T> OnlyContainElementsOfType<T>(this NonGenericCollectionAssertions assertions, string because = "", params object[] reasonArgs)
        {
            return CollectionAssertionsEx.AssertAllElementsOfTypeAndCast<T>(assertions.Subject, because, reasonArgs);
        }

        /// <summary>
        /// Ascertains that the given collection only contains elements of type <typeparamref name="TOut"/> and returns
        /// a constraint that provides typed access to the collection.
        /// </summary>
        /// <typeparam name="TIn">Type of elements in the incoming collection.</typeparam>
        /// <typeparam name="TOut">Expected element type.</typeparam>
        /// <param name="assertions">Collection assertions to chain to.</param>
        /// <param name="because">Reason for this expectation.</param>
        /// <param name="reasonArgs">Arguments to the reason message.</param>
        /// <returns>Constraint for further chaining.</returns>
        public static CollectionWhichConstraint<TOut> OnlyContainElementsOfType<TIn, TOut>(this GenericCollectionAssertions<TIn> assertions, string because = "", params object[] reasonArgs)
        {
            return CollectionAssertionsEx.AssertAllElementsOfTypeAndCast<TOut>(assertions.Subject, because, reasonArgs);
        }
    }
}
