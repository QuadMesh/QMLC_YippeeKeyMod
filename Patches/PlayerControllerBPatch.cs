using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using YippeeKey.ConfigSync;
using YippeeKey.LocalScripts;

namespace YippeeKey.Patches
{
    [HarmonyPatch]
    internal class PlayerControllerBPatch
    {
        /// <summary>
        /// This is mainly done so all players have a yippe input script, will most likely be rewritten to be just the local player in the future.
        /// </summary>
        /// <param name="__instance"></param>
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
