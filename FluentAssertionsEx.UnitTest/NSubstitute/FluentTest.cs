using System;
using HivePeople.FluentAssertionsEx;
using NSubstitute;
using FluentAssertions;
using NUnit.Framework;
using NSubstitute.Exceptions;
using System.Threading.Tasks;
using NSubstitute.Core;
using HivePeople.FluentAssertionsEx.NSubstitute.Query;
using HivePeople.FluentAssertionsEx.Extensions;

namespace FluentAssertionsEx.UnitTest.NSubstitute
{
    [TestFixture]
    public class FluentTest
    {
        [Test]
        public void EnterQueryScopeRestoresOldContextOnDispose()
        {
            var scope = Fluent.EnterQueryScope();

            // Sanity check
            SubstitutionContext.Current.Should().BeOfType<FluentQuerySubstitutionContext>("because we have entered query scope");

            scope.Dispose();

            // Verify that the current context is no longer our fluent context
            // TODO: Cannot use the generic version of NotBeOfType yet due to https://github.com/dennisdoomen/FluentAssertions/issues/443
            SubstitutionContext.Current.Should().NotBeOfType(typeof(FluentQuerySubstitutionContext));
        }

        [Test]
        public void ReceivedInOrderAcceptsCallsMadeInOrder()
        {
            using (Fluent.EnterQueryScope())
            {
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
        }

        [Test]
        public void ReceivedInOrderRejectsCallsMadeOutOfOrder()
        {
            using (Fluent.EnterQueryScope())
            {
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
        }

        [Test]
        public void ReceivedInOrderAcceptsCallsMadeInOrderWithFluentSpec()
        {
            using (Fluent.EnterQueryScope())
            {
                var mock = Substitute.For<IComparable<string>>();

                mock.CompareTo("a");
                mock.CompareTo("b");

                Fluent.ReceivedInOrder(() =>
                {
                    mock.CompareTo(Fluent.Match<string>(s => s.Should().Be("a")));
                    mock.CompareTo(Fluent.Match<string>(s => s.Should().Be("b")));
                });
            }
        }

        [Test]
        public void ReceivedInOrderRejectsCallsMadeOutOfOrderWithFluentSpec()
        {
            using (Fluent.EnterQueryScope())
            {
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

        // NOTE: This interface is public so NSubstitute can easily create a proxy for it
        public interface IAsyncInterface
        {
            Task SomeMethodAsync(string arg);
            Task<int> AnotherMethodAsync();
        }

        [Test]
        public async Task ReceivedInOrderAcceptsAsyncCallsMadeInOrder()
        {
            using (Fluent.EnterQueryScope())
            {
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
        }

        [Test]
        public async Task ReceivedInOrderRejectsAsyncCallsMadeOutOfOrder()
        {
            using (Fluent.EnterQueryScope())
            {
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
        }

        [Test]
        public void ReceivedInAnyOrderAcceptsCallsMadeInOrder()
        {
            using (Fluent.EnterQueryScope())
            {
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
        }

        [Test]
        public void ReceivedInAnyOrderAcceptsCallsMadeOutOfOrder()
        {
            using (Fluent.EnterQueryScope())
            {
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
        }

        [Test]
        public void ReceivedInAnyOrderRejectsMissingCall()
        {
            using (Fluent.EnterQueryScope())
            {
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
        }

        [Test]
        public void ReceivedInAnyOrderAcceptsCallsWithFluentSpec()
        {
            using (Fluent.EnterQueryScope())
            {
                var mock = Substitute.For<IComparable<string>>();

                mock.CompareTo("b");
                mock.CompareTo("a");

                Fluent.ReceivedInAnyOrder(() =>
                {
                    mock.CompareTo(Fluent.Match<string>(s => s.Should().Be("a")));
                    mock.CompareTo(Fluent.Match<string>(s => s.Should().Be("b")));
                });
            }
        }

        [Test]
        public void ReceivedInAnyOrderRejectsMissingCallWithFluentSpec()
        {
            using (Fluent.EnterQueryScope())
            {
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

        [Test]
        public async Task ReceivedInAnyOrderAcceptsAsyncCallsMadeOutOfOrder()
        {
            using (Fluent.EnterQueryScope())
            {
                var mock = Substitute.For<IAsyncInterface>();
                mock.AnotherMethodAsync().Returns(Task.FromResult(2));

                await mock.SomeMethodAsync("djir");
                await mock.AnotherMethodAsync();
                await mock.SomeMethodAsync("yoyo");

                await Fluent.ReceivedInAnyOrder(async () =>
                {
                    await mock.SomeMethodAsync(Fluent.Match<string>(s => s.Should().Contain("yo")));
                    await mock.AnotherMethodAsync();
                    await mock.SomeMethodAsync(Fluent.Match<string>(s => s.Should().Contain("djir")));
                });
            }
        }

        [Test]
        public async Task ReceivedInAnyOrderRejectsMissingAsyncCall()
        {
            using (Fluent.EnterQueryScope())
            {
                var mock = Substitute.For<IAsyncInterface>();
                mock.AnotherMethodAsync().Returns(Task.FromResult(2));

                await mock.SomeMethodAsync("djir");
                await mock.AnotherMethodAsync();

                Func<Task> callingReceivedInAnyOrder = () => Fluent.ReceivedInAnyOrder(async () =>
                {
                    await mock.SomeMethodAsync(Fluent.Match<string>(s => s.Should().Contain("yo")));
                    await mock.AnotherMethodAsync();
                    await mock.SomeMethodAsync(Fluent.Match<string>(s => s.Should().Contain("djir")));
                });

                callingReceivedInAnyOrder.ShouldThrow<CallSequenceNotFoundException>();
            }
        }

        /// <summary>
        /// Should still support vanilla <see cref="Received.InOrder(Action)"/> queries.
        /// </summary>
        [Test]
        public void VanillaQueryingIsStillSupported()
        {
            using (Fluent.EnterQueryScope())
            {
                var mock = Substitute.For<IComparable>();
                object x = new Object(), y = new Object();

                mock.CompareTo(x);
                mock.CompareTo(y);

                Received.InOrder(() =>
                {
                    mock.CompareTo(x);
                    mock.CompareTo(y);
                });
            }
        }
    }
}
