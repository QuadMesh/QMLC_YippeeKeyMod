using BepInEx;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Unity.Netcode;
using YippeeKey.Patches;

namespace YippeeKey
{
    public class NetworkHandlerYP : NetworkBehaviour
    {

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

        [ServerRpc(RequireOwnership =false)]
        public void ScreamYippeeServerRPC(string eventName)
        {
            YippeeKeyPlugin.Instance.Log("Server event received");
            NetworkObjectManagerYK.playYippeeAtPlayer(ref eventName);
            YippeeKeyPlugin.Instance.Log("Notifying Clients");
            ScreamYippeeClientRPC(eventName);
        }

        [ClientRpc]
        public void ScreamYippeeClientRPC(string eventName)
        {
            YippeeKeyPlugin.Instance.Log($"Event received from Server for {eventName}");
            NetworkObjectManagerYK.playYippeeAtPlayer(ref eventName);
        }

        public static NetworkHandlerYP Instance { get; private set; }

    }
}
