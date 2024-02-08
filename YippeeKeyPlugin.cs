using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Windows;

namespace YippeeKey
{
    /// <summary>
    /// Main plugin class, required for the fun of it.
    /// </summary>
    [BepInPlugin(GUID, Name, Version)]
    internal class YippeeKeyPlugin : BaseUnityPlugin
    {
        //Keycode, might be replaced with inpututils
        public ConfigEntry<KeyCode> ConfigKey;
        //Debug config, usable for debugging, not for public use.
        public ConfigEntry<bool> DebugKey;
        //Key to disable notifying game about sound
        public ConfigEntry<bool> NotifyEnemies;
        //Key to disable visuals of the mod (Particles)
        public ConfigEntry<bool> AllowVisuals;


        const string GUID = "QMLCYipeeKey_plugin";
        const string Name = "Yippee key";
        const string Version = "0.0.0.1";

        //AssetBundle
        public AssetBundle? MainAssetBundle;

        //No touchy, input
        public static YippeeKeyPlugin Instance;

        //No touchy, Harmony
        Harmony harmony = new Harmony("net.quadmesh.yippeekey.plugin");

        private void Awake()
        {
            Instance = this;
            SetupConfigKeys();
            var dllFolderPath = System.IO.Path.GetDirectoryName(Info.Location);
            var assetBundleFilePath = System.IO.Path.Combine(dllFolderPath, "yippeekey");
            MainAssetBundle = AssetBundle.LoadFromFile(assetBundleFilePath);
            if (MainAssetBundle != null)
            {
                Logger.LogInfo($"{MainAssetBundle.name} Should be loaded succesfully.");
            }
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), GUID);
            NetcodePatcher();
            Logger.LogInfo("Yippe mod ready!");
            Logger.LogInfo($"Debugging? [{(DebugKey.Value ? "Yes!" : "Nope.")}]");
            Logger.LogInfo($"Key to use Yippe: '{ConfigKey.Value}'");
        }

        //Easy way to log using the plugin logger, only used if Debug is enabled.
        public void Log(string message)
        {
            if (DebugKey.Value) Logger.LogInfo(message);
        }

        //Set up the config keys using this command, BepinEx handles it.
        private void SetupConfigKeys()
        {
            ConfigKey = Config.Bind("Input",
                "InputKeyYippeee",
                KeyCode.I,
                "The keycode you're using to SCREAM YIPPEE!"
                );

            AllowVisuals = Config.Bind("Visual",
                "Add visuals",
                true,
                "Adds visuals like particles when shouting 'Yippee!'");

            NotifyEnemies = Config.Bind("Gameplay",
                "Notify Enemy AI (HOST ONLY)",
                true,
                "Notify Enemies when someone shouts 'Yippee!'. This setting will only apply if you are the host.");

            DebugKey = Config.Bind("Misc",
                "Show Debug Messages",
                false,
                "Whether or not debug messages appear inside the log");
        }

        //Netcode stuff, requried for patching.
        private static void NetcodePatcher()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }
        }
    }
}
