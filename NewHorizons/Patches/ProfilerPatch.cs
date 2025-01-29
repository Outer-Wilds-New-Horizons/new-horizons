// #define ENABLE_PROFILER

using HarmonyLib;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine.Profiling;

namespace NewHorizons.Patches;

/// <summary>
/// attach profiler markers to important methods
/// </summary>
[HarmonyPatch]
public static class ProfilerPatch
{
	[HarmonyTargetMethods]
	public static IEnumerable<MethodBase> TargetMethods()
	{
		foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
		{
			if (!type.Name.EndsWith("Builder")) continue;

			foreach (var method in type.GetRuntimeMethods())
			{
				if (!(method.Name.StartsWith("Make") || method.Name.StartsWith("Init"))) continue;
				if (method.IsGenericMethod) continue;

				Main.Instance.ModHelper.Console.WriteLine($"[profiler] profiling method {method.DeclaringType.Name}.{method.Name}");
				yield return method;
			}
		}
	}

	[HarmonyPrefix]
	public static void Prefix(MethodBase __originalMethod, out Stopwatch __state)
	{
		Profiler.BeginSample($"{__originalMethod.DeclaringType.Name}.{__originalMethod.Name}");

		__state = new Stopwatch();
		__state.Start();
	}

	[HarmonyPostfix]
	public static void Postfix(MethodBase __originalMethod, Stopwatch __state)
	{
		Profiler.EndSample();

		__state.Stop();
		Main.Instance.ModHelper.Console.WriteLine($"[profiler] method {__originalMethod.DeclaringType.Name}.{__originalMethod.Name} took {__state.Elapsed.TotalMilliseconds} ms");
	}
}
