using System;
using HivePeople.FluentAssertionsEx;
using NSubstitute;
using FluentAssertions;
using NUnit.Framework;
using NSubstitute.Exceptions;

namespace FluentAssertionsEx.UnitTest.NSubstitute
{
    [TestFixture]
    public class FluentTest
    {
        [Test]
        public void ReceivedInOrderAcceptsCallsMadeInOrder()
        {
            Fluent.Init();

            var mock = Substitute.For<IComparable>();
            var otherObj = new Object();
            var yetAnotherObj = new Object();

            mock.CompareTo(otherObj);
            mock.CompareTo(yetAnotherObj);

            Fluent.ReceivedInOrder(() =>
            {
                mock.CompareTo(otherObj);
                mock.CompareTo(yetAnotherObj);
            });
        }

        [Test]
        public void ReceivedInOrderRejectsCallsMadeOutOfOrder()
        {
            Fluent.Init();

            var mock = Substitute.For<IComparable>();
            var otherObj = new Object();
            var yetAnotherObj = new Object();

            mock.CompareTo(yetAnotherObj);
            mock.CompareTo(otherObj);

            Action outOfOrder = () => Fluent.ReceivedInOrder(() =>
            {
                mock.CompareTo(otherObj);
                mock.CompareTo(yetAnotherObj);
            });

            outOfOrder.ShouldThrow<CallSequenceNotFoundException>();
        }

        [Test]
        public void ReceivedInOrderAcceptsCallsMadeInOrderWithFluentSpec()
        {
            Fluent.Init();

            var mock = Substitute.For<IComparable<string>>();

            mock.CompareTo("a");
            mock.CompareTo("b");

            Fluent.ReceivedInOrder(() =>
            {
                mock.CompareTo(Fluent.Match<string>(s => s.Should().Be("a")));
                mock.CompareTo(Fluent.Match<string>(s => s.Should().Be("b")));
            });
        }

        [Test]
        public void ReceivedInOrderRejectsCallsMadeOutOfOrderWithFluentSpec()
        {
            Fluent.Init();

            var mock = Substitute.For<IComparable<string>>();

            mock.CompareTo("b");
            mock.CompareTo("a");

            Action callingReceivedInOrder = () => Fluent.ReceivedInOrder(() =>
            {
                mock.CompareTo(Fluent.Match<string>(s => s.Should().Be("a")));
                mock.CompareTo(Fluent.Match<string>(s => s.Should().Be("b")));
            });

            callingReceivedInOrder.ShouldThrow<CallSequenceNotFoundException>();
        }
    }
}
