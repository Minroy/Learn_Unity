using InventoryModule.Iterfaces;
using System;
using UnityEngine;

namespace InventoryModule
{
    public class ContainerModule : MonoBehaviour, IContainerIdentifier, IIgnoreContainer, ICloneable, ICurrentStatus
    {
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

        // check where you are OPen or not. 
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
