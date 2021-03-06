﻿using Composite.Core.Instrumentation;
using Composite.Core.Instrumentation.Plugin;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;


namespace Composite.Plugins.Instrumentation.PerformanceCounterProviders.NoPerformanceCounterProvider
{
    [ConfigurationElementType(typeof(NonConfigurablePerformanceCounterProvider))]
    internal sealed class NoPerformanceCounterProvider : IPerformanceCounterProvider
    {
        NoPerformanceCounterProviderPerformanceToken _noPerformanceCounterProviderPerformanceToken = new NoPerformanceCounterProviderPerformanceToken();

        public void SystemStartupIncrement()
        {
        }

        public IPerformanceCounterToken BeginElementCreation()
        {
            return _noPerformanceCounterProviderPerformanceToken;
        }

        public void EndElementCreation(IPerformanceCounterToken performanceToken, int resultElementCount, int totalElementCount)
        {
        }

        public IPerformanceCounterToken BeginAspNetControlCompile()
        {
            return _noPerformanceCounterProviderPerformanceToken;
        }

        public void EndAspNetControlCompile(IPerformanceCounterToken performanceToken, int controlsCompiledCount)
        {
        }

        public IPerformanceCounterToken BeginPageHookCreation()
        {
            return _noPerformanceCounterProviderPerformanceToken;
        }

        public void EndPageHookCreation(IPerformanceCounterToken performanceToken, int pageCount)
        {
        }

        public void EntityTokenParentCacheHitIncrement()
        {            
        }

        public void EntityTokenParentCacheMissIncrement()
        {            
        }

        private sealed class NoPerformanceCounterProviderPerformanceToken : IPerformanceCounterToken
        {
            public void Dispose()
            {
            }
        }        
    }
}
