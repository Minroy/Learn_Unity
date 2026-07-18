#pragma warning disable
using UnityEngine;
using System.Collections.Generic;
using InventoryModule;
using System.ComponentModel;

namespace InventoryModule.Windows {
	/// <summary> This displays a Containers internal List. </summary>
	[RequireComponent(typeof(CanvasGroup))]
	public class DisplayModule : InventoryModuleBase {	
		/// <summary> displayes the given container </summary>
		public void Display(ContainerBehaviour newParameter) {
		}

		/// <summary> displayes the given container, with canvas options </summary>
		public void Display(ContainerBehaviour Containerbehaviour, Canvas Canvas) {
		}

		/// <summary> displayes the given container, with canvas options </summary>
		public void Display(ContainerBehaviour Containerbehaviour, Canvas Canvas, SlotContext SlotContext) {
		}

		/// <summary> Hides the current Container </summary>
		public void Hide() {
		}

		/// <summary> Hides the current Container </summary>
		public void Hide(object parameter) {
		}
	}

}
