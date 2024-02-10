using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Windows;
using YippeeKey.ConfigSync;
namespace YippeeKey
{
    /// <summary>
    /// Main plugin class, required for the fun of it.
    /// </summary>
    [BepInPlugin(GUID, Name, Version)]
    [BepInDependency("io.github.CSync")]
    internal class YippeeKeyPlugin : BaseUnityPlugin
    {

        public const string GUID = "QMLCYipeeKey_plugin";
        public const string Name = "Yippee key";
        public const string Version = "1.2.1.0";

        //AssetBundle
        public AssetBundle? MainAssetBundle;

        //No touchy, input
        public static YippeeKeyPlugin Instance;

        public YippeeSyncedConfig GetConfig;
        
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
            harmony.PatchAll(typeof(YippeeSyncedConfig));
            NetcodePatcher();
            Logger.LogInfo("Yippe mod ready!");
            Logger.LogInfo($"Debugging? [{(YippeeSyncedConfig.Instance.DebugKey.Value ? "Yes!" : "Nope.")}]");
            Logger.LogInfo($"Key to use Yippe: '{YippeeSyncedConfig.Instance.ConfigKey.Value}'");
        }

        //Easy way to log using the plugin logger, only used if Debug is enabled.
        public void Log(string message)
        {
            if (YippeeSyncedConfig.Default.DebugKey.Value) Logger.LogInfo(message);
        }

        //Same as above, but for errors, Ignores debug.
        public void LogError(string message)
        {
            Logger.LogError(message);
        }


        //Set up the config keys using this command, BepinEx handles it.
        private void SetupConfigKeys()
        {
            GetConfig = new YippeeSyncedConfig(Config);
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
