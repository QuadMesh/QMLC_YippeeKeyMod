using BepInEx;
using BepInEx.Configuration;
using CSync.Lib;
using CSync.Util;
using GameNetcodeStuff;
using HarmonyLib;
using LethalLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.UIElements.UIR.Allocator2D;

namespace YippeeKey.ConfigSync
{
    [DataContract]
    public class YippeeSyncedConfig : SyncedInstance<YippeeSyncedConfig>
    {
        public ConfigEntry<float> DisplayDebugInfo { get; private set; }

        [DataMember] public SyncedEntry<bool> NotifyEnemies { get; private set; }
        [DataMember] public SyncedEntry<int> EnemyAIDetectionRange { get; private set; }
        [DataMember] public SyncedEntry<float> EnemyAIDetectionVolume { get; private set; }
        [DataMember] public SyncedEntry<bool> CooldownEnabled { get; private set; }
        [DataMember] public SyncedEntry<float> CooldownTime { get; private set; }
        //Allow dead players to 'yippee!'
        [DataMember] public SyncedEntry<bool> DeadPlayersYippeeAlivePlayers { get; private set; }
        //Dead players trigger EnemyAI
        [DataMember] public SyncedEntry<bool> DeadPlayerAlertsEnemyAI { get; private set; }

        //Keycode, might be replaced with inpututils
        public ConfigEntry<KeyCode> ConfigKey { get; private set; }
        //Debug config, usable for debugging, not for public use.
        public ConfigEntry<bool> DebugKey { get; private set; }
        //Key to disable visuals of the mod (Particles)
        public ConfigEntry<bool> AllowVisuals { get; private set; }
        //Allows particles to be spammed (pre 1.1.O)
        public ConfigEntry<bool> AllowParticeSpam { get; private set; }
        //Allows the Yippees to overlap (AUDIO)
        public ConfigEntry<bool> AllowYippeeOverlap { get; private set; }
        //Key to set the volume of Yippee!
        public ConfigEntry<float> YippeeVolume { get; private set; }
        //Allow randomy pitched Yippee!'s
        public ConfigEntry<bool> RandomPitchedYippee { get; private set; }

        public YippeeSyncedConfig(ConfigFile cfg)
        {
            InitInstance(this);

            YippeeVolume = cfg.Bind("Gameplay",
                "Yippee Audio Volume",
                1f,
                "Volume of the Yippee sound itself.\nNote: This will not affect AI detection, only local sound.");

            NotifyEnemies = cfg.BindSyncedEntry("Gameplay",
                "Notify Enemy AI (HOST ONLY)",
                true,
                "Notify Enemies when someone shouts 'Yippee!'. This setting will only apply if you are the host.");

            CooldownEnabled = cfg.BindSyncedEntry("Gameplay",
                "Yippee cooldown (HOST ONLY)",
            false,
               "Enables cooldowns when you are hosting the instance");

            CooldownTime = cfg.BindSyncedEntry("Gameplay",
                "Yippee cooldown time (HOST ONLY)",
                2f,
                "The time (in seconds) it will take for the cooldown to stop");

            DeadPlayersYippeeAlivePlayers = cfg.BindSyncedEntry("Gameplay",
                "Dead players can Yippee (HOST ONLY)",
                true,
                "Allow dead players to yippee");

            EnemyAIDetectionRange = cfg.Bind("Gameplay Advanced",
                "EnemyAI detection range (HOST ONLY)",
                10,
                new ConfigDescription("DetectionRange for EnemyAI when shouting 'Yippee!' (HOST ONLY)\nDo not mess with this value unless you know what you're doing!",
                new AcceptableValueRange<int>(1, 100))).ToSyncedEntry();

            EnemyAIDetectionVolume = cfg.BindSyncedEntry("Gameplay Advanced",
                "EnemyAI detection volume (HOST ONLY)",
                1f,
                "Detection volume for EnemyAI when shouting 'Yippee!' (HOST ONLY)\nNote: This is NOT the volume of the sound, only the volume of which the EnemyAI component hears the sound.\nDo not mess with this value unless you know what you're doing!");

            DeadPlayerAlertsEnemyAI = cfg.BindSyncedEntry("Gameplay Advanced",
                "Dead players Yippee alerts AI (HOST ONLY)",
                false,
                "Dead players can yippe and alert AI (reduced range)");

            ConfigKey = cfg.Bind("Input",
                "Yippee Key Binding",
                KeyCode.I,
                "The keybind you're using to shout 'Yippee!'");


            RandomPitchedYippee = cfg.Bind("Audio",
                "Randomly pitched yippee",
                true,
                "Makes the pitch of the 'Yippee' when shouting be randomized");

            AllowYippeeOverlap = cfg.Bind("Audio",
                "Allow overlapping Yippee",
                false,
                "Make the audio when spamming Yippe overlap instead of restarting\n(useful with 'Spam Particles')");

            AllowVisuals = cfg.Bind("Visual",
                "Add visuals",
                true,
                "Adds visuals like particles when shouting 'Yippee!'");

            AllowParticeSpam = cfg.Bind("Visual",
                "Spam particles",
                false,
                "Allows particles to be spammed\n(useful with 'Allow Yippee Overlap')");

            DebugKey = cfg.Bind("Misc",
                "Show Debug Messages",
                false,
                "Whether or not debug messages appear inside the log");

            RemoveOrphans(cfg);
        }

        void RemoveOrphans(ConfigFile config)
        {
            PropertyInfo orphanedEntriesProp = config.GetType().GetProperty("OrphanedEntries", BindingFlags.NonPublic | BindingFlags.Instance);

            var orphanedEntries = (Dictionary<ConfigDefinition, string>)orphanedEntriesProp.GetValue(config, null);

            orphanedEntries.Clear(); // Clear orphaned entries (Unbinded/Abandoned entries)
            config.Save();
            
            //Thanks KittenJI!
        }

        internal static void RequestSync()
        {
            if (!IsClient) return;

            YippeeKeyPlugin.Instance.Log("Config Sync requested");

            using FastBufferWriter stream = new FastBufferWriter(IntSize, Allocator.Temp);

            // Method `OnRequestSync` will then get called on host.
            stream.SendMessage($"{YippeeKeyPlugin.GUID}_OnRequestConfigSync");
        }

        internal static void OnRequestSync(ulong clientId, FastBufferReader _)
        {
            if (!IsHost) return;

            YippeeKeyPlugin.Instance.Log("Syncing config");

            byte[] array = SerializeToBytes(Instance);
            int value = array.Length;

            using FastBufferWriter stream = new FastBufferWriter(value + IntSize, Allocator.Temp);

            try
            {
                stream.WriteValueSafe(in value, default);
                stream.WriteBytesSafe(array);

                stream.SendMessage($"{YippeeKeyPlugin.GUID}_OnReceiveConfigSync", clientId);
            }
            catch (Exception e)
            {
                YippeeKeyPlugin.Instance.LogError($"Error occurred syncing config with client: {clientId}\n{e}");
            }
        }

        internal static void OnReceiveSync(ulong _, FastBufferReader reader)
        {
            YippeeKeyPlugin.Instance.Log("Config file received");
            if (!reader.TryBeginRead(IntSize))
            {
                YippeeKeyPlugin.Instance.LogError("Config sync error: Could not begin reading buffer.");
                return;
            }

            reader.ReadValueSafe(out int val, default);
            if (!reader.TryBeginRead(val))
            {
                YippeeKeyPlugin.Instance.LogError("Config sync error: Host could not sync.");
                return;
            }

            byte[] data = new byte[val];
            reader.ReadBytesSafe(ref data, val);

            try
            {
                Instance.SyncInstance(data);
                YippeeKeyPlugin.Instance.Log("Config Synced succesfully!");
                YippeeKeyPlugin.Instance.Log($"Default config working? {(Default == null ? "Nope" : "Yes!")}");
                YippeeKeyPlugin.Instance.Log($"Synced config working? {(Instance == null ? "Nope" : "Yes!")}");
                YippeeKeyPlugin.Instance.Log($"AIdistance:\nlocal:{Default.EnemyAIDetectionRange.Value}\nSynced:{Instance.EnemyAIDetectionRange.Value}");
                YippeeKeyPlugin.Instance.Log($"AIVolume:\nlocal:{Default.EnemyAIDetectionVolume.Value}\nSynced:{Instance.EnemyAIDetectionVolume.Value}");
                YippeeKeyPlugin.Instance.Log($"NotifyAI:\nlocal:{Default.NotifyEnemies.Value}\nSynced:{Instance.NotifyEnemies.Value}");
                YippeeKeyPlugin.Instance.Log($"CooldownEnabled:\nlocal:{Default.CooldownEnabled.Value}\nSynced:{Instance.CooldownEnabled.Value}");
                YippeeKeyPlugin.Instance.Log($"CooldownTime:\nlocal:{Default.CooldownTime.Value}\nSynced:{Instance.CooldownTime.Value}");
                YippeeKeyPlugin.Instance.Log($"DeadPlayersCanYippee:\nlocal:{Default.DeadPlayersYippeeAlivePlayers.Value}\nSynced:{Instance.DeadPlayersYippeeAlivePlayers.Value}");
                YippeeKeyPlugin.Instance.Log($"DeadPlayerAlertEnemyAI:\nlocal:{Default.DeadPlayerAlertsEnemyAI.Value}\nSynced:{Instance.DeadPlayerAlertsEnemyAI.Value}");
            }
            catch (Exception e)
            {
                YippeeKeyPlugin.Instance.LogError($"Error syncing config instance!\n{e}");
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.ConnectClientToPlayerObject))]
        public static void InitializeLocalPlayer()
        {
            if (IsHost)
            {
                MessageManager.RegisterNamedMessageHandler($"{YippeeKeyPlugin.GUID}_OnRequestConfigSync", OnRequestSync);
                Synced = true;

                return;
            }

            Synced = false;
            MessageManager.RegisterNamedMessageHandler($"{YippeeKeyPlugin.GUID}_OnReceiveConfigSync", OnReceiveSync);
            RequestSync();
        }
    }
}
