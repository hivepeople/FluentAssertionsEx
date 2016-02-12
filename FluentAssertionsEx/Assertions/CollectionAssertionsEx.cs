using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions.Execution;

namespace HivePeople.FluentAssertionsEx.Assertions
{
    public class CollectionWhichConstraint<T>
    {
        public IEnumerable<T> Subject { get; private set; }

        public CollectionWhichConstraint(IEnumerable<T> actualValue)
        {
            this.Subject = actualValue;
        }

        public IEnumerable<T> Which { get { return Subject; } }
    }

    internal static class CollectionAssertionsEx
    {
        internal static CollectionWhichConstraint<T> AssertAllElementsOfTypeAndCast<T>(this IEnumerable actualCollection, string because = "", params object[] reasonArgs)
        {
            if (ReferenceEquals(actualCollection, null))
            {
                string msg = "Expected {context:collection} to contains items assignable to type {0}{reason}, but found <null>.";

                Execute.Assertion
                    .BecauseOf(because, reasonArgs)
                    .FailWith(msg, typeof(T));
            }

            Type expectedType = typeof(T);
            int index = 0;

            foreach (object item in actualCollection)
            {
                if (!expectedType.IsAssignableFrom(item.GetType()))
                {
                    string msg = "Expected {context:collection} to contain only items of type {0}{reason}, but item {1} at index {2} was of type {3}.";

                    Execute.Assertion
                        .BecauseOf(because, reasonArgs)
                        .FailWith(msg, expectedType, item, index, item.GetType());
                }

                index++;
            }

            return new CollectionWhichConstraint<T>(actualCollection.Cast<T>());
        }
    }
}
