using Cysharp.Threading.Tasks;
using InventoryModule.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using K4os.Compression.LZ4;


namespace InventoryModule.Packer
{


    public sealed class InstanceDataWriter : InstanceDataServicePovider
    {
        public static InstanceDataWriter Instance { get; } = new InstanceDataWriter();

        private readonly ByteWriter _byteWriter = new();

        private CompressionMode CompressionMode = CompressionMode.LZ4_Min;

        private bool isWriting;

        private InstanceDataWriter()
        {

        }


        

        public void CompressionType(CompressionMode mode)
        {
            CompressionMode = mode;
        }

        public async UniTask ProcessWrite()
        {
            if (isWriting)
                return;

            isWriting = true;

            try
            {
                while (waitingListForWriting.TryDequeue(out var itemToPack))
                {
                    if (itemToPack is IInstanceDataPacker packer)
                    {
                        try
                        {
                            packer.WriteDataToPacker(this);

                            SaveDataToRegistry(itemToPack);

                            _byteWriter.Reset();
                        }
                        catch (Exception ex)
                        {
                            _byteWriter.Reset();

                            Debug.LogError($"Serialization failed: {ex}");
                        }
                    }

                    await UniTask.NextFrame();
                }
            }
            finally
            {
                isWriting = false;
            }
        }

        public void SaveDataToRegistry(IInstanceable itemToPack)
        {
            byte[] compressedData = LZ4Pickler.Pickle(_byteWriter.ToArray(), LZ4Level.L00_FAST);
            InstanceGlobalRegistry.Add(itemToPack, compressedData);
        }
    


        // =========================
        // Primitive Types
        // =========================

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(bool value) =>
            _byteWriter.Write(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte value) =>
            _byteWriter.Write(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(sbyte value) =>
            _byteWriter.Write(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(short value) =>
            _byteWriter.Write(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ushort value) =>
            _byteWriter.Write(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(int value) =>
            _byteWriter.Write(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(uint value) =>
            _byteWriter.Write(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(long value) =>
            _byteWriter.Write(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ulong value) =>
            _byteWriter.Write(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(float value) =>
            _byteWriter.Write(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(double value) =>
            _byteWriter.Write(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(decimal value) =>
            _byteWriter.Write(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(char value) =>
            _byteWriter.Write(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(string value) =>
            _byteWriter.Write(value);


        // =========================
        // Collections
        // =========================

        public void Write<T>(List<T> list)
        {
            if (list == null) throw new ArgumentNullException($"{typeof(T)} is Null");

            if (typeof(ISerializable).IsAssignableFrom(typeof(T)))
            {
                _byteWriter.Write(list.Count);

                foreach (var item in list)
                {
                    Write((ISerializable)item);
                }
                return;
            }

            _byteWriter.Write(list);
        }

        public void Write<T>(T Object) where T : ISerializable
        {
            if (Object is null) return;
            if (Object is not IDeserializable)
            {
                Debug.LogWarning($"{Object}, Needs to have a IDeserializable also or the System wont be able to read");
                return;
            }

            Object.OnWrite(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(List<int> list) =>
            _byteWriter.Write(list);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(List<string> list) =>
            _byteWriter.Write(list);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(List<float> list) =>
            _byteWriter.Write(list);


        // =========================
        // Enum
        // =========================

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(Enum value) =>
            _byteWriter.Write(value);


        // =========================
        // Custom-Types using Iserializable and IDesializable
        // =========================

        public void WriteType<T>(T Object) where T : ISerializable
        {
            
        }

        // =========================
        // Unity Objects
        // =========================

        public void Write(Sprite sprite)
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

        public void Write(Vector2 value)
        {
            Write(value.x);
            Write(value.y);
        }

        public void Write(Vector3 value)
        {
            Write(value.x);
            Write(value.y);
            Write(value.z);
        }

        public void Write(Vector4 value)
        {
            Write(value.x);
            Write(value.y);
            Write(value.z);
            Write(value.w);
        }
    }

    public sealed class InstanceDataReader : InstanceDataServicePovider
    {


    }

    // this is just a Class that both read and write can use
    public class InstanceDataServicePovider
    {
        protected static ConcurrentQueue<IInstanceable> waitingListForWriting = new();
        protected static ConcurrentQueue<IInstanceable> waitingListForReading = new();

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
