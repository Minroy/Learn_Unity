using UnityEngine;
using InventoryModule.Windows;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace InventoryModule
{
    public class A : MonoBehaviour
    {
        public void Awake()
        {
            Execute();
        }

        public virtual void Execute()
        {

        }
    }

    public class B : A
    {
        public override void Execute()
        {
            Debug.Log("yffu");
        }
    }

    public class C : B
    {
        public override void Execute()
        {
            Debug.Log("yffu");
        }
    }
    public class E : B
    {
        public override void Execute()
        {
            Debug.Log("yffu");
        }
    }
    public class F : B
    {
        public override void Execute()
        {
            Debug.Log("yffu");
        }
    }
}