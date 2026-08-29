using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;


namespace InventoryModule.Packer
{
    //TODO High. Create InstanceDataPackers
    public sealed class InstanceDataWriter : InstanceDataServicePovider
    {
        public static InstanceDataWriter Instance { get; } = new();

        bool isWriting;

        public async void ProcessQueueWriteAysnc()
        {
            if (isWriting)
                return;

            isWriting = true;
            var BreakPoint = 5; // amount of items to process before returning frame back to unity. 
            var currentPoints = 0;
            
            while (WaitingListForWriting.Count > 0)
            {
                currentPoints++;

                WritingCurrentInstance = WaitingListForWriting.Dequeue();

                if (WritingCurrentInstance is IInstanceDataPacker packer)
                {
                    packer.WriteDataToPacker(this);
                }

                WritingCurrentInstance = null;

                if (currentPoints >= BreakPoint)
                {
                    currentPoints = 0;
                    await UniTask.Yield();
                }
            }

            isWriting = false;
        }

        public void InstantWrite<T>(T data)
        {
            Write(data);
        }

        public void Write<T>(T data)
        {
            if (WritingCurrentInstance is not null)
            {
                Debug.Log(WritingCurrentInstance.ItemId + "," + WritingCurrentInstance.InstanceId);
            }


        }

        public void Write(Object Object)
        {

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
        public static InstanceDataReader Instance { get; } = new();
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
        protected static Queue<IInstanceable> WaitingListForWriting = new();
        protected static Queue<IInstanceable> WaitingListForReading = new();
        protected static IInstanceable WritingCurrentInstance;

        public bool BeginWritingFor(IInstanceable CurrentInstance)
        {
            if (CurrentInstance is not null)
            {
                WaitingListForWriting.Enqueue(CurrentInstance);
                return true;
            }
            return false;
        }

        public bool BeginReadingFor(IInstanceable CurrentInstance)
        {
            if (CurrentInstance is not null)
            {
                WaitingListForReading.Enqueue(CurrentInstance);
            }
            return true;
        }
    }
}
