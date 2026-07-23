using InventoryModule.IDSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "PotionSO", menuName = "Scriptable Objects/PotionSO")]
public class PotionSO : ItemScriptableObject
{
    public override int MaxAmount => throw new System.NotImplementedException();

    public override Sprite Icon => throw new System.NotImplementedException();
}

