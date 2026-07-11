#pragma warning disable
using UnityEngine;
using System.Collections.Generic;
using InventoryModule;
using System;

namespace Inventory.Examples {
	public class PlayerInventoryInput : MonoBehaviour 
	{
        private void Awake()
        {
            InventoryManager.Register(nameof(Logger), (Action<string>)Logger);
        }


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                InventoryManager.ExecuteLogic(nameof(Logger), "eweweweewe");
            }
        }

        public void Logger(string message)
        {
            Debug.Log(message);
        }
    }

}
