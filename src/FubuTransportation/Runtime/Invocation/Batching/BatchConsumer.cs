﻿using System.Collections.Generic;
using FubuCore.Logging;

namespace FubuTransportation.Runtime.Invocation.Batching
{
    public abstract class BatchConsumer<T> where T : IBatchMessage
    {
        private readonly IMessageExecutor _executor;

        public BatchConsumer(IMessageExecutor executor)
        {
            _executor = executor;
        }

        public void Handle(T batch)
        {
            BatchStart(batch);

            batch.Messages.Each(x => _executor.Execute(x));

            BatchFinish(batch);
        }

        public virtual void BatchStart(T batch){}
        public virtual void BatchFinish(T batch){}
    }



}