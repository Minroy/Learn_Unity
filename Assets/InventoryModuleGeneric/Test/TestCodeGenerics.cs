using InventoryModule.Packer;
using UnityEngine;

public class FastBytesExampleTest : MonoBehaviour
{
    public FastBytes Convertor = new FastBytes();

    public void Awake()
    {
       var file = Convertor.Serialize(1);
    }

    public void OnEnable()
    {
        
    }
}