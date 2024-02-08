using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using YippeeKey.LocalScripts;

namespace YippeeKey.Patches
{
    [HarmonyPatch]
    internal class PlayerControllerBPatch
    {
        [HarmonyPostfix, HarmonyPatch(typeof(PlayerControllerB), "OnEnable")]
        static void AddInputter(PlayerControllerB __instance)
        {
            YippeeInputter yippee;
            __instance.gameObject.TryGetComponent<YippeeInputter>(out yippee);
            if (yippee == null)
            {
                YippeeKeyPlugin.Instance.Log("YippeInputter added!");
                __instance.gameObject.AddComponent<YippeeInputter>();
            }
        }
    }
}
