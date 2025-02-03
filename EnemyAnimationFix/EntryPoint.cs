﻿using BepInEx;
using BepInEx.Unity.IL2CPP;
using EnemyAnimationFix.NativePatches;
using EnemyAnimationFix.Networking.Notify;
using GTFO.API;
using HarmonyLib;

namespace EnemyAnimationFix
{
    [BepInPlugin("Dinorush." + MODNAME, MODNAME, "1.2.2")]
    [BepInDependency("dev.gtfomodding.gtfo-api", BepInDependency.DependencyFlags.HardDependency)]
    internal sealed class EntryPoint : BasePlugin
    {
        public const string MODNAME = "EnemyAnimationFix";
        public override void Load()
        {
            Log.LogMessage("Loading " + MODNAME);
            new Harmony(MODNAME).PatchAll();
            ChangeStatePatches.ApplyNativePatch();
            ValidTargetPatches.ApplyInstructionPatch();
            Configuration.Init();
            AssetAPI.OnStartupAssetsLoaded += AssetAPI_OnStartupAssetsLoaded;
            LevelAPI.OnLevelCleanup += LevelAPI_OnLevelCleanup;
            Log.LogMessage("Loaded " + MODNAME);
        }

        private void AssetAPI_OnStartupAssetsLoaded()
        {
            NotifyManager.Init();
        }

        private void LevelAPI_OnLevelCleanup()
        {
            // Fix screams not resetting between runs
            Enemies.EB_InCombat.s_globalScreamTimer = 0;
        }
    }
}