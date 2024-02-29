using BepInEx;
using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using YippeeKey.ConfigSync;
using YippeeKey.Patches;

namespace YippeeKey.LocalScripts
{
    public class YippeeInputter : MonoBehaviour
    {
        //Reference to localPlayer
        private PlayerControllerB? _localPlayer;

        //Local cooldown
        private bool cooldownActive = false;

        //Local cooldown time
        private float cooldown = YippeeSyncedConfig.Instance.CooldownTime.Value;

        //CooldownTime
        float CoolDownStartTime => YippeeSyncedConfig.Instance.CooldownTime.Value;

        private void Awake()
        {
            _localPlayer = GetComponent<PlayerControllerB>();
        }

        //Guard clauses incoming!!
        public void Update()
        {
            ManageCooldown();
            
            //If both cooldown is enabled AND the cooldown is active
            if (YippeeSyncedConfig.Instance.CooldownEnabled.Value && cooldownActive) return;
            //Input
            if (!UnityInput.Current.GetKeyDown(YippeeSyncedConfig.Default.ConfigKey.Value)) return;
            //Is local (very important)
            if (!IsLocal()) return;
            //Is in terminal
            if (GameNetworkManager.Instance.localPlayerController.inTerminalMenu) return;
            //Is in menu
            if (GameNetworkManager.Instance.localPlayerController.quickMenuManager.isMenuOpen) return;
            //Is tuping in chat
            if (GameNetworkManager.Instance.localPlayerController.isTypingChat) return;

            //Finally, run the event.
            NetworkObjectManagerYK.SendYippeeEventToServer(GameNetworkManager.Instance.localPlayerController.gameObject.name, GameNetworkManager.Instance.localPlayerController.isPlayerDead);
            //Restart cooldown.
            if (YippeeSyncedConfig.Instance.CooldownEnabled.Value)
            {
                //Reset the cooldown.
                cooldownActive = true;
                YippeeKeyPlugin.Instance.Log("Cooldown active");
            }
        }

        private void ManageCooldown()
        {
            //If the cooldown is active
            if (cooldownActive)
            {
                //Count down the timer
                cooldown -= Time.deltaTime;
                YippeeKeyPlugin.Instance.Log($"{cooldown}");
                if (cooldown <= 0)
                {
                    //Reset timer, wait for input.
                    cooldownActive = false;
                    cooldown = CoolDownStartTime;
                    YippeeKeyPlugin.Instance.Log("Cooldown has wore off, time to Yippee!");
                }
                return;
            }
        }

        /// <summary>
        /// Return whether the player this component is attached to is the true local player.
        /// </summary>
        /// <returns>
        /// True: Local attached object is local player<br></br>
        /// False: Local attached object is Remote player
        /// </returns>
        private bool IsLocal() => _localPlayer.Equals(GameNetworkManager.Instance.localPlayerController);
    }
}
