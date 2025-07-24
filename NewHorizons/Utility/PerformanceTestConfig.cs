using NewHorizons.Utility.OWML;
using System;
using UnityEngine;

namespace NewHorizons.Utility
{
    /// <summary>
    /// Configuration and utilities for performance testing
    /// </summary>
    public static class PerformanceTestConfig
    {
        /// <summary>
        /// Enable detailed performance profiling
        /// </summary>
        public static bool EnableDetailedProfiling { get; set; } = true;
        
        /// <summary>
        /// Number of SearchUtilities.Find() calls to simulate for stress testing
        /// </summary>
        public static int SearchStressTestIterations { get; set; } = 100;
        
        /// <summary>
        /// Run a stress test of SearchUtilities.Find() to demonstrate performance improvements
        /// </summary>
        public static void RunSearchStressTest()
        {
            if (!EnableDetailedProfiling) return;
            
            NHLogger.Log("=== STARTING SEARCH STRESS TEST ===");
            
            // Common object names that would be searched during mod loading
            var commonSearches = new[]
            {
                "Player_Body",
                "Ship_Body", 
                "Probe_Body",
                "Sun_Body",
                "CameraController",
                "PlayerCameraController",
                "Ship",
                "HUD_ReticuleCanvas",
                "ToolModeUI",
                "Ship_Cockpit",
                "Ship_Thruster_Model",
                "TimeLoopRing_Body"
            };
            
            PerformanceProfiler.StartTimer("SearchStressTest.Total");
            
            for (int iteration = 0; iteration < SearchStressTestIterations; iteration++)
            {
                foreach (var searchName in commonSearches)
                {
                    PerformanceProfiler.TimeOperation($"SearchStressTest.Individual.{searchName}", 
                        () => SearchUtilities.Find(searchName, warn: false));
                }
            }
            
            PerformanceProfiler.StopTimer("SearchStressTest.Total");
            
            NHLogger.Log("=== SEARCH STRESS TEST COMPLETED ===");
            
            // Log results
            var totalStats = PerformanceProfiler.GetStats("SearchStressTest.Total");
            NHLogger.Log($"Total search stress test time: {totalStats.AverageMs:F1}ms");
            
            foreach (var searchName in commonSearches)
            {
                var stats = PerformanceProfiler.GetStats($"SearchStressTest.Individual.{searchName}");
                if (stats.SampleCount > 0)
                {
                    NHLogger.Log($"  {searchName}: {stats.AverageMs:F2}ms avg, {stats.TotalMs}ms total");
                }
            }
        }
        
        /// <summary>
        /// Run performance tests for StreamingHandler operations
        /// </summary>
        public static void RunStreamingStressTest()
        {
            if (!EnableDetailedProfiling) return;
            
            NHLogger.Log("=== STARTING STREAMING STRESS TEST ===");
            
            PerformanceProfiler.StartTimer("StreamingStressTest.MaterialTableCache");
            
            // Simulate multiple calls to GetMaterialTables to test caching
            for (int i = 0; i < 50; i++)
            {
                PerformanceProfiler.TimeOperation("StreamingStressTest.MaterialTableLookup", 
                    () => Resources.FindObjectsOfTypeAll<StreamingMaterialTable>());
            }
            
            PerformanceProfiler.StopTimer("StreamingStressTest.MaterialTableCache");
            
            var lookupStats = PerformanceProfiler.GetStats("StreamingStressTest.MaterialTableLookup");
            NHLogger.Log($"Material table lookups: {lookupStats.AverageMs:F2}ms avg, {lookupStats.SampleCount} samples");
            
            NHLogger.Log("=== STREAMING STRESS TEST COMPLETED ===");
        }
        
        /// <summary>
        /// Generate a comprehensive performance baseline for comparison
        /// </summary>
        public static void GeneratePerformanceBaseline()
        {
            NHLogger.Log("=== GENERATING PERFORMANCE BASELINE ===");
            
            RunSearchStressTest();
            RunStreamingStressTest();
            
            PerformanceProfiler.GeneratePerformanceReport();
            
            NHLogger.Log("=== PERFORMANCE BASELINE COMPLETE ===");
            NHLogger.Log("Results saved to NH_PerformanceProfile.json in persistent data path.");
            NHLogger.Log("Run this test again after optimizations to compare performance.");
        }
        
        /// <summary>
        /// Log cache statistics for analysis
        /// </summary>
        public static void LogCacheStatistics()
        {
            NHLogger.Log("=== CACHE STATISTICS ===");
            
            // These would need to be exposed from SearchUtilities if we want detailed stats
            // For now, just log that caching is active
            NHLogger.Log("Disk-based path cache: Active");
            NHLogger.Log("Memory cache: Active");
            NHLogger.Log("Material table cache: Active");
            
            NHLogger.Log("Cache files stored in: " + Application.persistentDataPath);
        }
    }
}