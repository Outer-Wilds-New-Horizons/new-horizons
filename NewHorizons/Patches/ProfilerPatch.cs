using HarmonyLib;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

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
			// only allow builders for now
			if (!type.Name.EndsWith("Builder")) continue;

			foreach (var method in type.GetMethods())
			{
				// make and init methods
				if (!(method.Name.StartsWith("Make") || method.Name.StartsWith("Init"))) continue;

				Main.Instance.ModHelper.Console.WriteLine($"[profiler] profiling method {method.DeclaringType.Name}.{method.Name}");
				yield return method;
			}
		}
	}

	[HarmonyPrefix]
	public static void Prefix(out Stopwatch __state)
	{
		__state = new Stopwatch();
		__state.Start();
	}

	[HarmonyPostfix]
	public static void Postfix(MethodBase __originalMethod, Stopwatch __state)
	{
		__state.Stop();
		Main.Instance.ModHelper.Console.WriteLine($"[profiler] method {__originalMethod.DeclaringType.Name}.{__originalMethod.Name} took {__state.Elapsed.TotalMilliseconds} ms");
	}
}
