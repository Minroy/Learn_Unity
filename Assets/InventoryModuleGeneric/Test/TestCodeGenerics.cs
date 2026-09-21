using System;

using UnityEngine;
using InventoryModule.Packer;

public class BytePackerStringBenchmark : MonoBehaviour
{
    private const int ELEMENT_COUNT = 1_000_000;
    private const int ITERATIONS = 3;
    private const int WARMUP = 3;

    private void Start()
    {
        RunBenchmark();
    }

    private void RunBenchmark()
    {
        Debug.Log("========================================");
        Debug.Log(" ByteWriter / ByteReader STRING Benchmark");
        Debug.Log("========================================");

        // ------------------------------------------------------------
        // Prepare deterministic source data
        // ------------------------------------------------------------

        string[] source = new string[ELEMENT_COUNT];

        for (int i = 0; i < source.Length; i++)
        {
            source[i] = GenerateString(i);
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
            using (var writer = Serialize(source))
            {
                DeserializeAndValidate(writer.AsSpan(), source);
            }
        }

        // ------------------------------------------------------------
        // WRITE BENCHMARK
        // ------------------------------------------------------------

        long totalWriteTicks = 0;
        long totalWriteAllocations = 0;
        int serializedSize = 0;

        Debug.Log("Starting benchmark...");

        var sw = new System.Diagnostics.Stopwatch();

        for (int iteration = 0; iteration < ITERATIONS; iteration++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long allocatedBefore =
                GC.GetAllocatedBytesForCurrentThread();

            sw.Restart();

            using (var writer = Serialize(source))
            {
                sw.Stop();

                totalWriteTicks += sw.ElapsedTicks;

                serializedSize = writer.Position;
            }

            long allocatedAfter =
                GC.GetAllocatedBytesForCurrentThread();

            totalWriteAllocations +=
                allocatedAfter - allocatedBefore;
        }

        double writeMilliseconds =
            TicksToMilliseconds(totalWriteTicks) / ITERATIONS;

        double writeAllocatedBytes =
            totalWriteAllocations / (double)ITERATIONS;

        // ------------------------------------------------------------
        // Create one writer buffer for READ benchmark
        // ------------------------------------------------------------

        using (var benchmarkWriter = Serialize(source))
        {
            ReadOnlySpan<byte> benchmarkData =
                benchmarkWriter.AsSpan();

            serializedSize = benchmarkData.Length;

            // --------------------------------------------------------
            // READ BENCHMARK
            // --------------------------------------------------------

            long totalReadTicks = 0;
            long totalReadAllocations = 0;

            for (int iteration = 0; iteration < ITERATIONS; iteration++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                long allocatedBefore =
                    GC.GetAllocatedBytesForCurrentThread();

                sw.Restart();

                DeserializeAndValidate(
                    benchmarkData,
                    source);

                sw.Stop();

                totalReadTicks += sw.ElapsedTicks;

                long allocatedAfter =
                    GC.GetAllocatedBytesForCurrentThread();

                totalReadAllocations +=
                    allocatedAfter - allocatedBefore;
            }

            double readMilliseconds =
                TicksToMilliseconds(totalReadTicks) / ITERATIONS;

            double readAllocatedBytes =
                totalReadAllocations / (double)ITERATIONS;

            // --------------------------------------------------------
            // Results
            // --------------------------------------------------------

            double megabytes =
                serializedSize / (1024.0 * 1024.0);

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
            Debug.Log(
                $"Write allocated : {FormatBytes(writeAllocatedBytes)} / run");

            Debug.Log("");

            Debug.Log($"Read time       : {readMilliseconds:F4} ms");
            Debug.Log($"Read speed      : {readMBps:F2} MB/s");
            Debug.Log(
                $"Read allocated  : {FormatBytes(readAllocatedBytes)} / run");

            Debug.Log("========================================");
        }
    }

    // ================================================================
    // SERIALIZE
    // ================================================================

    private ByteWriter Serialize(string[] values)
    {
        // Rough starting capacity.
        // The writer can grow if required.
        var writer = new ByteWriter(values.Length * 32 + 4);

        writer.Write(values.Length);

        for (int i = 0; i < values.Length; i++)
        {
            writer.Write(values[i]);
        }

        return writer;
    }

    // ================================================================
    // DESERIALIZE + VALIDATE
    // ================================================================

    private void DeserializeAndValidate(
        ReadOnlySpan<byte> data,
        string[] expected)
    {
        var reader = new ByteReader(data);

        reader.Read(out int count);

        if (count != expected.Length)
        {
            throw new Exception(
                $"COUNT MISMATCH! Expected {expected.Length}, got {count}");
        }

        for (int i = 0; i < count; i++)
        {
            reader.Read(out string actual);

            string expectedValue = expected[i];

            if (actual != expectedValue)
            {
                throw new Exception(
                    $"DATA CORRUPTION at index {i}!\n" +
                    $"Expected: {expectedValue}\n" +
                    $"Actual:   {actual}");
            }
        }

        if (reader.Remaining != 0)
        {
            throw new Exception(
                $"Unread bytes remaining: {reader.Remaining}");
        }
    }

    // ================================================================
    // DETERMINISTIC TEST STRINGS
    // ================================================================

    private string GenerateString(int index)
    {
        switch (index % 8)
        {
            case 0:
                return "Sword";

            case 1:
                return "Health Potion";

            case 2:
                return "Very Long Inventory Item Name";

            case 3:
                return "ABC123456789";

            case 4:
                return "The quick brown fox jumps over the lazy dog";

            case 5:
                return "中文测试";

            case 6:
                return "éèêë";

            default:
                return $"Item_{index}";
        }
    }

    // ================================================================
    // HELPERS
    // ================================================================

    private double TicksToMilliseconds(long ticks)
    {
        return ticks * 1000.0 /
               System.Diagnostics.Stopwatch.Frequency;
    }

    private string FormatBytes(double bytes)
    {
        if (bytes < 1024)
            return $"{bytes:F0} B";

        if (bytes < 1024 * 1024)
            return $"{bytes / 1024.0:F2} KB";

        if (bytes < 1024 * 1024 * 1024)
            return $"{bytes / (1024.0 * 1024.0):F2} MB";

        return $"{bytes / (1024.0 * 1024.0 * 1024.0):F2} GB";
    }
}