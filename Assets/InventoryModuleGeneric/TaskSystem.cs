using System;
using System.Collections.Generic;
using UnityEngine;

namespace InventoryModule.Multitasking
{
    public class TaskSystem
    {
        private readonly Queue<Func<Awaitable>> operationQueue = new();
        private bool isProcessingQueue;

        // ---- no return value ----
        protected Awaitable EnqueueTask(Func<Awaitable> operation)
        {
            var cts = new AwaitableCompletionSource();

            operationQueue.Enqueue(async () =>
            {
                try
                {
                    await operation();
                    cts.SetResult();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    cts.SetException(e);
                }
            });

            StartProcessingIfNeeded();
            return cts.Awaitable;
        }

        // ---- returns a value ----
        protected Awaitable<TResult> EnqueueTask<TResult>(Func<Awaitable<TResult>> operation)
        {
            var cts = new AwaitableCompletionSource<TResult>();

            operationQueue.Enqueue(async () =>
            {
                try
                {
                    var result = await operation();
                    cts.SetResult(result);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    cts.SetException(e);
                }
            });

            StartProcessingIfNeeded();
            return cts.Awaitable;
        }

        private void StartProcessingIfNeeded()
        {
            if (isProcessingQueue) return;
            isProcessingQueue = true;
            _ = ProcessQueue();
        }

        private async Awaitable ProcessQueue()
        {
            while (operationQueue.Count > 0)
            {
                var op = operationQueue.Dequeue();
                await op();
            }

            isProcessingQueue = false;
        }
    }
}