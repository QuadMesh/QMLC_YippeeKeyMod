﻿using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using YippeeKey.LocalScripts;

namespace YippeeKey.Patches
{
    [HarmonyPatch]
    public class NetworkObjectManagerYK
    {

        [HarmonyPostfix, HarmonyPatch(typeof(GameNetworkManager), "Start")]
        public static void Init()
        {
            if (networkPrefab != null)
            {
                YippeeKeyPlugin.Instance.Log("Networkmanager already exist");
            }

            //I don't know what went wrong here, bah... it'll work... :clueless:
            YippeeKeyPlugin.Instance.Log("Getting network prefab setup");
            networkPrefab = (GameObject)YippeeKeyPlugin.Instance.MainAssetBundle.LoadAsset("NetworkHandlerYippeeKey");
            YippeeKeyPlugin.Instance.Log("Adding NetworkHandlerYP");
            networkPrefab.AddComponent<NetworkHandlerYP>();

            YippeeKeyPlugin.Instance.Log("Adding Networked prefab");
            NetworkManager.Singleton.AddNetworkPrefab(networkPrefab);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(StartOfRound), "Awake")]
        static void SpawnNetworkHandler()
        {
            YippeeKeyPlugin.Instance.Log("Spawning NetworkHandler");
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                YippeeKeyPlugin.Instance.Log("NetworkHandler spawned!");
                YippeeKeyPlugin.Instance.Log($"Is networkPrefab existant? {(networkPrefab != null ? "Yes!" : "Nope")}");
                var networkHandlerHost = UnityEngine.Object.Instantiate(networkPrefab, Vector3.zero, Quaternion.identity);
                YippeeKeyPlugin.Instance.Log($"Is NetworkHandler existant? {(networkHandlerHost != null ? "Yes!" : "Nope")}");
                networkHandlerHost.GetComponent<NetworkObject>()?.Spawn();
            }
            //NetworkHandlerYP.ScreamYippeeEvent += ReceivedEventFromServer;
        }

        /*[HarmonyPostfix, HarmonyPatch(typeof(GameNetworkManager), "StartDisconnect")]
        static void UnsubscribeFromHandler()
        {
            YippeeKeyPlugin.Instance.Log("Destroying network prefab");
            networkPrefab = null;
        }*/

        public static void SendEventToServer(string eventName)
        {
            YippeeKeyPlugin.Instance.Log("Sending event to rest of lobby.");
            NetworkHandlerYP.Instance.ScreamYippeeServerRPC(eventName);
        }

        public static void playYippeeAtPlayer(ref string playerName)
        {
            //Very hacky, but it works for the most part.

            //Save transform for use
            Transform playerTransform = GameObject.Find(playerName).transform;
            YippeeKeyPlugin.Instance.Log($"{playerName}'s transform captured.");

            //Get the yippeesound manager from the player, can be null.
            YippeeSoundManager yippeeSound = GameObject.Find(playerName).GetComponentInChildren<YippeeSoundManager>();
            YippeeKeyPlugin.Instance.Log($"Attempted to get {playerName}'s Yippesound, Successful? {(yippeeSound != null ? "Yes!" : "Nope")}");
            //Does the player not have a yippee controller? Add it.
            if (yippeeSound == null)
            {
                YippeeKeyPlugin.Instance.Log("Yippee object didn't exist. creating.");
                //Load prefab from assetbundle
                GameObject yippee = (GameObject)YippeeKeyPlugin.Instance.MainAssetBundle.LoadAsset("YippeSound");
                YippeeKeyPlugin.Instance.Log($"Prefab for use, loaded!");
                GameObject yippeeSoundObject = GameObject.Instantiate(yippee, new Vector3(playerTransform.position.x, playerTransform.position.y + 2f, playerTransform.position.z), playerTransform.rotation, playerTransform);
                YippeeKeyPlugin.Instance.Log($"{yippeeSoundObject.gameObject.name} Instantiated for use, loaded!");
                yippeeSound = yippee.AddComponent<YippeeSoundManager>();
                YippeeKeyPlugin.Instance.Log($"{yippeeSoundObject.name} component added, ready!");
            }

            YippeeKeyPlugin.Instance.Log("Playing Yippe");
            //Play yippee noise
            yippeeSound?.Play();
            YippeeKeyPlugin.Instance.Log("Host could have alerted enemies.");

            //Notify round that there has been a funny noise over here. ONLY IF SERVER.
            if (GameNetworkManager.Instance.localPlayerController.IsServer)
            {
                //If the host of the game does not notify enemies, don't notify. Host's game, Host's rules.
                if (!YippeeKeyPlugin.Instance.NotifyEnemies.Value)
                {
                    YippeeKeyPlugin.Instance.Log("Locally disabled notifying enemies.");
                    return;
                }
                YippeeKeyPlugin.Instance.Log("Yippeed in the world, let's see where this goes...");
                RoundManager.Instance.PlayAudibleNoise(playerTransform.position);
            }
        }

        static GameObject? networkPrefab = null;
    }
}