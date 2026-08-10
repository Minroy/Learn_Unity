using InventoryModule.IDSystem;
using InventoryModule.IDSystem.Instance;
using UnityEngine;

[CreateAssetMenu(fileName = "Arrows", menuName = "Scriptable Objects/Arrows")]
public class Arrows : InstanceItemScriptableObject
{
    public int dmg = 12;
    public int MaxCap = 122;
    public string Name; 
    public override int MaxAmount => throw new System.NotImplementedException();

    public override Sprite Icon => throw new System.NotImplementedException();
}

