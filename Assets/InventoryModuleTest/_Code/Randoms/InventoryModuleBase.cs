using System;
using System.Collections.Generic;
using UnityEngine;

namespace InventoryModule
{
    public class InventoryModuleBase : MonoBehaviour
    {
        private readonly Queue<Func<Awaitable>> operationQueue = new();
        private bool isProcessingQueue;

        /// <summary>
        /// Enqueues a structural operation.
        /// Operations run strictly one at a time, in order — an operation
        /// enqueued while another is mid-flight will not start until the
        /// current one has fully finished.
        /// </summary>
        protected Awaitable EnqueTaskAysc(Func<Awaitable> operation)
        {
            operationQueue.Enqueue(operation);

            if (!isProcessingQueue)
            {
                isProcessingQueue = true;
                _ = ProcessQueue();
            }

            return Awaitable.NextFrameAsync();
        }

        private async Awaitable ProcessQueue()
        {
            while (operationQueue.Count > 0)
            {
                var op = operationQueue.Dequeue();

                try
                {
                    await op();
                }
                catch (Exception e)
                {
                    Debug.LogException(e, this);
                }
            }

            isProcessingQueue = false;
        }
    }
}
