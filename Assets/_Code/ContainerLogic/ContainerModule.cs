using InventoryModule.Iterfaces;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace InventoryModule
{
    public class ContainerModule : MonoBehaviour, IContainerIdentifier, IIgnoreContainer, ICloneable, ICurrentStatus
    {
        private readonly Queue<Func<Awaitable>> operationQueue = new();
        private bool isProcessingQueue;

        /// <summary>
        /// <summary>
        /// Enqueues a structural operation.
        /// Operations run strictly one at a time, in order — an operation
        /// enqueued while another is mid-flight will not start until the
        /// current one has fully finished.
        /// </summary>
        /// </summary>
        protected Awaitable EnqueueContainer(Func<Awaitable> operation)
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




        

        public object Clone()
        {
            throw new NotImplementedException();
        }


        public void IgnoreContainer(bool ignore)
        {

        }

        public bool IsIgnoreContainer(bool ignore)
        {
            throw new NotImplementedException();
        }

        // check where you are OPen or not. 
        public ContainerStatus GetContainerStatus()
        {
            throw new NotImplementedException();
        }

        public void SetCurrentStatus(ContainerStatus status)
        {
            throw new NotImplementedException();
        }
    }
}
