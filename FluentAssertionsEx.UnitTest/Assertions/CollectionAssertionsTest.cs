using System;
using System.Collections;
using System.Collections.Generic;
using FluentAssertions;
using HivePeople.FluentAssertionsEx.Extensions;
using NUnit.Framework;

namespace FluentAssertionsEx.UnitTest.Assertions
{
    [TestFixture]
    public class CollectionAssertionsTest
    {
        [Test]
        public void CanVerifyEnumerableOnlyContainsElementsOfType()
        {
            IEnumerable collection = new[] { "A", "B", "C" };
            collection.Should().OnlyContainElementsOfType<string>();
        }

        [Test]
        public void OnlyContainElementsOfTypeRejectsCollectionWithElementOfWrongType()
        {
            IEnumerable collection = new object[] { "A", 1, "B" };
            Action callingAssertion = () => collection.Should().OnlyContainElementsOfType<string>();
            callingAssertion.ShouldThrow<Exception>().WithMessage("*Expected*System.String*was*System.Int32*");
        }

        class Foo { };
        class Bar : Foo { };

        [Test]
        public void CanVerifyGenericEnumerableOnlyContainsElementsOfType()
        {
            IEnumerable<Foo> collection = new[] { new Bar(), new Bar() };
            collection.Should().OnlyContainElementsOfType<Foo, Bar>();
        }
    }
}
