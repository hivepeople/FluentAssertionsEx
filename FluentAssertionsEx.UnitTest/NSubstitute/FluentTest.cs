﻿using System;
using HivePeople.FluentAssertionsEx;
using NSubstitute;
using FluentAssertions;
using NUnit.Framework;
using NSubstitute.Exceptions;
using System.Threading.Tasks;

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

        // NOTE: This interface is public so NSubstitute can easily create a proxy for it
        public interface IAsyncInterface
        {
            Task SomeMethodAsync(string arg);
            Task<int> AnotherMethodAsync();
        }

        [Test]
        public async Task ReceivedInOrderAcceptsAsyncCallsMadeInOrder()
        {
            Fluent.Init();

            var mock = Substitute.For<IAsyncInterface>();
            mock.AnotherMethodAsync().Returns(Task.FromResult(2));

            await mock.SomeMethodAsync("yoyo");
            await mock.AnotherMethodAsync();
            await mock.SomeMethodAsync("djir");

            await Fluent.ReceivedInOrder(async () =>
            {
                await mock.SomeMethodAsync(Fluent.Match<string>(s => s.Should().Contain("yo")));
                await mock.AnotherMethodAsync();
                await mock.SomeMethodAsync(Fluent.Match<string>(s => s.Should().Contain("djir")));
            });
        }

        [Test]
        public async Task ReceivedInOrderRejectsAsyncCallsMadeOutOfOrder()
        {
            Fluent.Init();

            var mock = Substitute.For<IAsyncInterface>();
            mock.AnotherMethodAsync().Returns(Task.FromResult(2));

            await mock.SomeMethodAsync("djir");
            await mock.AnotherMethodAsync();
            await mock.SomeMethodAsync("yoyo");

            Func<Task> callingReceivedInOrder = () => Fluent.ReceivedInOrder(async () =>
            {
                await mock.SomeMethodAsync(Fluent.Match<string>(s => s.Should().Contain("yo")));
                await mock.AnotherMethodAsync();
                await mock.SomeMethodAsync(Fluent.Match<string>(s => s.Should().Contain("djir")));
            });

            callingReceivedInOrder.ShouldThrow<CallSequenceNotFoundException>();
        }

        [Test]
        public void ReceivedInAnyOrderAcceptsCallsMadeInOrder()
        {
            Fluent.Init();

            var mock = Substitute.For<IComparable>();
            var otherObj = new Object();
            var yetAnotherObj = new Object();

            mock.CompareTo(otherObj);
            mock.CompareTo(yetAnotherObj);

            Fluent.ReceivedInAnyOrder(() =>
            {
                mock.CompareTo(otherObj);
                mock.CompareTo(yetAnotherObj);
            });
        }

        [Test]
        public void ReceivedInAnyOrderAcceptsCallsMadeOutOfOrder()
        {
            Fluent.Init();

            var mock = Substitute.For<IComparable>();
            var otherObj = new Object();
            var yetAnotherObj = new Object();

            mock.CompareTo(yetAnotherObj);
            mock.CompareTo(otherObj);

            Fluent.ReceivedInAnyOrder(() =>
            {
                mock.CompareTo(otherObj);
                mock.CompareTo(yetAnotherObj);
            });
        }

        [Test]
        public void ReceivedInAnyOrderRejectsMissingCall()
        {
            Fluent.Init();

            var mock = Substitute.For<IComparable>();
            var otherObj = new Object();
            var yetAnotherObj = new Object();

            mock.CompareTo(otherObj);

            Action callingReceivedInAnyOrder = () => Fluent.ReceivedInAnyOrder(() =>
            {
                mock.CompareTo(otherObj);
                mock.CompareTo(yetAnotherObj);
            });

            callingReceivedInAnyOrder.ShouldThrow<CallSequenceNotFoundException>();
        }

        [Test]
        public void ReceivedInAnyOrderAcceptsCallsWithFluentSpec()
        {
            Fluent.Init();

            var mock = Substitute.For<IComparable<string>>();

            mock.CompareTo("b");
            mock.CompareTo("a");

            Fluent.ReceivedInAnyOrder(() =>
            {
                mock.CompareTo(Fluent.Match<string>(s => s.Should().Be("a")));
                mock.CompareTo(Fluent.Match<string>(s => s.Should().Be("b")));
            });
        }

        [Test]
        public void ReceivedInAnyOrderRejectsMissingCallWithFluentSpec()
        {
            Fluent.Init();

            var mock = Substitute.For<IComparable<string>>();

            mock.CompareTo("b");

            Action callingReceivedInAnyOrder = () => Fluent.ReceivedInAnyOrder(() =>
            {
                mock.CompareTo(Fluent.Match<string>(s => s.Should().Be("a")));
                mock.CompareTo(Fluent.Match<string>(s => s.Should().Be("b")));
            });

            callingReceivedInAnyOrder.ShouldThrow<CallSequenceNotFoundException>();
        }
    }
}
