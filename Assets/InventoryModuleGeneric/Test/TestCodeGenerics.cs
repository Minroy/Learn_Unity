using InventoryModule;
using UnityEngine;

public class TestCodeGenerics : MonoBehaviour
{
    [SerializeField] private PotionSO testPotion1;
    [SerializeField] private PotionSO testPotion2;
    [SerializeField] private PotionSO testPotion3;


    void Start()
    {
        Debug.Log(testPotion1.ItemID);
        Instantiate(testPotion1);
        Instantiate(testPotion2);
        Instantiate(testPotion3);
    }
}

public class TestItems : MonoBehaviour, IItem
{
    public uint? ItemID => throw new System.NotImplementedException();

    public int MaxAmount => throw new System.NotImplementedException();

    public Sprite Icon => throw new System.NotImplementedException();

    public void SetID(uint id)
    {
        throw new System.NotImplementedException();
    }
}

public class TestItem2 : MonoBehaviour, IItem
{
    public uint? ItemID => throw new System.NotImplementedException();

    public int MaxAmount => throw new System.NotImplementedException();

    public Sprite Icon => throw new System.NotImplementedException();

    public void SetID(uint id)
    {
        throw new System.NotImplementedException();
    }
}