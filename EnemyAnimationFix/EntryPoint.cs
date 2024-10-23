using BepInEx;
using BepInEx.Unity.IL2CPP;
using EnemyAnimationFix.NativePatches;
using EnemyAnimationFix.Networking.Notify;
using GTFO.API;
using HarmonyLib;

namespace EnemyAnimationFix
{
    [BepInPlugin("Dinorush." + MODNAME, MODNAME, "0.1.0")]
    [BepInDependency("dev.gtfomodding.gtfo-api", BepInDependency.DependencyFlags.HardDependency)]
    internal sealed class EntryPoint : BasePlugin
    {
        public const string MODNAME = "EnemyAnimationFix";
        public override void Load()
        {
            Log.LogMessage("Loading " + MODNAME);
            new Harmony(MODNAME).PatchAll();
            ChangeStatePatches.ApplyNativePatch();
            AssetAPI.OnStartupAssetsLoaded += AssetAPI_OnStartupAssetsLoaded;
            Log.LogMessage("Loaded " + MODNAME);
        }

        private void AssetAPI_OnStartupAssetsLoaded()
        {
            NotifyManager.Init();
        }
    }
}