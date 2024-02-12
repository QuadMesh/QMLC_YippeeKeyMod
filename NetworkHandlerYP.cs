using BepInEx;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Unity.Netcode;
using UnityEngine.UIElements;
using YippeeKey.Patches;

namespace YippeeKey
{
    public sealed class NetworkHandlerYP : NetworkBehaviour
    {
        /// <summary>
        /// When the network object spawns, this procedure is called to make networking possible
        /// </summary>
        public override void OnNetworkSpawn()
        {
            YippeeKeyPlugin.Instance.Log("Despawning NetworkObject");
            YippeeKeyPlugin.Instance.Log($"NetworkManager existant? {(NetworkManager.Singleton != null ? "Yes!" : "No")}");
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                try
                {
                    NetworkObject? networkObject = this.gameObject.GetComponent<NetworkObject>();
                    YippeeKeyPlugin.Instance.Log($"networkObject existant? {(networkObject != null ? "Yes!" : "No")}");
                    networkObject?.Spawn();
                }
                catch (Exception ex)
                {
                    YippeeKeyPlugin.Instance.Log("networkObject was already spawned.");
                }
            }
            Instance = this;
            
            base.OnNetworkSpawn();
        }

        //On sent to server: Sent to the rest of the lobby.
        [ServerRpc(RequireOwnership=false)]
        public void ScreamYippeeServerRPC(string eventName, bool isCallerDead)
        {
            YippeeKeyPlugin.Instance.Log("Server event received");
            //NetworkObjectManagerYK.playYippeeAtPlayer(ref eventName, ref isCallerDead);
            YippeeKeyPlugin.Instance.Log("Notifying Clients");
            ScreamYippeeClientRPC(eventName, isCallerDead);
        }

        //On received: play yippe
        [ClientRpc]
        public void ScreamYippeeClientRPC(string eventName, bool isCallerDead)
        {
                YippeeKeyPlugin.Instance.Log($"Event received from Server for {eventName}");
                NetworkObjectManagerYK.playYippeeAtPlayer(ref eventName, ref isCallerDead);
        }

        public static NetworkHandlerYP Instance { get; private set; }

    }
}
