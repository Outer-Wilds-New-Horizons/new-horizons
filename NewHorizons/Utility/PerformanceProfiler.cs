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
    /// Performance profiling utility for measuring and tracking load time improvements
    /// </summary>
    public static class PerformanceProfiler
    {
        private static readonly Dictionary<string, List<long>> _performanceResults = new Dictionary<string, List<long>>();
        private static readonly Dictionary<string, Stopwatch> _activeTimers = new Dictionary<string, Stopwatch>();
        private static readonly string ProfileDataPath = Path.Combine(Application.persistentDataPath, "NH_PerformanceProfile.json");
        
        /// <summary>
        /// Start timing an operation
        /// </summary>
        public static void StartTimer(string operationName)
        {
            if (_activeTimers.ContainsKey(operationName))
            {
                NHLogger.LogWarning($"Timer '{operationName}' was already running. Restarting.");
                _activeTimers[operationName].Restart();
            }
            else
            {
                _activeTimers[operationName] = Stopwatch.StartNew();
            }
        }
        
        /// <summary>
        /// Stop timing an operation and record the result
        /// </summary>
        public static long StopTimer(string operationName)
        {
            if (!_activeTimers.TryGetValue(operationName, out var stopwatch))
            {
                NHLogger.LogError($"Timer '{operationName}' was not started.");
                return 0;
            }
            
            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;
            
            if (!_performanceResults.ContainsKey(operationName))
            {
                _performanceResults[operationName] = new List<long>();
            }
            
            _performanceResults[operationName].Add(elapsedMs);
            _activeTimers.Remove(operationName);
            
            NHLogger.LogVerbose($"Operation '{operationName}' completed in {elapsedMs}ms");
            return elapsedMs;
        }
        
        /// <summary>
        /// Check if a timer is currently active
        /// </summary>
        public static bool IsTimerActive(string operationName)
        {
            return _activeTimers.ContainsKey(operationName);
        }
        
        /// <summary>
        /// Time a specific operation with automatic start/stop
        /// </summary>
        public static T TimeOperation<T>(string operationName, Func<T> operation)
        {
            StartTimer(operationName);
            try
            {
                return operation();
            }
            finally
            {
                StopTimer(operationName);
            }
        }
        
        /// <summary>
        /// Time a specific operation with automatic start/stop (void return)
        /// </summary>
        public static void TimeOperation(string operationName, Action operation)
        {
            StartTimer(operationName);
            try
            {
                operation();
            }
            finally
            {
                StopTimer(operationName);
            }
        }
        
        /// <summary>
        /// Get performance statistics for an operation
        /// </summary>
        public static PerformanceStats GetStats(string operationName)
        {
            if (!_performanceResults.TryGetValue(operationName, out var results) || results.Count == 0)
            {
                return new PerformanceStats();
            }
            
            return new PerformanceStats
            {
                OperationName = operationName,
                SampleCount = results.Count,
                AverageMs = results.Average(),
                MinMs = results.Min(),
                MaxMs = results.Max(),
                TotalMs = results.Sum(),
                MedianMs = GetMedian(results)
            };
        }
        
        /// <summary>
        /// Generate a performance report comparing before/after optimizations
        /// </summary>
        public static void GeneratePerformanceReport()
        {
            var report = new PerformanceReport
            {
                Timestamp = DateTime.UtcNow,
                Operations = _performanceResults.Keys.Select(GetStats).ToList()
            };
            
            // Log summary to console
            NHLogger.Log("=== PERFORMANCE PROFILE REPORT ===");
            foreach (var stat in report.Operations.Where(s => s.SampleCount > 0))
            {
                NHLogger.Log($"{stat.OperationName}: {stat.AverageMs:F1}ms avg ({stat.SampleCount} samples)");
            }
            
            // Save detailed report to disk
            try
            {
                var json = JsonConvert.SerializeObject(report, Formatting.Indented);
                File.WriteAllText(ProfileDataPath, json);
                NHLogger.Log($"Detailed performance report saved to: {ProfileDataPath}");
            }
            catch (Exception ex)
            {
                NHLogger.LogError($"Failed to save performance report: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Load previous performance data for comparison
        /// </summary>
        public static PerformanceReport LoadPreviousReport()
        {
            try
            {
                if (File.Exists(ProfileDataPath))
                {
                    var json = File.ReadAllText(ProfileDataPath);
                    return JsonConvert.DeserializeObject<PerformanceReport>(json);
                }
            }
            catch (Exception ex)
            {
                NHLogger.LogError($"Failed to load previous performance report: {ex.Message}");
            }
            
            return null;
        }
        
        /// <summary>
        /// Compare current performance with previous baseline
        /// </summary>
        public static void CompareWithBaseline()
        {
            var previousReport = LoadPreviousReport();
            if (previousReport == null)
            {
                NHLogger.Log("No baseline performance data found. Current run will serve as baseline.");
                return;
            }
            
            NHLogger.Log("=== PERFORMANCE COMPARISON ===");
            
            foreach (var currentStat in _performanceResults.Keys.Select(GetStats).Where(s => s.SampleCount > 0))
            {
                var previousStat = previousReport.Operations.FirstOrDefault(s => s.OperationName == currentStat.OperationName);
                if (previousStat != null && previousStat.SampleCount > 0)
                {
                    var improvement = ((previousStat.AverageMs - currentStat.AverageMs) / previousStat.AverageMs) * 100;
                    var changeDirection = improvement > 0 ? "FASTER" : "SLOWER";
                    var changeAmount = Math.Abs(improvement);
                    
                    NHLogger.Log($"{currentStat.OperationName}: {currentStat.AverageMs:F1}ms vs {previousStat.AverageMs:F1}ms " +
                                $"({changeAmount:F1}% {changeDirection})");
                }
            }
        }
        
        /// <summary>
        /// Clear all performance data
        /// </summary>
        public static void ClearData()
        {
            _performanceResults.Clear();
            _activeTimers.Clear();
            NHLogger.LogVerbose("Performance profiler data cleared");
        }
        
        private static double GetMedian(List<long> values)
        {
            var sorted = values.OrderBy(x => x).ToList();
            int count = sorted.Count;
            
            if (count % 2 == 0)
            {
                return (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
            }
            else
            {
                return sorted[count / 2];
            }
        }
    }
    
    /// <summary>
    /// Performance statistics for a specific operation
    /// </summary>
    public class PerformanceStats
    {
        public string OperationName { get; set; } = "";
        public int SampleCount { get; set; }
        public double AverageMs { get; set; }
        public long MinMs { get; set; }
        public long MaxMs { get; set; }
        public long TotalMs { get; set; }
        public double MedianMs { get; set; }
    }
    
    /// <summary>
    /// Complete performance report for serialization
    /// </summary>
    public class PerformanceReport
    {
        public DateTime Timestamp { get; set; }
        public List<PerformanceStats> Operations { get; set; } = new List<PerformanceStats>();
        public string Version { get; set; } = "1.0";
        public string ModVersion { get; set; } = Main.Instance?.ModHelper?.Manifest?.Version ?? "Unknown";
    }
}