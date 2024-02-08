using BepInEx;
using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using YippeeKey.Patches;

namespace YippeeKey.LocalScripts
{
    public class YippeeInputter : MonoBehaviour
    {

        private PlayerControllerB _localPlayer;

        private void Awake()
        {
            _localPlayer = GetComponent<PlayerControllerB>();
        }

        //Guard clauses incoming!!
        public void Update()
        {
            //Input
            if (!UnityInput.Current.GetKeyDown(YippeeKeyPlugin.Instance.ConfigKey.Value)) return;
            //Is local (very important)
            if (!IsLocal()) return;
            //Is alive
            if (GameNetworkManager.Instance.localPlayerController.isPlayerDead) return;
            //Is in terminal
            if (GameNetworkManager.Instance.localPlayerController.inTerminalMenu) return;
            //Is in menu
            if (GameNetworkManager.Instance.localPlayerController.quickMenuManager.isMenuOpen) return;

            //Finally, run the event.
            NetworkObjectManagerYK.SendEventToServer(GameNetworkManager.Instance.localPlayerController.gameObject.name);
        }

        /// <summary>
        /// Return whether the player this component is attached to is the true local player.
        /// </summary>
        /// <returns>
        /// True: Local attached object is local player<br></br>
        /// False: Local attached object is Remote player
        /// </returns>
        private bool IsLocal()
        {
            return _localPlayer.Equals(GameNetworkManager.Instance.localPlayerController);
        }
    }
}
