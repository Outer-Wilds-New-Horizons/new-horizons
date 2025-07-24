using NewHorizons.Handlers;
using NewHorizons.Utility.OWML;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

namespace NewHorizons.Utility
{
    /// <summary>
    /// Comprehensive performance benchmark comparing original vs optimized implementations
    /// </summary>
    public static class PerformanceBenchmark
    {
        private static readonly string BenchmarkResultsPath = Path.Combine(Application.persistentDataPath, "NH_BenchmarkResults.json");
        
        /// <summary>
        /// Run complete benchmark comparing original vs optimized code
        /// </summary>
        public static void RunCompleteBenchmark()
        {
            NHLogger.Log("=== STARTING PERFORMANCE BENCHMARK ===");
            NHLogger.Log("Comparing Original (main branch) vs Optimized (current) implementations");
            
            var results = new BenchmarkResults
            {
                TestTimestamp = DateTime.UtcNow,
                ModVersion = Main.Instance?.ModHelper?.Manifest?.Version ?? "Unknown",
                TestIterations = 50 // Number of iterations for reliable averages
            };
            
            // Warm up Unity systems
            WarmUpSystems();
            
            // Benchmark SearchUtilities.Find()
            results.SearchBenchmark = BenchmarkSearchUtilities(results.TestIterations);
            
            // Benchmark StreamingHandler.SetUpStreaming()
            results.StreamingBenchmark = BenchmarkStreamingHandler(results.TestIterations);
            
            // Save and display results
            SaveBenchmarkResults(results);
            DisplayBenchmarkResults(results);
            
            NHLogger.Log("=== PERFORMANCE BENCHMARK COMPLETE ===");
        }
        
        /// <summary>
        /// Warm up Unity systems to ensure fair comparison
        /// </summary>
        private static void WarmUpSystems()
        {
            NHLogger.Log("Warming up systems...");
            
            // Warm up Resources.FindObjectsOfTypeAll
            Resources.FindObjectsOfTypeAll<GameObject>().Take(10).ToArray();
            
            // Warm up scene graph traversal
            foreach (var rootGO in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().Take(5))
            {
                rootGO.GetComponentsInChildren<Transform>();
            }
            
            NHLogger.Log("System warm-up complete");
        }
        
        /// <summary>
        /// Benchmark SearchUtilities.Find() - Original vs Optimized
        /// </summary>
        private static SearchBenchmarkResults BenchmarkSearchUtilities(int iterations)
        {
            NHLogger.Log($"Benchmarking SearchUtilities.Find() with {iterations} iterations...");
            
            var results = new SearchBenchmarkResults();
            
            // Test cases covering different scenarios
            var testCases = new[]
            {
                "Player_Body",
                "Ship_Body", 
                "Sun_Body",
                "CameraController",
                "HUD_ReticuleCanvas",
                "Ship_Cockpit",
                "NonExistentObject1", // Test expensive fallback
                "NonExistentObject2"  // Test expensive fallback
            };
            
            // Test Original Implementation
            NHLogger.Log("Testing original implementation...");
            SearchUtilitiesOriginal.ClearCache();
            
            var originalTimes = new List<long>();
            var originalStopwatch = Stopwatch.StartNew();
            
            for (int i = 0; i < iterations; i++)
            {
                foreach (var testCase in testCases)
                {
                    var sw = Stopwatch.StartNew();
                    SearchUtilitiesOriginal.Find(testCase, warn: false);
                    sw.Stop();
                    originalTimes.Add(sw.ElapsedMilliseconds);
                }
            }
            originalStopwatch.Stop();
            
            results.OriginalTotalMs = originalStopwatch.ElapsedMilliseconds;
            results.OriginalAverageMs = originalTimes.Average();
            results.OriginalMaxMs = originalTimes.Max();
            results.OriginalMinMs = originalTimes.Min();
            
            // Test Optimized Implementation
            NHLogger.Log("Testing optimized implementation...");
            SearchUtilities.ClearCache();
            
            var optimizedTimes = new List<long>();
            var optimizedStopwatch = Stopwatch.StartNew();
            
            for (int i = 0; i < iterations; i++)
            {
                foreach (var testCase in testCases)
                {
                    var sw = Stopwatch.StartNew();
                    SearchUtilities.Find(testCase, warn: false);
                    sw.Stop();
                    optimizedTimes.Add(sw.ElapsedMilliseconds);
                }
            }
            optimizedStopwatch.Stop();
            
            results.OptimizedTotalMs = optimizedStopwatch.ElapsedMilliseconds;
            results.OptimizedAverageMs = optimizedTimes.Average();
            results.OptimizedMaxMs = optimizedTimes.Max();
            results.OptimizedMinMs = optimizedTimes.Min();
            
            // Calculate improvement
            results.ImprovementPercent = ((results.OriginalAverageMs - results.OptimizedAverageMs) / results.OriginalAverageMs) * 100;
            results.SpeedupFactor = results.OriginalAverageMs / results.OptimizedAverageMs;
            
            NHLogger.Log($"SearchUtilities - Original: {results.OriginalAverageMs:F2}ms, Optimized: {results.OptimizedAverageMs:F2}ms, " +
                        $"Improvement: {results.ImprovementPercent:F1}%, Speedup: {results.SpeedupFactor:F1}x");
            
            return results;
        }
        
        /// <summary>
        /// Benchmark StreamingHandler.SetUpStreaming() - Original vs Optimized
        /// </summary>
        private static StreamingBenchmarkResults BenchmarkStreamingHandler(int iterations)
        {
            NHLogger.Log($"Benchmarking StreamingHandler.SetUpStreaming() with {iterations} iterations...");
            
            var results = new StreamingBenchmarkResults();
            
            // Find objects with streaming components for testing
            var testObjects = Resources.FindObjectsOfTypeAll<GameObject>()
                .Where(go => go.scene.name != null && go.GetComponentsInChildren<StreamingMeshHandle>().Length > 0)
                .Take(10)
                .ToArray();
            
            NHLogger.Log($"Found {testObjects.Length} objects with streaming components for testing");
            
            if (testObjects.Length == 0)
            {
                results.ErrorMessage = "No objects with StreamingMeshHandle components found for testing";
                return results;
            }
            
            results.ObjectsTestedCount = testObjects.Length;
            
            // Test Original Implementation
            NHLogger.Log("Testing original streaming implementation...");
            StreamingHandlerOriginal.Init();
            
            var originalTimes = new List<long>();
            var originalStopwatch = Stopwatch.StartNew();
            
            for (int i = 0; i < iterations; i++)
            {
                foreach (var obj in testObjects)
                {
                    var sw = Stopwatch.StartNew();
                    StreamingHandlerOriginal.SetUpStreaming(obj, null);
                    sw.Stop();
                    originalTimes.Add(sw.ElapsedMilliseconds);
                }
            }
            originalStopwatch.Stop();
            
            results.OriginalTotalMs = originalStopwatch.ElapsedMilliseconds;
            results.OriginalAverageMs = originalTimes.Average();
            results.OriginalMaxMs = originalTimes.Max();
            results.OriginalMinMs = originalTimes.Min();
            
            // Test Optimized Implementation
            NHLogger.Log("Testing optimized streaming implementation...");
            StreamingHandler.Init();
            
            var optimizedTimes = new List<long>();
            var optimizedStopwatch = Stopwatch.StartNew();
            
            for (int i = 0; i < iterations; i++)
            {
                foreach (var obj in testObjects)
                {
                    var sw = Stopwatch.StartNew();
                    StreamingHandler.SetUpStreaming(obj, null);
                    sw.Stop();
                    optimizedTimes.Add(sw.ElapsedMilliseconds);
                }
            }
            optimizedStopwatch.Stop();
            
            results.OptimizedTotalMs = optimizedStopwatch.ElapsedMilliseconds;
            results.OptimizedAverageMs = optimizedTimes.Average();
            results.OptimizedMaxMs = optimizedTimes.Max();
            results.OptimizedMinMs = optimizedTimes.Min();
            
            // Calculate improvement
            results.ImprovementPercent = ((results.OriginalAverageMs - results.OptimizedAverageMs) / results.OriginalAverageMs) * 100;
            results.SpeedupFactor = results.OriginalAverageMs / results.OptimizedAverageMs;
            
            NHLogger.Log($"StreamingHandler - Original: {results.OriginalAverageMs:F2}ms, Optimized: {results.OptimizedAverageMs:F2}ms, " +
                        $"Improvement: {results.ImprovementPercent:F1}%, Speedup: {results.SpeedupFactor:F1}x");
            
            return results;
        }
        
        /// <summary>
        /// Save benchmark results to disk for analysis
        /// </summary>
        private static void SaveBenchmarkResults(BenchmarkResults results)
        {
            try
            {
                var json = JsonConvert.SerializeObject(results, Formatting.Indented);
                File.WriteAllText(BenchmarkResultsPath, json);
                NHLogger.Log($"Benchmark results saved to: {BenchmarkResultsPath}");
            }
            catch (Exception ex)
            {
                NHLogger.LogError($"Failed to save benchmark results: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Display comprehensive benchmark results
        /// </summary>
        private static void DisplayBenchmarkResults(BenchmarkResults results)
        {
            NHLogger.Log("=== BENCHMARK RESULTS SUMMARY ===");
            
            if (results.SearchBenchmark != null)
            {
                var search = results.SearchBenchmark;
                NHLogger.Log("SearchUtilities.Find() Performance:");
                NHLogger.Log($"  Original Implementation:");
                NHLogger.Log($"    Total: {search.OriginalTotalMs}ms");
                NHLogger.Log($"    Average: {search.OriginalAverageMs:F2}ms");
                NHLogger.Log($"    Min/Max: {search.OriginalMinMs}ms / {search.OriginalMaxMs}ms");
                NHLogger.Log($"  Optimized Implementation:");
                NHLogger.Log($"    Total: {search.OptimizedTotalMs}ms");
                NHLogger.Log($"    Average: {search.OptimizedAverageMs:F2}ms");
                NHLogger.Log($"    Min/Max: {search.OptimizedMinMs}ms / {search.OptimizedMaxMs}ms");
                NHLogger.Log($"  Performance Improvement:");
                NHLogger.Log($"    Speedup: {search.SpeedupFactor:F1}x faster");
                NHLogger.Log($"    Improvement: {search.ImprovementPercent:F1}% reduction in time");
            }
            
            if (results.StreamingBenchmark != null && string.IsNullOrEmpty(results.StreamingBenchmark.ErrorMessage))
            {
                var streaming = results.StreamingBenchmark;
                NHLogger.Log("StreamingHandler.SetUpStreaming() Performance:");
                NHLogger.Log($"  Objects Tested: {streaming.ObjectsTestedCount}");
                NHLogger.Log($"  Original Implementation:");
                NHLogger.Log($"    Total: {streaming.OriginalTotalMs}ms");
                NHLogger.Log($"    Average: {streaming.OriginalAverageMs:F2}ms");
                NHLogger.Log($"    Min/Max: {streaming.OriginalMinMs}ms / {streaming.OriginalMaxMs}ms");
                NHLogger.Log($"  Optimized Implementation:");
                NHLogger.Log($"    Total: {streaming.OptimizedTotalMs}ms");
                NHLogger.Log($"    Average: {streaming.OptimizedAverageMs:F2}ms");
                NHLogger.Log($"    Min/Max: {streaming.OptimizedMinMs}ms / {streaming.OptimizedMaxMs}ms");
                NHLogger.Log($"  Performance Improvement:");
                NHLogger.Log($"    Speedup: {streaming.SpeedupFactor:F1}x faster");
                NHLogger.Log($"    Improvement: {streaming.ImprovementPercent:F1}% reduction in time");
            }
            
            // Calculate combined improvement
            if (results.SearchBenchmark != null && results.StreamingBenchmark != null && 
                string.IsNullOrEmpty(results.StreamingBenchmark.ErrorMessage))
            {
                var combinedOriginal = results.SearchBenchmark.OriginalTotalMs + results.StreamingBenchmark.OriginalTotalMs;
                var combinedOptimized = results.SearchBenchmark.OptimizedTotalMs + results.StreamingBenchmark.OptimizedTotalMs;
                var combinedImprovement = ((combinedOriginal - combinedOptimized) / (double)combinedOriginal) * 100;
                var combinedSpeedup = combinedOriginal / (double)combinedOptimized;
                
                NHLogger.Log("Combined Performance Impact:");
                NHLogger.Log($"  Total Original Time: {combinedOriginal}ms");
                NHLogger.Log($"  Total Optimized Time: {combinedOptimized}ms");
                NHLogger.Log($"  Combined Improvement: {combinedImprovement:F1}% reduction");
                NHLogger.Log($"  Combined Speedup: {combinedSpeedup:F1}x faster");
            }
        }
    }
    
    // Data structures for benchmark results
    public class BenchmarkResults
    {
        public DateTime TestTimestamp { get; set; }
        public string ModVersion { get; set; }
        public int TestIterations { get; set; }
        public SearchBenchmarkResults SearchBenchmark { get; set; }
        public StreamingBenchmarkResults StreamingBenchmark { get; set; }
    }
    
    public class SearchBenchmarkResults
    {
        public long OriginalTotalMs { get; set; }
        public double OriginalAverageMs { get; set; }
        public long OriginalMaxMs { get; set; }
        public long OriginalMinMs { get; set; }
        
        public long OptimizedTotalMs { get; set; }
        public double OptimizedAverageMs { get; set; }
        public long OptimizedMaxMs { get; set; }
        public long OptimizedMinMs { get; set; }
        
        public double ImprovementPercent { get; set; }
        public double SpeedupFactor { get; set; }
    }
    
    public class StreamingBenchmarkResults
    {
        public int ObjectsTestedCount { get; set; }
        public string ErrorMessage { get; set; }
        
        public long OriginalTotalMs { get; set; }
        public double OriginalAverageMs { get; set; }
        public long OriginalMaxMs { get; set; }
        public long OriginalMinMs { get; set; }
        
        public long OptimizedTotalMs { get; set; }
        public double OptimizedAverageMs { get; set; }
        public long OptimizedMaxMs { get; set; }
        public long OptimizedMinMs { get; set; }
        
        public double ImprovementPercent { get; set; }
        public double SpeedupFactor { get; set; }
    }
}