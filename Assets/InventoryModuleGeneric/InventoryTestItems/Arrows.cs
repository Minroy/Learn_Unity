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
    public override void DeserializeData(InstanceDataReader reader)
    {

    }
    public override void SerializeData(InstanceDataWriter writer)
    {
        writer.Write(gagga);
        writer.Write(gagdga);
        writer.Write(gaggda);
        writer.Write(gaddgga);
        writer.Write(TestList);
    }
}

public struct Waste
{

}

