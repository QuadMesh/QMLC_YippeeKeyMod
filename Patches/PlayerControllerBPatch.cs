﻿using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Json;
using System.Text;
using UnityEngine;
using YippeeKey.ConfigSync;
using YippeeKey.LocalScripts;

namespace YippeeKey.Patches
{
    [HarmonyPatch]
    internal sealed class PlayerControllerBPatch
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
            YippeeKeyPlugin.Instance.Log("Setting SoundManager");
            //Load prefab from assetbundle
            AddSoundManagerToPlayer(__instance);
        }

        public static void AddSoundManagerToPlayer(PlayerControllerB playerController)
        {
            //If the player died, but the body was destroyed without getting the sound manager to set dead body
            //DO NOT CREATE A 2nd YIPPEESOUNDMANAGER!!!!!
            if (playerController.gameObject.GetComponentInChildren<YippeeSoundManager>())
            {
                YippeeKeyPlugin.Instance.Log("You already have a soundManager, there is no need to add another.");
                return;
            }

            //Get the prefab from the assetbundle
            GameObject yippeePrefab = (GameObject)YippeeKeyPlugin.Instance.MainAssetBundle.LoadAsset("YippeSound");
            YippeeKeyPlugin.Instance.Log($"Prefab for use, loaded!");
            //Instantiate the yippesound
            GameObject yippeeSoundObject = UnityEngine.Object.Instantiate(yippeePrefab, new Vector3(playerController.gameObject.transform.position.x, playerController.gameObject.transform.position.y + 2f, playerController.gameObject.transform.position.z), playerController.gameObject.transform.rotation, playerController.gameObject.transform);
            YippeeSoundManager yippeeSoundreference = yippeeSoundObject.AddComponent<YippeeSoundManager>();
            yippeeSoundreference.Player = playerController;
            NetworkObjectManagerYK.soundManagers.Add(playerController.gameObject.name, yippeeSoundreference);
            YippeeKeyPlugin.Instance.Log($"Soundmanager set for {playerController.gameObject.name}");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.SpawnDeadBody))]
        static void ReparentOnDeath(PlayerControllerB __instance)
        {

            //Due to the fact that dead bodies are not the same as normal players, we have to do this.
            //Could be that the deadbody has to exist in order to work properly.
            if (__instance.deadBody != null && __instance.deadBody.enabled)
            {
                YippeeKeyPlugin.Instance.Log($"Reparenting soundmanager to dead body");
                try
                {
                    NetworkObjectManagerYK.soundManagers[__instance.gameObject.name].transform.SetParent(__instance.deadBody.transform);
                    YippeeKeyPlugin.Instance.Log($"Reparented {__instance.gameObject.name}");
                }
                catch
                {
                    YippeeKeyPlugin.Instance.LogError("Unable to set the YippeeSoundManager to the dead body, most likey Nullref.");
                }
            }
        }
    }
}
