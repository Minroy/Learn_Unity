#pragma warning disable
using InventoryModule;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Examples
{
    public class PlayerInventoryInput : MonoBehaviour
    {
        [SerializeField]
        private float Range;
        [SerializeField]
        private Camera MainCamera;

        private ContainerBehaviour displayContainer;

        private void Awake()
        {
        }

        public void Update()
        {
            var MainRay = new Ray(MainCamera.transform.position, MainCamera.transform.forward);
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (Physics.Raycast(MainRay, out RaycastHit hitInfo, Range))
                {
                    if ((hitInfo.collider != null))
                    {
                        displayContainer = hitInfo.collider.gameObject.GetComponent<ContainerBehaviour>();
                        displayContainer.Display();
                    }
                    else return;
                }
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                displayContainer?.Hide();
            }



        }
    }

}
