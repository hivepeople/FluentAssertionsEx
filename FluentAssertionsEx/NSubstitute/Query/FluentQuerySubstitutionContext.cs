using System;
using System.Collections.Generic;
using System.Threading;
using NSubstitute.Core;
using NSubstitute.Core.Arguments;
using NSubstitute.Exceptions;
using NSubstitute.Proxies;
using NSubstitute.Proxies.CastleDynamicProxy;
using NSubstitute.Proxies.DelegateProxy;
using NSubstitute.Routing;

namespace HivePeople.FluentAssertionsEx.NSubstitute.Query
{
    public class FluentQuerySubstitutionContext : ISubstitutionContext
    {
        private readonly ISubstitutionContext innerContext;
        private readonly ISubstituteFactory substituteFactory;
        private readonly AsyncLocal<FluentQuery> query = new AsyncLocal<FluentQuery>();

        public FluentQuerySubstitutionContext(ISubstitutionContext innerContext)
        {
            this.innerContext = innerContext;

            var callRouterFactory = new CallRouterFactory();
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
        #endregion Delegated members

        #region Specialized members
        public bool IsQuerying { get { return query.Value != null; } }

        public ISubstituteFactory SubstituteFactory { get { return substituteFactory; } }

        public IQueryResults RunQuery(Action calls)
        {
            // Cannot retrofit RunQuery since IQueryResults interface doesn't make sense for FluentQuery
            throw new NotImplementedException();
        }

        public FluentQuery RunFluentQuery(Action calls)
        {
            if (IsQuerying)
                throw new InvalidOperationException("Cannot run nested queries");

            var activeQuery = new FluentQuery();
            query.Value = activeQuery;
            try
            {
                calls();
            }
            finally
            {
                query.Value = null;
            }

            return activeQuery;
        }

        public void AddToQuery(object target, ICallSpecification callSpecification)
        {
            if (!IsQuerying)
                throw new NotRunningAQueryException();

            query.Value.Add(callSpecification, target);
        }
        #endregion Specialized members
    }
}
