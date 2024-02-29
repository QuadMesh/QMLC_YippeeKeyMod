using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using YippeeKey.LocalScripts;

namespace YippeeKey.Patches
{
    /// <summary>
    /// This class patches all the dead bodies to supply a SoundManager where needed.
    /// Removes them when not needed anymore.
    /// </summary>
    [HarmonyPatch]
    internal class DeadBodyInfoPatch
    {
        [HarmonyPrefix, HarmonyPatch(typeof(DeadBodyInfo), nameof(DeadBodyInfo.DeactivateBody))]
        static void ReparentOnRespawn(DeadBodyInfo __instance)
        {
            foreach (YippeeSoundManager yippeeSound in NetworkObjectManagerYK.soundManagers.Values)
            {
                if (yippeeSound.Player.isPlayerDead)
                {
                    //We don't need a reference check here, if the controller doesn't exist... Uhh, not my problem. /shrug
                    YippeeKeyPlugin.Instance.Log($"Reparenting soundmanager to normal body");
                    NetworkObjectManagerYK.soundManagers[__instance.playerScript.gameObject.name].transform.SetParent(__instance.playerScript.transform);
                    YippeeKeyPlugin.Instance.Log($"Reparented {__instance.gameObject.name}");
                }
            }
        }
    }
}
