using InventoryModule.Iterfaces;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace InventoryModule
{
    public class ContainerModule : MonoBehaviour, IContainerIdentifier, IIgnoreContainer, ICloneable, ICurrentStatus
    {
        [SerializeField] CanvasGroup canvasGroup;
        private bool isDisplaying = false;  // Start hidden


        private readonly Queue<Func<Awaitable>> operationQueue = new();
        private bool isProcessingQueue;

        /// <summary>
        /// Enqueues a structural operation.
        /// Operations run strictly one at a time, in order — an operation
        /// enqueued while another is mid-flight will not start until the
        /// current one has fully finished.
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




        public void Display()
        {
            if (!isDisplaying)
            {
                EnqueueContainer(async () =>
                {
                    canvasGroup.alpha = 1;
                    canvasGroup.blocksRaycasts = true;
                    isDisplaying = true;
                    await Awaitable.NextFrameAsync();
                });
            }
        }

        public void Hide()
        {
            if (isDisplaying)
            {
                EnqueueContainer(async () =>
                {
                    canvasGroup.alpha = 0;
                    canvasGroup.blocksRaycasts = false;
                    isDisplaying = false;
                    await Awaitable.NextFrameAsync();
                });
            }
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

        // check whereter you are OPen or not. 
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
