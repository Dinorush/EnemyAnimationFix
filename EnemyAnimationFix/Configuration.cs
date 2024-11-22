using System;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using GTFO.API.Utilities;

namespace EnemyAnimationFix
{
    public static class Configuration
    {
        public static float MinWaveSleepTime { get; private set; } = 0f;
        public static float MaxWaveSleepTime { get; private set; } = 6f;

        public static event Action? OnReload;

        private readonly static ConfigFile configFile;

        static Configuration()
        {
            configFile = new ConfigFile(Path.Combine(Paths.ConfigPath, EntryPoint.MODNAME + ".cfg"), saveOnInit: true);
            BindAll(configFile);
        }

        internal static void Init()
        {
            LiveEditListener listener = LiveEdit.CreateListener(Paths.ConfigPath, EntryPoint.MODNAME + ".cfg", false);
            listener.FileChanged += OnFileChanged;
        }

        private static void OnFileChanged(LiveEditEventArgs _)
        {
            configFile.Reload();
            string section = "Wave Settings";
            MinWaveSleepTime = (float)configFile[section, "Minimum Wave Inactive Time"].BoxedValue;
            MaxWaveSleepTime = (float)configFile[section, "Maximum Wave Inactive Time"].BoxedValue;

            OnReload?.Invoke();
        }

        private static void BindAll(ConfigFile config)
        {
            string section = "Wave Settings";
            string description = "Minimum amount of time in seconds before enemies become active after spawning.";
            MinWaveSleepTime = config.Bind(section, "Minimum Wave Inactive Time", MinWaveSleepTime, description).Value;

            description = "Maximum amount of time in seconds before enemies become active after spawning.";
            MaxWaveSleepTime = config.Bind(section, "Maximum Wave Inactive Time", MaxWaveSleepTime, description).Value;
        }
    }
}
