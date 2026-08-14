using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace InventoryModule.Packer
{
    //TODO; Create A Data writer, Reader
    public sealed class InstanceDataWriter : InstanceDataServicePovider
    {

        public void Write<T>(T data, IInstanceable InstanceItem)
        {
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

    }
}
