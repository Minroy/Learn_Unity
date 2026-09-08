using Cysharp.Threading.Tasks;
using InventoryModule.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


namespace InventoryModule.Packer
{
    

    public sealed class InstanceDataWriter : InstanceDataServicePovider
    {
        public static InstanceDataWriter Instance = new InstanceDataWriter();

        private ByteWriter ByteWriter = new ByteWriter();

        bool isWriting;

        public async UniTask ProcessWrite()
        {
            
            if (Instance.isWriting) return;

            Instance.isWriting = true;

            try
            {
                while (waitingListForWriting.TryDequeue(out var ItemToPacker))
                {
                    if (ItemToPacker is IInstanceDataPacker packer)
                    {
                        try
                        {
                            packer.WriteDataToPacker(Instance);
                            byte[] Bytes = ByteWriter.ToArray();
                            
                            //some sotrage here 

                            ByteWriter.Reset();
                        }
                        catch (Exception ex)
                        {
                            ByteWriter.Reset();
                            Instance.isWriting = false;
                            Debug.LogError($"Serialization failed: {ex}");
                        }
                    }

                    await UniTask.NextFrame();
                }
            }
            finally
            {
                Instance.isWriting = false;
            }
        }

        public void Write(bool value) => ByteWriter.Write(value);

        public void Write(byte value) => ByteWriter.Write(value);
        public void Write(sbyte value) => ByteWriter.Write(value);

        public void Write(short value) => ByteWriter.Write(value);
        public void Write(ushort value) => ByteWriter.Write(value);

        public void Write(int value) => ByteWriter.Write(value);
        public void Write(uint value) => ByteWriter.Write(value);

        public void Write(long value) => ByteWriter.Write(value);
        public void Write(ulong value) => ByteWriter.Write(value);

        public void Write(float value) => ByteWriter.Write(value);
        public void Write(double value) => ByteWriter.Write(value);
        public void Write(decimal value) => ByteWriter.Write(value);

        public void Write(char value) => ByteWriter.Write(value);
        public void Write(string value) => ByteWriter.Write(value);

        public void Write<T>(IList<T> list) => ByteWriter.Write(list);
        public void Write<TEnum>(TEnum value) where TEnum : struct, System.Enum => ByteWriter.Write(value);

        public void WriteNull() => ByteWriter.WriteNull();

        public void Write(UnityEngine.Object value)
        {

        }

        public void Write(Transform transform)
        {
            Write(transform.position.x);
            Write(transform.position.y);
            Write(transform.position.z);

            Write(transform.rotation.x);
            Write(transform.rotation.y);
            Write(transform.rotation.z);
            Write(transform.rotation.w);

            Write(transform.localScale.x);
            Write(transform.localScale.y);
            Write(transform.localScale.z);
        }
    }

    public sealed class InstanceDataReader : InstanceDataServicePovider
    {


    }

    // this is just a Class that both read and write can use
    public class InstanceDataServicePovider
    {
        protected static ConcurrentQueue<IInstanceable> waitingListForWriting = new ConcurrentQueue<IInstanceable>();
        protected static ConcurrentQueue<IInstanceable> waitingListForReading = new ConcurrentQueue<IInstanceable>();

        public void BeginWriting(IInstanceable instance)
        {
            if (instance == null) return;

            waitingListForWriting.Enqueue(instance);
            InstanceDataWriter.Instance.ProcessWrite().Forget();
        }
        public void BeginReading(IInstanceable instance)
        {
            if (instance == null) return;

            waitingListForReading.Enqueue(instance);
        }

    }
}
