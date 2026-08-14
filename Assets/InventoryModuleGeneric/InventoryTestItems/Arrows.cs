using InventoryModule;
using InventoryModule.Packer;
using UnityEngine;

//TestItem
[CreateAssetMenu(fileName = "Arrows", menuName = "Scriptable Objects/Arrows")]
public class Arrows : InstanceItemScriptableObject, IInstanceDataPacker, IInstanceDataPackerAuto
{
    public int dmg = 12;
    public int MaxCap = 122;
    public string Name;

    public void ReadDataFormPacker(InstanceDataReader reader)
    {
        dmg = reader.Read(dmg);
        MaxCap = reader.Read(MaxCap);
        Name = reader.Read(Name);
    }

    public void WriteDataToPacker(InstanceDataWriter writer)
    {
        writer.Write(dmg, this);
        writer.Write(MaxCap, this);
        writer.Write(Name, this);
    }
}

