using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;

namespace NewHorizons.Utility.DebugTools
{
    public class MultiButtonScreenPrompt : ScreenPrompt
    {
        private Sprite customSprite2;

        public MultiButtonScreenPrompt(string prompt, Sprite customSprite, Sprite customSprite2) : base(prompt, customSprite)
        {
            this.customSprite2 = customSprite2;
            this._multiCommandType = MultiCommandType.CUSTOM_BOTH;
        }

        public List<Sprite> GetSpriteList()
        {
            return new List<Sprite> { this._customSprite, this.customSprite2 };
        }
    }

    [HarmonyPatch]
    public static class MultiButtonScreenPromptPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ScreenPrompt), "GetSpriteList")]
        public static void GetSpriteList_Postfix(ScreenPrompt __instance, ref List<Sprite> __result)
        {
            if (__instance is MultiButtonScreenPrompt multiButtonScreenPrompt)
            {
                __result = multiButtonScreenPrompt.GetSpriteList();
            }
        }
    }
}