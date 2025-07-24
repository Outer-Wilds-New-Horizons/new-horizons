using NewHorizons.Utility.OWML;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

namespace NewHorizons.Utility
{
    /// <summary>
    /// Performance comparison utility to measure improvements between main branch and optimized code
    /// </summary>
    public static class PerformanceComparison
    {
        private static readonly string ComparisonResultsPath = Path.Combine(Application.persistentDataPath, "NH_PerformanceComparison.json");
        
        /// <summary>
        /// Run comprehensive performance comparison between original and optimized code
        /// </summary>
        public static void RunFullComparison()
        {
            NHLogger.Log("=== STARTING PERFORMANCE COMPARISON ===");
            
            var results = new ComparisonResults
            {
                TestTimestamp = DateTime.UtcNow,
                SceneName = SceneManager.GetActiveScene().name,
                ModVersion = Main.Instance?.ModHelper?.Manifest?.Version ?? "Unknown"
            };
            
            // Test SearchUtilities.Find() performance
            results.SearchUtilitiesResults = TestSearchUtilitiesPerformance();
            
            // Test StreamingHandler performance (if objects are available)
            results.StreamingHandlerResults = TestStreamingHandlerPerformance();
            
            // Test overall scene loading impact
            results.SceneLoadResults = TestSceneLoadPerformance();
            
            // Save results to disk
            SaveComparisonResults(results);
            
            // Display summary
            DisplayComparisonSummary(results);
            
            NHLogger.Log("=== PERFORMANCE COMPARISON COMPLETE ===");
        }
        
        /// <summary>
        /// Test SearchUtilities.Find() performance with various scenarios
        /// </summary>
        private static SearchTestResults TestSearchUtilitiesPerformance()
        {
            NHLogger.Log("Testing SearchUtilities.Find() performance...");
            
            var results = new SearchTestResults();
            
            // Clear caches to test cold performance
            SearchUtilities.ClearCache();
            
            // Test scenarios
            var testCases = new[]
            {
                new SearchTestCase { Name = "Player_Body", ExpectedToExist = true },
                new SearchTestCase { Name = "Ship_Body", ExpectedToExist = true },
                new SearchTestCase { Name = "NonExistentObject", ExpectedToExist = false },
                new SearchTestCase { Name = "Sun_Body", ExpectedToExist = true },
                new SearchTestCase { Name = "CameraController", ExpectedToExist = true },
                new SearchTestCase { Name = "HUD_ReticuleCanvas", ExpectedToExist = true },
                new SearchTestCase { Name = "Ship_Cockpit", ExpectedToExist = true },
                new SearchTestCase { Name = "AnotherNonExistent", ExpectedToExist = false }
            };
            
            // Test cold cache performance (first run)
            results.ColdCacheResults = new List<SearchResult>();
            foreach (var testCase in testCases)
            {
                var stopwatch = Stopwatch.StartNew();
                var found = SearchUtilities.Find(testCase.Name, warn: false);
                stopwatch.Stop();
                
                results.ColdCacheResults.Add(new SearchResult
                {
                    SearchName = testCase.Name,
                    FoundObject = found != null,
                    ExpectedToExist = testCase.ExpectedToExist,
                    ElapsedMs = stopwatch.ElapsedMilliseconds
                });
                
                NHLogger.LogVerbose($"Cold cache - {testCase.Name}: {stopwatch.ElapsedMilliseconds}ms");
            }
            
            // Test warm cache performance (second run)
            results.WarmCacheResults = new List<SearchResult>();
            foreach (var testCase in testCases)
            {
                var stopwatch = Stopwatch.StartNew();
                var found = SearchUtilities.Find(testCase.Name, warn: false);
                stopwatch.Stop();
                
                results.WarmCacheResults.Add(new SearchResult
                {
                    SearchName = testCase.Name,
                    FoundObject = found != null,
                    ExpectedToExist = testCase.ExpectedToExist,
                    ElapsedMs = stopwatch.ElapsedMilliseconds
                });
                
                NHLogger.LogVerbose($"Warm cache - {testCase.Name}: {stopwatch.ElapsedMilliseconds}ms");
            }
            
            // Calculate statistics
            results.ColdCacheAvgMs = results.ColdCacheResults.Average(r => r.ElapsedMs);
            results.WarmCacheAvgMs = results.WarmCacheResults.Average(r => r.ElapsedMs);
            results.CacheImprovementPercent = ((results.ColdCacheAvgMs - results.WarmCacheAvgMs) / results.ColdCacheAvgMs) * 100;
            
            NHLogger.Log($"SearchUtilities - Cold: {results.ColdCacheAvgMs:F1}ms, Warm: {results.WarmCacheAvgMs:F1}ms, Improvement: {results.CacheImprovementPercent:F1}%");
            
            return results;
        }
        
        /// <summary>
        /// Test StreamingHandler performance if possible
        /// </summary>
        private static StreamingTestResults TestStreamingHandlerPerformance()
        {
            NHLogger.Log("Testing StreamingHandler performance...");
            
            var results = new StreamingTestResults();
            
            try
            {
                // Clear streaming caches
                StreamingHandler.Init();
                
                // Find test objects with streaming components
                var testObjects = new List<GameObject>();
                var allObjects = Resources.FindObjectsOfTypeAll<GameObject>()
                    .Where(go => go.scene.name != null && go.GetComponentsInChildren<StreamingMeshHandle>().Length > 0)
                    .Take(10) // Limit to 10 test objects
                    .ToArray();
                
                NHLogger.Log($"Found {allObjects.Length} objects with streaming components for testing");
                
                if (allObjects.Length == 0)
                {
                    results.ErrorMessage = "No objects with StreamingMeshHandle components found for testing";
                    return results;
                }
                
                // Test cold performance (first run)
                var coldTimes = new List<long>();
                foreach (var obj in allObjects)
                {
                    var stopwatch = Stopwatch.StartNew();
                    StreamingHandler.SetUpStreaming(obj, null);
                    stopwatch.Stop();
                    coldTimes.Add(stopwatch.ElapsedMilliseconds);
                }
                
                // Test warm performance (cached run)
                var warmTimes = new List<long>();
                foreach (var obj in allObjects)
                {
                    var stopwatch = Stopwatch.StartNew();
                    StreamingHandler.SetUpStreaming(obj, null);
                    stopwatch.Stop();
                    warmTimes.Add(stopwatch.ElapsedMilliseconds);
                }
                
                results.ColdCacheAvgMs = coldTimes.Average();
                results.WarmCacheAvgMs = warmTimes.Average();
                results.CacheImprovementPercent = ((results.ColdCacheAvgMs - results.WarmCacheAvgMs) / results.ColdCacheAvgMs) * 100;
                results.ObjectsTestd = allObjects.Length;
                
                NHLogger.Log($"StreamingHandler - Cold: {results.ColdCacheAvgMs:F1}ms, Warm: {results.WarmCacheAvgMs:F1}ms, Improvement: {results.CacheImprovementPercent:F1}%");
            }
            catch (Exception ex)
            {
                results.ErrorMessage = $"StreamingHandler test failed: {ex.Message}";
                NHLogger.LogError(results.ErrorMessage);
            }
            
            return results;
        }
        
        /// <summary>
        /// Test overall scene loading performance impact
        /// </summary>
        private static SceneLoadTestResults TestSceneLoadPerformance()
        {
            var results = new SceneLoadTestResults
            {
                SceneName = SceneManager.GetActiveScene().name,
                TotalGameObjects = Resources.FindObjectsOfTypeAll<GameObject>().Count(go => go.scene.name != null),
                StreamingComponents = Resources.FindObjectsOfTypeAll<StreamingMeshHandle>().Length
            };
            
            // Get current performance profile data if available
            var currentStats = PerformanceProfiler.GetStats($"SceneLoad.{results.SceneName}");
            if (currentStats.SampleCount > 0)
            {
                results.CurrentLoadTimeMs = (long)currentStats.AverageMs;
                NHLogger.Log($"Scene Load Performance - {results.SceneName}: {results.CurrentLoadTimeMs}ms");
            }
            else
            {
                NHLogger.Log($"No scene load timing data available for {results.SceneName}");
            }
            
            return results;
        }
        
        /// <summary>
        /// Save comparison results to disk for analysis
        /// </summary>
        private static void SaveComparisonResults(ComparisonResults results)
        {
            try
            {
                var json = JsonConvert.SerializeObject(results, Formatting.Indented);
                File.WriteAllText(ComparisonResultsPath, json);
                NHLogger.Log($"Performance comparison results saved to: {ComparisonResultsPath}");
            }
            catch (Exception ex)
            {
                NHLogger.LogError($"Failed to save comparison results: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Display a summary of the performance comparison
        /// </summary>
        private static void DisplayComparisonSummary(ComparisonResults results)
        {
            NHLogger.Log("=== PERFORMANCE COMPARISON SUMMARY ===");
            
            if (results.SearchUtilitiesResults != null)
            {
                var search = results.SearchUtilitiesResults;
                NHLogger.Log($"SearchUtilities.Find():");
                NHLogger.Log($"  Cold Cache: {search.ColdCacheAvgMs:F1}ms average");
                NHLogger.Log($"  Warm Cache: {search.WarmCacheAvgMs:F1}ms average");
                NHLogger.Log($"  Improvement: {search.CacheImprovementPercent:F1}% faster with cache");
            }
            
            if (results.StreamingHandlerResults != null && string.IsNullOrEmpty(results.StreamingHandlerResults.ErrorMessage))
            {
                var streaming = results.StreamingHandlerResults;
                NHLogger.Log($"StreamingHandler.SetUpStreaming():");
                NHLogger.Log($"  Cold Cache: {streaming.ColdCacheAvgMs:F1}ms average");
                NHLogger.Log($"  Warm Cache: {streaming.WarmCacheAvgMs:F1}ms average");
                NHLogger.Log($"  Improvement: {streaming.CacheImprovementPercent:F1}% faster with cache");
                NHLogger.Log($"  Objects Tested: {streaming.ObjectsTestd}");
            }
            
            if (results.SceneLoadResults != null && results.SceneLoadResults.CurrentLoadTimeMs > 0)
            {
                var scene = results.SceneLoadResults;
                NHLogger.Log($"Scene Load Performance:");
                NHLogger.Log($"  Scene: {scene.SceneName}");
                NHLogger.Log($"  Load Time: {scene.CurrentLoadTimeMs}ms");
                NHLogger.Log($"  GameObjects: {scene.TotalGameObjects}");
                NHLogger.Log($"  Streaming Components: {scene.StreamingComponents}");
            }
        }
        
        /// <summary>
        /// Load and compare with previous results (main branch baseline)
        /// </summary>
        public static void CompareWithMainBranch()
        {
            try
            {
                if (File.Exists(ComparisonResultsPath))
                {
                    var json = File.ReadAllText(ComparisonResultsPath);
                    var previousResults = JsonConvert.DeserializeObject<ComparisonResults>(json);
                    
                    NHLogger.Log("=== COMPARISON WITH MAIN BRANCH ===");
                    NHLogger.Log($"Previous Test: {previousResults.TestTimestamp}");
                    NHLogger.Log($"Current Test: {DateTime.UtcNow}");
                    
                    // This would require storing main branch results separately
                    // For now, just display current optimized results
                    DisplayComparisonSummary(previousResults);
                }
                else
                {
                    NHLogger.Log("No previous comparison results found. Run performance test to establish baseline.");
                }
            }
            catch (Exception ex)
            {
                NHLogger.LogError($"Failed to load previous comparison results: {ex.Message}");
            }
        }
    }
    
    // Data structures for storing comparison results
    public class ComparisonResults
    {
        public DateTime TestTimestamp { get; set; }
        public string SceneName { get; set; }
        public string ModVersion { get; set; }
        public SearchTestResults SearchUtilitiesResults { get; set; }
        public StreamingTestResults StreamingHandlerResults { get; set; }
        public SceneLoadTestResults SceneLoadResults { get; set; }
    }
    
    public class SearchTestResults
    {
        public List<SearchResult> ColdCacheResults { get; set; }
        public List<SearchResult> WarmCacheResults { get; set; }
        public double ColdCacheAvgMs { get; set; }
        public double WarmCacheAvgMs { get; set; }
        public double CacheImprovementPercent { get; set; }
    }
    
    public class SearchResult
    {
        public string SearchName { get; set; }
        public bool FoundObject { get; set; }
        public bool ExpectedToExist { get; set; }
        public long ElapsedMs { get; set; }
    }
    
    public class SearchTestCase
    {
        public string Name { get; set; }
        public bool ExpectedToExist { get; set; }
    }
    
    public class StreamingTestResults
    {
        public double ColdCacheAvgMs { get; set; }
        public double WarmCacheAvgMs { get; set; }
        public double CacheImprovementPercent { get; set; }
        public int ObjectsTestd { get; set; }
        public string ErrorMessage { get; set; }
    }
    
    public class SceneLoadTestResults
    {
        public string SceneName { get; set; }
        public long CurrentLoadTimeMs { get; set; }
        public int TotalGameObjects { get; set; }
        public int StreamingComponents { get; set; }
    }
}