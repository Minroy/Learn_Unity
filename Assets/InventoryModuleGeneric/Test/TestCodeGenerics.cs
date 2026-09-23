using System.Diagnostics;
using UnityEngine;

namespace InventoryModule.Packer
{
    public class RandomStringLargeWorkingSetBenchmark : MonoBehaviour
    {
        [Header("Benchmark")]
        [SerializeField] private int warmupMilliseconds = 1000;
        [SerializeField] private int measurementMilliseconds = 3000;

        [Header("Dataset")]
        [SerializeField] private int stringsPerSize = 16384;

        [Header("String Sizes")]
        [SerializeField]
        private int[] stringLengths =
        {
            8,
            32,
            128,
            1024,
            4096,
            16384,
            65536
        };

        [Header("Random")]
        [SerializeField] private int randomSeed = 12345;

        private string[][] _datasets;
        private int[][] _indices;

        private ByteWriter _writer;

        private void Start()
        {
            GenerateDataset();
            RunBenchmark();
        }

        // ================================================================
        // DATASET
        // ================================================================

        private void GenerateDataset()
        {
            System.Random random = new System.Random(randomSeed);

            _datasets = new string[stringLengths.Length][];
            _indices = new int[stringLengths.Length][];

            UnityEngine.Debug.Log(
                "Generating large random-string benchmark dataset...");

            long totalBytes = 0;

            for (int sizeIndex = 0; sizeIndex < stringLengths.Length; sizeIndex++)
            {
                int length = stringLengths[sizeIndex];

                string[] strings = new string[stringsPerSize];

                for (int i = 0; i < stringsPerSize; i++)
                {
                    strings[i] = GenerateRandomAsciiString(
                        random,
                        length);

                    totalBytes += strings[i].Length * sizeof(char);
                }

                _datasets[sizeIndex] = strings;

                // Create shuffled access order.
                int[] indices = new int[stringsPerSize];

                for (int i = 0; i < indices.Length; i++)
                    indices[i] = i;

                Shuffle(indices, random);

                _indices[sizeIndex] = indices;

                double gb = totalBytes /
                            (1024.0 * 1024.0 * 1024.0);

                UnityEngine.Debug.Log(
                    $"Generated {length:N0} chars | " +
                    $"Dataset: {GetDatasetSizeGB(strings):F3} GB");
            }

            UnityEngine.Debug.Log(
                $"TOTAL SOURCE DATASET: " +
                $"{totalBytes / (1024.0 * 1024.0 * 1024.0):F3} GB");
        }

        private static string GenerateRandomAsciiString(
            System.Random random,
            int length)
        {
            char[] chars = new char[length];

            for (int i = 0; i < chars.Length; i++)
            {
                // Printable ASCII.
                chars[i] = (char)random.Next(32, 127);
            }

            return new string(chars);
        }

        private static void Shuffle(
            int[] values,
            System.Random random)
        {
            for (int i = values.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);

                int temp = values[i];
                values[i] = values[j];
                values[j] = temp;
            }
        }

        private static double GetDatasetSizeGB(string[] strings)
        {
            long bytes = 0;

            for (int i = 0; i < strings.Length; i++)
                bytes += strings[i].Length * sizeof(char);

            return bytes /
                   (1024.0 * 1024.0 * 1024.0);
        }

        // ================================================================
        // BENCHMARK
        // ================================================================

        private void RunBenchmark()
        {
            UnityEngine.Debug.Log(
                "==============================================");

            UnityEngine.Debug.Log(
                " RANDOM STRING LARGE WORKING SET BENCHMARK");

            UnityEngine.Debug.Log(
                "==============================================");

            UnityEngine.Debug.Log(
                $"Strings per size: {stringsPerSize:N0}");

            UnityEngine.Debug.Log(
                $"Warmup: {warmupMilliseconds:N0} ms");

            UnityEngine.Debug.Log(
                $"Measurement: {measurementMilliseconds:N0} ms");

            UnityEngine.Debug.Log(
                "==============================================");

            for (int i = 0; i < stringLengths.Length; i++)
            {
                RunWriteBenchmark(
                    stringLengths[i],
                    _datasets[i],
                    _indices[i]);
            }
        }

        // ================================================================
        // WRITE
        // ================================================================

        private void RunWriteBenchmark(
            int stringLength,
            string[] strings,
            int[] indices)
        {
            int payloadSize =
                sizeof(int) +
                stringLength * sizeof(char);

            /*
             * Give the writer enough room so that the benchmark
             * does NOT repeatedly trigger EnsureCapacity().
             *
             * We are measuring string serialization, not buffer growth.
             */
            int writerCapacity = payloadSize * 4;

            if (writerCapacity < 1024)
                writerCapacity = 1024;

            _writer = new ByteWriter(writerCapacity);

            // ------------------------------------------------------------
            // Warmup
            // ------------------------------------------------------------

            Stopwatch warmupTimer = Stopwatch.StartNew();

            int warmupIndex = 0;

            while (warmupTimer.ElapsedMilliseconds < warmupMilliseconds)
            {
                string value = strings[indices[warmupIndex]];

                _writer.Reset();
                _writer.Write(value);

                warmupIndex++;

                if (warmupIndex == indices.Length)
                    warmupIndex = 0;
            }

            warmupTimer.Stop();

            // ------------------------------------------------------------
            // Measurement
            // ------------------------------------------------------------

            long operations = 0;
            long totalBytes = 0;

            int index = 0;

            Stopwatch timer = Stopwatch.StartNew();

            while (timer.ElapsedMilliseconds < measurementMilliseconds)
            {
                string value = strings[indices[index]];

                _writer.Reset();

                _writer.Write(value);

                /*
                 * Consume the result so the benchmark always observes
                 * the actual writer state.
                 */
                int written = _writer.Position;

                totalBytes += written;
                operations++;

                index++;

                if (index == indices.Length)
                    index = 0;
            }

            timer.Stop();

            // ------------------------------------------------------------
            // Results
            // ------------------------------------------------------------

            double seconds =
                timer.Elapsed.TotalSeconds;

            double totalMB =
                totalBytes /
                (1024.0 * 1024.0);

            double throughputMB =
                totalMB / seconds;

            double throughputGB =
                throughputMB / 1024.0;

            double nsPerOperation =
                timer.Elapsed.TotalMilliseconds *
                1_000_000.0 /
                operations;

            UnityEngine.Debug.Log(
                $"STRING {stringLength:N0} chars\n" +
                $"  Payload:       {payloadSize:N0} B\n" +
                $"  Dataset:       {GetDatasetSizeGB(strings):F3} GB\n" +
                $"  Operations:    {operations:N0}\n" +
                $"  Time:          {seconds:F3} s\n" +
                $"  Total bytes:   {totalMB:N2} MB\n" +
                $"  Throughput:    {throughputMB:N2} MB/s\n" +
                $"  Throughput:    {throughputGB:N2} GB/s\n" +
                $"  Time/op:       {nsPerOperation:N2} ns\n" +
                $"  Final writer:  {_writer.Position:N0} B");

            _writer = null;
        }
    }
}