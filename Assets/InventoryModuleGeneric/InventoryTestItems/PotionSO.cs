using InventoryModule;
using InventoryModule.IDSystem.Instance;
using InventoryModule.Packer;
using UnityEditor;
using UnityEngine;

//exmaple of an item
[CreateAssetMenu(fileName = "PotionSO", menuName = "Scriptable Objects/PotionSO")]
public class PotionSO : ItemScriptableObject, IStackable , ISerializable , IDeserializable
{
    int Example1 = 123;
    ulong Exp = 3829019457832002;

    public bool CustomStackLogic()
    {
       return Exp.Equals(Example1);
    }

    public void OnWrite(InstanceDataWriter writer)
    {
        writer.Write(Example1);
        writer.Write(Exp);
    }

    public void OnRead(InstanceDataReader reader)
    {
      
    }

   
}

