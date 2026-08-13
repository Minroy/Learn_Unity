using InventoryModule;
using InventoryModule.IDSystem.Instance;
using UnityEngine;

[CreateAssetMenu(fileName = "Arrows", menuName = "Scriptable Objects/Arrows")]
public class Arrows : InstanceItemScriptableObject
{
    public int dmg = 12;
    public int MaxCap = 122;
    public string Name;
}

