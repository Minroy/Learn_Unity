using InventoryModule.IDSystem;
using UnityEngine;

public class TestCodeGenerics : MonoBehaviour
{
    //private FixedInventory<PotionSO> potionSOs = new(10);
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

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