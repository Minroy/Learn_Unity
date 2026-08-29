using InventoryModule;
using InventoryModule.Packer;
using UnityEngine;

public class TestCodeGenerics : MonoBehaviour
{
    [SerializeField] private Arrows testPotion1;
    [SerializeField] private Arrows testPotion2;
    [SerializeField] private PotionSO testPotion3;


    void Start()
    {
        Instantiate(testPotion1);
        testPotion2 = Instantiate(testPotion1);
        for (int i = 0; i < 10000; i++)
        {
            Instantiate(testPotion2);
               
        }
        InstanceDataWriter.Instance.ProcessQueueWriteAysnc();
    }


}

public class TestItems : MonoBehaviour, IItem
{
    public uint ItemId => throw new System.NotImplementedException();

    public int MaxAmount => throw new System.NotImplementedException();

    public Sprite Icon => throw new System.NotImplementedException();

    public void SetID(uint id)
    {
        throw new System.NotImplementedException();
    }
}

public class TestItem2 : MonoBehaviour, IItem
{
    public uint ItemId => throw new System.NotImplementedException();

    public int MaxAmount => throw new System.NotImplementedException();

    public Sprite Icon => throw new System.NotImplementedException();

    public void SetID(uint id)
    {
        throw new System.NotImplementedException();
    }
}