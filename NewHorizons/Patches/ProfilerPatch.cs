#if ENABLE_PROFILER

using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Profiling;

namespace NewHorizons.Patches;

/// <summary>
/// attach profiler markers to important methods
/// </summary>
[HarmonyPatch]
public static class ProfilerPatch
{
	private static string FriendlyName(this MethodBase @this) => $"{@this.DeclaringType.Name}.{@this.Name}";

	[HarmonyTargetMethods]
	public static IEnumerable<MethodBase> TargetMethods()
	{
		foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
		{
			if (!(
					type.Name == "Main" ||
					type.Name.EndsWith("Builder") ||
					type.Name.EndsWith("Handler") ||
					type.Name.EndsWith("Utilities")
				)) continue;

			foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
			{
				if (method.ContainsGenericParameters) continue;

				// Main.Instance.ModHelper.Console.WriteLine($"[profiler] profiling {method.FriendlyName()}");
				yield return method;
			}
		}
	}

	[HarmonyPrefix]
	public static void Prefix(MethodBase __originalMethod /*, out Stopwatch __state*/)
	{
		Profiler.BeginSample(__originalMethod.FriendlyName());

		// __state = new Stopwatch();
		// __state.Start();
	}

	[HarmonyPostfix]
	public static void Postfix( /*MethodBase __originalMethod, Stopwatch __state*/)
	{
		Profiler.EndSample();

		// __state.Stop();
		// Main.Instance.ModHelper.Console.WriteLine($"[profiler] {__originalMethod.MethodName()} took {__state.Elapsed.TotalMilliseconds:f1} ms");
	}
}

/// <summary>
/// bundle loading causes log spam that slows loading, but only in unity dev profiler mode.
/// patch it out so it doesnt do false-positive slowness.
/// </summary>
[HarmonyPatch]
public static class DisableShaderLogSpamPatch
{
	[HarmonyPrefix]
	[HarmonyPatch(typeof(StackTraceUtility), "ExtractStackTrace")]
	[HarmonyPatch(typeof(Application), "CallLogCallback")]
	private static bool DisableShaderLogSpam() => false;
}

#endif
