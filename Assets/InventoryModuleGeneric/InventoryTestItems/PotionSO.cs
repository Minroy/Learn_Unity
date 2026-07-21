using InventoryModule;
using UnityEngine;

[CreateAssetMenu(fileName = "PotionSO", menuName = "Scriptable Objects/PotionSO")]
public class PotionSO : ItemScritableObject
{
    public override int MaxAmount => throw new System.NotImplementedException();

    public override Sprite Icon => throw new System.NotImplementedException();
}

