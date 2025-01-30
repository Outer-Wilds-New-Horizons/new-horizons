#if ENABLE_PROFILER

using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
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
			// if (!type.Name.EndsWith("Builder")) continue;
			// if (!(type.FullName.Contains("Builder") || type.FullName.Contains("Utility"))) continue;

			foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
			{
				if (!(
						method.Name.StartsWith("Make") ||
						method.Name.StartsWith("Init") ||
						method.Name.StartsWith("Find") ||
						method.Name == "SetUpStreaming" ||
						method.Name == "OnSceneLoaded"
					)) continue;

				if (method.ContainsGenericParameters) continue;

				Main.Instance.ModHelper.Console.WriteLine($"[profiler] profiling {method.FriendlyName()}");
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

#endif
