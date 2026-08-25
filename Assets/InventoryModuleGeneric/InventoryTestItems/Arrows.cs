using InventoryModule;
using InventoryModule.Packer;
using UnityEngine;

//TestItem
[CreateAssetMenu(fileName = "Arrows", menuName = "Scriptable Objects/Arrows")]
public class Arrows : InstanceItemScriptableObject
{
    public int dmg = 12;
    public int MaxCap = 122;
    public string Name;

    public Arrows aa;

    public override void ReadDataFormPacker(InstanceDataReader reader)
    {
        dmg = reader.Read(dmg);
        MaxCap = reader.Read(MaxCap);
        Name = reader.Read(Name);
    }

    public override void WriteDataToPacker(InstanceDataWriter writer)
    {
        Debug.Log("hvhfv");
        writer.Write(dmg);
        writer.Write(MaxCap);
        writer.Write(Name);
    }
}

