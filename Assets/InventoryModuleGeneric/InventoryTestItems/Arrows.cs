using InventoryModule.IDSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "Arrows", menuName = "Scriptable Objects/Arrows")]
public class Arrows : ItemScriptableObject
{
    public override int MaxAmount => throw new System.NotImplementedException();

    public override Sprite Icon => throw new System.NotImplementedException();
}

