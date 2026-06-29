

using UnityEngine;

public interface IInventoryListImplemet
{
    public InventoryList CurrentList { get; set; }
    bool Maxamount { get; set; }
}

internal interface IContainerRequirements : IInventoryListImplemet
{
    public Transform CurrentTransform { get; set; }
}
