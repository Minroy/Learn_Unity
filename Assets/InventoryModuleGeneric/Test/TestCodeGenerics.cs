using InventoryModule;
using UnityEngine;

public class TestCodeGenerics : MonoBehaviour
{
    [SerializeField] private Arrows testPotion1;
    [SerializeField] private Arrows testPotion2;
    [SerializeField] private PotionSO testPotion3;


    void Start()
    {
        Debug.Log(testPotion1.InstanceID);
        testPotion2 = Instantiate(testPotion1);
        Arrows testest4 = Instantiate(testPotion1);
        Debug.Log(testPotion2.InstanceID);
        Debug.Log(testPotion1.InstanceID);
        Debug.Log(testest4.InstanceID);
    }

}

public class TestItems : MonoBehaviour, IItem
{
    public uint ItemID => throw new System.NotImplementedException();

    public int MaxAmount => throw new System.NotImplementedException();

    public Sprite Icon => throw new System.NotImplementedException();

    public void SetID(uint id)
    {
        throw new System.NotImplementedException();
    }
}

public class TestItem2 : MonoBehaviour, IItem
{
    public uint ItemID => throw new System.NotImplementedException();

    public int MaxAmount => throw new System.NotImplementedException();

    public Sprite Icon => throw new System.NotImplementedException();

    public void SetID(uint id)
    {
        throw new System.NotImplementedException();
    }
}