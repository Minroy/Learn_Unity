using Cysharp.Threading.Tasks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using UnityEngine;


namespace InventoryModule.Packer
{


    public sealed class InstanceDataWriter : InstanceDataServicePovider
    {
        public static InstanceDataWriter Instance = new InstanceDataWriter();

        private ByteWriter _byteWriter = new ByteWriter();


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

                            //Error Fix. Dont remove
                            //Designed to fix a Issue with readonlyspans not being allowed in async methods. 
                            StoreBufferDataToFile(Instance._byteWriter);

                            //some sotrage here 

                            Instance._byteWriter.Reset();
                        }
                        catch (Exception ex)
                        {
                            Instance._byteWriter.Reset();
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

        private void StoreBufferDataToFile(ByteWriter byteWriter)
        {
           ReadOnlySpan<byte> Data = byteWriter.AsSpan();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(bool value) => Instance._byteWriter.Write(value);



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte value) => Instance._byteWriter.Write(value);



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(sbyte value) => Instance._byteWriter.Write(value);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(short value) => Instance._byteWriter.Write(value);




        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ushort value) => Instance._byteWriter.Write(value);




        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(int value) => Instance._byteWriter.Write(value);



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(uint value) => Instance._byteWriter.Write(value);



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(long value) => Instance._byteWriter.Write(value);



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ulong value) => Instance._byteWriter.Write(value);



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(float value) => Instance._byteWriter.Write(value);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(double value) => Instance._byteWriter.Write(value);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(decimal value) => Instance._byteWriter.Write(value);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(char value) => Instance._byteWriter.Write(value);



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(string value) => Instance._byteWriter.Write(value);



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(IList<T> list) => Instance._byteWriter.Write(list);



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<TEnum>(TEnum value) where TEnum : struct, System.Enum => Instance._byteWriter.Write(value);




        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteNull() => Instance._byteWriter.WriteNull();

        public void Write(UnityEngine.Object value)
        {

        }


        [MethodImpl]
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

        public void Write(Vector2 Vector2)
        {
            Write(Vector2.x);
            Write(Vector2.y);
        }
        public void Write(Vector3 Vector3)
        {
            Write(Vector3.x);
            Write(Vector3.y);
            Write(Vector3.z);
        }
        public void Write(Vector4 Vector4)
        {
            Write(Vector4.x);
            Write(Vector4.y);
            Write(Vector4.z);
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
