using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute.Core;
using NSubstitute.Core.Arguments;
using NSubstitute.Exceptions;
using NSubstitute.Proxies;
using NSubstitute.Proxies.CastleDynamicProxy;
using NSubstitute.Proxies.DelegateProxy;
using NSubstitute.Routing;
using NSubstitute.Routing.Handlers;

namespace HivePeople.FluentAssertionsEx.NSubstitute.Query
{
    public class FluentQuerySubstitutionContext : ISubstitutionContext
    {
        private class FluentQueryCompatibleRouteFactory : RouteFactory, IRouteFactory
        {
            IRoute IRouteFactory.CallQuery(ISubstituteState state)
            {
                return new Route(new ICallHandler[]
                {
                    new ClearUnusedCallSpecHandler(state),
                    new AddCallToQueryResultHandler(state.SubstitutionContext, state.CallSpecificationFactory),
                    // new ReturnConfiguredResultHandler(state.CallResults),  <- handler incompatible with FluentArgumentSpecification
                    new ReturnAutoValue(AutoValueBehaviour.UseValueForSubsequentCalls, state.AutoValueProviders, state.ConfigureCall),
                    new ReturnDefaultForReturnTypeHandler(new DefaultForType())
                });
            }
        }

        private class FluentCallRouterFactory : ICallRouterFactory
        {
            public ICallRouter Create(ISubstitutionContext substitutionContext, SubstituteConfig config)
            {
                var state = new SubstituteState(substitutionContext, config);
                return new CallRouter(state, substitutionContext, new FluentQueryCompatibleRouteFactory());
            }
        }

        private static readonly AsyncLocal<FluentQuery> query = new AsyncLocal<FluentQuery>();

        private readonly ISubstitutionContext innerContext;
        private readonly ISubstituteFactory substituteFactory;

        public FluentQuerySubstitutionContext(ISubstitutionContext innerContext)
        {
            this.innerContext = innerContext;

            var callRouterFactory = new FluentCallRouterFactory();
            var dynamicProxyFactory = new CastleDynamicProxyFactory();
            var delegateFactory = new DelegateProxyFactory();
            var proxyFactory = new ProxyFactory(delegateFactory, dynamicProxyFactory);
            var callRouteResolver = new CallRouterResolver();

            this.substituteFactory = new SubstituteFactory(this, callRouterFactory, proxyFactory, callRouteResolver);
        }

        // CODESTD: This class groups members by implementation strategy (delegating vs specializing)

        #region Delegated members
        public SequenceNumberGenerator SequenceNumberGenerator { get { return innerContext.SequenceNumberGenerator; } }

        public void ClearLastCallRouter()
        {
            innerContext.ClearLastCallRouter();
        }

        public IList<IArgumentSpecification> DequeueAllArgumentSpecifications()
        {
            return innerContext.DequeueAllArgumentSpecifications();
        }

        public void EnqueueArgumentSpecification(IArgumentSpecification spec)
        {
            innerContext.EnqueueArgumentSpecification(spec);
        }

        public ICallRouter GetCallRouterFor(object substitute)
        {
            return innerContext.GetCallRouterFor(substitute);
        }

        public IRouteFactory GetRouteFactory()
        {
            return innerContext.GetRouteFactory();
        }

        public void LastCallRouter(ICallRouter callRouter)
        {
            innerContext.LastCallRouter(callRouter);
        }

        public ConfiguredCall LastCallShouldReturn(IReturn value, MatchArgs matchArgs)
        {
            return innerContext.LastCallShouldReturn(value, matchArgs);
        }

        public void RaiseEventForNextCall(Func<ICall, object[]> getArguments)
        {
            innerContext.RaiseEventForNextCall(getArguments);
        }

        public IQueryResults RunQuery(Action calls)
        {
            return innerContext.RunQuery(calls);
        }
        #endregion Delegated members

        #region Specialized members
        public bool IsQuerying { get { return query.Value != null || innerContext.IsQuerying; } }

        public ISubstituteFactory SubstituteFactory { get { return substituteFactory; } }

        public void AddToQuery(object target, ICallSpecification callSpecification)
        {
            if (!IsQuerying)
                throw new NotRunningAQueryException();

            if (query.Value == null)
            {
                // Must be a legacy query, delegate
                innerContext.AddToQuery(target, callSpecification);
            }
            else
            {
                query.Value.Add(callSpecification, target);
            }
        }
        #endregion Specialized members

        #region New members
        public FluentQuery RunFluentQuery(Action calls)
        {
            return RunFluentQueryAsync(() =>
            {
                calls();
                return Task.CompletedTask;
            }).Result;
        }

        public async Task<FluentQuery> RunFluentQueryAsync(Func<Task> calls)
        {
            if (IsQuerying)
                throw new InvalidOperationException("Cannot run nested queries");

            var activeQuery = new FluentQuery();
            query.Value = activeQuery;
            try
            {
                await calls();
            }
            finally
            {
                query.Value = null;
            }

            return activeQuery;
        }
        #endregion New members
    }
}
