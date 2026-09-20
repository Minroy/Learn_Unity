using InventoryModule;
using InventoryModule.Packer;
using System.Collections.Generic;
using UnityEngine;

//TestItem
[CreateAssetMenu(fileName = "Arrows", menuName = "Scriptable Objects/Arrows")]
public class Arrows : InstanceItemScriptableObject
{
    int gagga = 1233;
    int gagdga = 1233;
    int gaggda = 1233;
    int gaddgga = 1233;

    public List<PotionSO> TestList = new();

    public override void DeserializeData(ByteReader reader)
    {
        throw new System.NotImplementedException();
    }

    public override void SerializeData(ByteWriter writer)
    {
        throw new System.NotImplementedException();
    }
}

public enum Waste
{

}

