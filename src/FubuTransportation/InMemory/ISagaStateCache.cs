﻿using System;
using FubuCore.Util;
using FubuCore;

namespace FubuTransportation.InMemory
{
    public interface ISagaStateCacheFactory
    {
        ISagaStateCache<T> FindCache<T>() where T : class;
    }

    public class SagaStateCacheFactory : ISagaStateCacheFactory
    {
        private readonly Cache<Type, object> _cache = new Cache<Type, object>(type => {
            var cacheType = typeof (SagaStateCache<>).MakeGenericType(type);
            return Activator.CreateInstance(cacheType);
        });

        public ISagaStateCache<T> FindCache<T>() where T : class
        {
            return (ISagaStateCache<T>) _cache[typeof (T)];
        }
    }

    public interface ISagaStateCache<T> where T : class
    {
        void Store(Guid correlationId, T state);
        T Find(Guid correlationId);
        void Delete(Guid correlationId);
    }
}