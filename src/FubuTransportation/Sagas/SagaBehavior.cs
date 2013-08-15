﻿using System;
using FubuMVC.Core.Behaviors;
using FubuMVC.Core.Runtime;

namespace FubuTransportation.Sagas
{
    public class SagaBehavior<TState, TMessage, THandler> : WrappingBehavior where THandler : IStatefulSaga<TState> 
                                                                             where TMessage : class
                                                                             where TState : class
    {
        private readonly IFubuRequest _request;
        private readonly ISagaRepository<TState, TMessage> _repository;
        private readonly THandler _handler;

        public SagaBehavior(IFubuRequest request, ISagaRepository<TState, TMessage> repository, THandler handler)
        {
            _request = request;
            _repository = repository;
            _handler = handler;
        }

        protected override void invoke(Action action)
        {
            var message = _request.Get<TMessage>();
            _handler.State = _repository.Find(message);

            action();

            if (_handler.State == null) return;

            if (_handler.IsCompleted())
            {
                _repository.Delete(_handler.State);
            }
            else
            {
                _repository.Save(_handler.State);
            }
        }
    }
}