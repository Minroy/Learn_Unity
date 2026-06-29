///--------------------------------------------///
///-----MADE WITH: UNODE VISUAL SCRIPTING-----///
///------------------------------------------///
#pragma warning disable
using UnityEngine;
using System.Collections.Generic;

public class PlayerInput : MonoBehaviour {	
	public GameObject ContainerTrans;
	public InventoryContainer InventoryContainer;
	public ItemSO Item1;
	public ItemSO Item2;
	
	private void Awake() {
		InventoryContainer = new InventoryContainer(true, ContainerTrans.transform.childCount, -1);
	}
	
	private void Update() {
		if(Input.GetKeyDown(KeyCode.Keypad1)) {
			InventoryLogicHandler.instance.Add(InventoryContainer, Item1, 40);
		}
		 else if(Input.GetKeyDown(KeyCode.Keypad2)) {
			InventoryLogicHandler.instance.Add(InventoryContainer, Item2, 40);
		}
	}
}

