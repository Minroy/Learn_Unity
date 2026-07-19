#pragma warning disable
using InventoryModule.Iterfaces;

using System;
using UnityEngine;

namespace InventoryModule
{
    public abstract class ContainerModule : InventoryModuleBase, IContainerIdentifier, IIgnoreContainer, ICloneable, ICurrentStatus
    {
        public bool UpdateVisuals;
        public event Action ReloadEntireContainer;
        public enum LoadSlots
        {
            Load,
            UnLoad,
        }


        private bool isDisplaying = false;  // Start hidden
        public void Display()
        {
            if (!isDisplaying)
            {
                InventoryManager.MainDisplayerContainer.TryGetComponent<CanvasGroup>(out var canvasGroup);
            }
        }

        public void Hide()
        {
            if (isDisplaying)
            {
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
