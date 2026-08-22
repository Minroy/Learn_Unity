using InventoryModule;
using InventoryModule.IDSystem.Instance;
using UnityEditor;
using UnityEngine;

//exmaple of an item
[CreateAssetMenu(fileName = "PotionSO", menuName = "Scriptable Objects/PotionSO")]
public class PotionSO : ItemScriptableObject, IStackable
{

    public bool CustomStackLogic()
    {
       return Exp.Equals(Example1);
    }


    int Example1 = 123;
    ulong Exp = 3829019457832002;

    public Arrows Arrows;

    public IItem item;
}

