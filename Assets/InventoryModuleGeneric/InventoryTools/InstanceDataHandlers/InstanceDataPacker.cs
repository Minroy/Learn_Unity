using UnityEngine;

namespace InventoryModule.Packer
{
    //PR:HIGH =  TODO: Create A Data writer, Reader
    public sealed class InstanceDataWriter : InstanceDataServicePovider
    {

        public static InstanceDataWriter Instance { get; } = new();

        public void Write<T>(T data, uint ItemId = default, ulong InstanceId = default)
        {
            ItemId = CurrentItemWriterId;
            InstanceId = CurrentInstanceWriterId;

            Debug.Log($"{ItemId}it/ {InstanceId}" );
            if (ItemId == 0 || InstanceId == 0) return;


            if (data is null)
            {
                WriteNull();
                return;
            }
        }

        private void WriteValueType<T>(T valueType) where T : struct
        {

        }

        private void WriteRefTypes<T>(T RefValue) where T : class
        {

        }

        private void WriteNull()
        {
            // 
        }
    }

    public sealed class InstanceDataReader : InstanceDataServicePovider
    {
        public T Read<T>(T toRead)
        {
            return default(T); // placeholder
        }

        public void Read<T>(out T value)
        {
            value = default(T);
        }

        private T ReadValueType<T>(T ValueType) where T : struct
        {
            return default(T);
        }
        private T ReadRefType<T>(T RefType) where T : class
        {
            return default(T);
        }
    }

    // this is just a Class that both read and write can use. Like
    public class InstanceDataServicePovider
    {
        protected uint CurrentItemWriterId = 0;
        protected ulong CurrentInstanceWriterId = 0;
        protected IInstanceDataPacker ind;
        public bool Begin(IInstanceable instanceable)
        {
            CurrentInstanceWriterId = instanceable.InstanceId;
            CurrentItemWriterId = instanceable.ItemId;
            return true;
        }
    }
}
