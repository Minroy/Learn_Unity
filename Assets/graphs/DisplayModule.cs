#pragma warning disable
using InventoryModule;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using InventoryModule;


namespace InventoryModule.Windows
{
    /// <summary> This displays a Containers internal List. </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class DisplayModule : InventoryModuleBase
    {
        private ContainerBehaviour CurrentContainer;
        private CanvasGroup CanvasGroup;



        /// <summary> displayes the given container </summary>
        public void Display()
        {
        }
        public void Display(ContainerBehaviour Containerbehaviour)
        {
        }

        /// <summary> displayes the given container, with canvas options </summary>
        public void Display(ContainerBehaviour Containerbehaviour, Canvas Canvas)
        {
        }

        /// <summary> displayes the given container, with canvas options </summary>
        public void Display(ContainerBehaviour Containerbehaviour, Canvas Canvas, SlotContext SlotContext)
        {

        }

        /// <summary> Hides the current Container </summary>
        public void Hide()
        {

        }

        /// <summary> Hides the current Container </summary>
        public void Hide(ContainerBehaviour containerBehaviour)
        {

        }

        public void Hide(ContainerBehaviour Containerbehaviour, Canvas Canvas)
        {

        }

        /// <summary> displayes the given container, with canvas options </summary>
        public void Hide(ContainerBehaviour Containerbehaviour, Canvas Canvas, SlotContext SlotContext)
        {

        }
    }

}
