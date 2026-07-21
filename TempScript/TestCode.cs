#pragma warning disable
using UnityEngine;
using System.Collections.Generic;
using InventoryModule.Generics.Data;
using InventoryModule.Generics.Interfaces;

public class TestCode : MaxyGames.UNode.RuntimeBehaviour {	
	public StaticInventory<IItemData> StaticInven;
	
	private void Start() {
		StaticInven = 20;
	}

	private void Update() {
	}
}

