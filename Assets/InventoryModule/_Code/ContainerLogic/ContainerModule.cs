using InventoryModule.Iterfaces;
using InventoryModule.Windows;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace InventoryModule
{
    public class ContainerModule : MonoBehaviour, IContainerIdentifier, IIgnoreContainer, ICloneable, ICurrentStatus
    {
        protected Viewer mainViewer;
        private bool isDisplaying = false;  // Start hidden


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




        public void Display()
        {
            if (!isDisplaying)
            {
                ////EnqueTaskAysc(async () =>
                ////{
                ////    canvasGroup.alpha = 1;
                ////    canvasGroup.blocksRaycasts = true;
                ////    isDisplaying = true;
                ////    await Awaitable.NextFrameAsync();
                ////});
            }
        }

        public void Hide()
        {
            if (isDisplaying)
            {
                ////EnqueTaskAysc(async () =>
                ////{
                ////    canvasGroup.alpha = 0;
                ////    canvasGroup.blocksRaycasts = false;
                ////    isDisplaying = false;
                ////    await Awaitable.NextFrameAsync();
                ////});
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

        #region Container TransferHandler

        public ContainerBehaviour QuickTransferTo { get; private set; }

        public void SetTransferTo(ContainerBehaviour containerToTransfer)
        {
            QuickTransferTo = containerToTransfer;
        }

        #endregion
    }
}
