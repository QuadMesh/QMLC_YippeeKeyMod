using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using YippeeKey.ConfigSync;
using YippeeKey.LocalScripts;

namespace YippeeKey.Patches
{
    [HarmonyPatch]
    public sealed class NetworkObjectManagerYK
    {

        public static Dictionary<string, YippeeSoundManager> soundManagers = new Dictionary<string, YippeeSoundManager>();

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

        [HarmonyPostfix, HarmonyPatch(typeof(GameNetworkManager), "StartDisconnect")]
        static void UnsubscribeFromHandler()
        {
            YippeeKeyPlugin.Instance.Log("UnSyncing config");
            soundManagers.Clear();
            YippeeSyncedConfig.RevertSync();

        }

        public static void SendYippeeEventToServer(string eventName, bool isCallerDead)
        {
            YippeeKeyPlugin.Instance.Log("Sending event to rest of lobby.");
            NetworkHandlerYP.Instance.ScreamYippeeServerRPC(eventName, isCallerDead);
        }

        public static void playYippeeAtPlayer(ref string playerName, ref bool isCallerDead)
        {
            //Very hacky, but it works for the most part.
            if (!soundManagers.ContainsKey(playerName))
            {
                YippeeKeyPlugin.Instance.LogError($"Unable to get sound manager for {playerName}!");
                return;
            }

            //Dead players can't yippee to alive players?
            if (!YippeeSyncedConfig.Instance.DeadPlayersYippeeAlivePlayers.Value
                && !GameNetworkManager.Instance.localPlayerController.isPlayerDead)
                return;

            GameObject playerToPlayAt = soundManagers[playerName].gameObject;
            //Save transform for use
            Transform playerTransform = playerToPlayAt.transform;
            YippeeKeyPlugin.Instance.Log($"{playerName}'s transform gotten from dictionary.");
            //Play yippee noise
            soundManagers[playerName].Play(isCallerDead);
            //Notify enemies check
            if (!YippeeSyncedConfig.Instance.NotifyEnemies.Value)
            {
                YippeeKeyPlugin.Instance.Log("Host-synced config dictates enemies are not alerted.");
                return;
            }

            if (isCallerDead && !YippeeSyncedConfig.Instance.DeadPlayerAlertsEnemyAI.Value || !YippeeSyncedConfig.Instance.DeadPlayersYippeeAlivePlayers.Value)
            {
                YippeeKeyPlugin.Instance.Log("Host-synced config dictates Dead players cannot alert enemies.");
                return;
            }
            
            YippeeKeyPlugin.Instance.Log("Host-synced config dictates enemies are alerted.");
            RoundManager.Instance.PlayAudibleNoise(playerTransform.position, YippeeSyncedConfig.Instance.EnemyAIDetectionRange.Value, YippeeSyncedConfig.Instance.EnemyAIDetectionVolume.Value);
        }

        static GameObject? networkPrefab = null;
    }
}
