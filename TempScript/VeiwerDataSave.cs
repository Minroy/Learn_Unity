#pragma warning disable
using InventoryModule.Windows;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace InventoryModule.SaveModules {
	public class GridLayoutDictionary {	
		protected static InventoryModule.SaveModules.GridLayoutDictionary Instance = new InventoryModule.SaveModules.GridLayoutDictionary();
		
		public void SaveVeiwerSettings() {
		}

		public void AddVeiwerSettings() {
		}

		public void LoadVeiwerSettings() {
		}

		public void OverrideVeiwerSettings() {
		}

		public void GetVeiwerSettings() {
		}
	}
	public struct VeiwerSettings {	
		public GridLayoutGroup gridLayoutGroup;
		public Canvas canvas;
		public CanvasScaler CanvasScaler;
		public GraphicRaycaster graphicRaycaster;
		public CanvasGroup canvasGroup;
	}

}
