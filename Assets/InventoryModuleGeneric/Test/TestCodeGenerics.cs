
using System;
using UnityEngine;
using InventoryModule.Packer;

public class BytePackerBenchmark : MonoBehaviour
{
    private const int ELEMENT_COUNT = 100_000_000;
    private const int ITERATIONS = 10;
    private const int WARMUP = 3;

    private void Start()
    {
        RunBenchmark();
    }

    private void RunBenchmark()
    {
        Debug.Log("========================================");
        Debug.Log(" ByteWriter / ByteReader Benchmark");
        Debug.Log("========================================");

        // ------------------------------------------------------------
        // Prepare deterministic source data
        // ------------------------------------------------------------

        long[] source = new long[ELEMENT_COUNT];

        for (int i = 0; i < source.Length; i++)
        {
            source[i] = GenerateValue(i);
        }

        Debug.Log($"Elements      : {ELEMENT_COUNT:N0}");
        Debug.Log($"Iterations    : {ITERATIONS}");
        Debug.Log($"Warmup        : {WARMUP}");

        // ------------------------------------------------------------
        // Warmup
        // ------------------------------------------------------------

        Debug.Log("Warming up...");

        for (int i = 0; i < WARMUP; i++)
        {
            byte[] data = Serialize(source);
            DeserializeAndValidate(data, source);
        }

        // ------------------------------------------------------------
        // WRITE BENCHMARK
        // ------------------------------------------------------------

        long totalWriteTicks = 0;
        int serializedSize = 0;
        Debug.Log("Starting benchmark...");  // Add this line
        var sw = new System.Diagnostics.Stopwatch();

        for (int iteration = 0; iteration < ITERATIONS; iteration++)
        {
            sw.Restart();

            byte[] data = Serialize(source);

            sw.Stop();

            totalWriteTicks += sw.ElapsedTicks;
            serializedSize = data.Length;
        }

        double writeMilliseconds =
            TicksToMilliseconds(totalWriteTicks) / ITERATIONS;

        // ------------------------------------------------------------
        // Create one final buffer for READ benchmark
        // ------------------------------------------------------------

        byte[] benchmarkData = Serialize(source);

        // ------------------------------------------------------------
        // READ BENCHMARK
        // ------------------------------------------------------------

        long totalReadTicks = 0;

        for (int iteration = 0; iteration < ITERATIONS; iteration++)
        {
            sw.Restart();

            DeserializeAndValidate(benchmarkData, source);

            sw.Stop();

            totalReadTicks += sw.ElapsedTicks;
        }

        double readMilliseconds =
            TicksToMilliseconds(totalReadTicks) / ITERATIONS;

        // ------------------------------------------------------------
        // Results
        // ------------------------------------------------------------

        double megabytes = serializedSize / (1024.0 * 1024.0);

        double writeMBps =
            megabytes / (writeMilliseconds / 1000.0);

        double readMBps =
            megabytes / (readMilliseconds / 1000.0);

        Debug.Log("");
        Debug.Log("========================================");
        Debug.Log(" RESULTS");
        Debug.Log("========================================");

        Debug.Log($"Serialized size : {serializedSize:N0} bytes");
        Debug.Log($"                 {megabytes:F2} MB");

        Debug.Log("");

        Debug.Log($"Write time      : {writeMilliseconds:F4} ms");
        Debug.Log($"Write speed     : {writeMBps:F2} MB/s");

        Debug.Log("");

        Debug.Log($"Read time       : {readMilliseconds:F4} ms");
        Debug.Log($"Read speed      : {readMBps:F2} MB/s");

        Debug.Log("========================================");
    }

    // ================================================================
    // SERIALIZE
    // ================================================================

    private byte[] Serialize(long[] values)
    {
        using (var writer = new ByteWriter(values.Length * sizeof(long) + 4))
        {
            writer.Write(values.Length);

            for (int i = 0; i < values.Length; i++)
            {
                writer.Write<long>(values[i]);
            }

            return writer.ToArray();
        }
    }

    // ================================================================
    // DESERIALIZE + VALIDATE
    // ================================================================

    private void DeserializeAndValidate(byte[] data, long[] expected)
    {
        using (var reader = new ByteReader(data))
        {
            reader.Read(out int count);

            if (count != expected.Length)
            {
                throw new Exception(
                    $"COUNT MISMATCH! Expected {expected.Length}, got {count}");
            }

            for (int i = 0; i < count; i++)
            {
                reader.Read(out long actual);

                long expectedValue = expected[i];

                if (actual != expectedValue)
                {
                    throw new Exception(
                        $"DATA CORRUPTION at index {i}!\n" +
                        $"Expected: {expectedValue}\n" +
                        $"Actual:   {actual}");
                }
            }
        }
    }

    // ================================================================
    // DETERMINISTIC TEST DATA
    // ================================================================

    private long GenerateValue(int index)
    {
        switch (index % 8)
        {
            case 0:
                return 0;

            case 1:
                return long.MaxValue;

            case 2:
                return long.MinValue;

            case 3:
                return -1;

            case 4:
                return 1234567890123456789L;

            case 5:
                return -1234567890123456789L;

            case 6:
                return index;

            default:
                return unchecked(
                    (long)index * 0x123456789ABCDEF
                );
        }
    }

    private double TicksToMilliseconds(long ticks)
    {
        return ticks * 1000.0 / System.Diagnostics.Stopwatch.Frequency;
    }
}
public class TestItems : MonoBehaviour /*IItem*/
{
    public uint ItemId => throw new System.NotImplementedException();

    public int MaxAmount => throw new System.NotImplementedException();

    public Sprite Icon => throw new System.NotImplementedException();

    public void SetID(uint id)
    {
        throw new System.NotImplementedException();
    }
}

public class TestItem2 : MonoBehaviour/*, IItem*/
{
    public uint ItemId => throw new System.NotImplementedException();

    public int MaxAmount => throw new System.NotImplementedException();

    public Sprite Icon => throw new System.NotImplementedException();

    public void SetID(uint id)
    {
        throw new System.NotImplementedException();
    }
}