using System.IO;
using BepInEx;
using BepInEx.Configuration;
using GTFO.API.Utilities;

namespace EnemyAnimationFix
{
    public static class Configuration
    {
        private readonly static ConfigEntry<float> _minWaveSleepTime;
        public static float MinWaveSleepTime => _minWaveSleepTime.Value;
        private readonly static ConfigEntry<float> _maxWaveSleepTime;
        public static float MaxWaveSleepTime => _maxWaveSleepTime.Value;

        private readonly static ConfigEntry<bool> _disableCullNear;
        public static bool DisableNearCull => _disableCullNear.Value;

        private readonly static ConfigFile configFile;

        static Configuration()
        {
            configFile = new ConfigFile(Path.Combine(Paths.ConfigPath, EntryPoint.MODNAME + ".cfg"), saveOnInit: true);

            string section = "Wave Settings";
            string description = "Minimum amount of time in seconds before enemies become active after spawning.";
            _minWaveSleepTime = configFile.Bind(section, "Minimum Wave Inactive Time", 0f, description);

            description = "Maximum amount of time in seconds before enemies become active after spawning.";
            _maxWaveSleepTime = configFile.Bind(section, "Maximum Wave Inactive Time", 6f, description);

            section = "Cull Settings";
            description = "Prevents nearby or attacking enemies from culling.\nThis fixes enemies not on screen failing to play footstep sounds or move when attacking.";
            _disableCullNear = configFile.Bind(section, "Disable Culling Nearby Enemies", true, description);
        }

        internal static void Init()
        {
            LiveEditListener listener = LiveEdit.CreateListener(Paths.ConfigPath, EntryPoint.MODNAME + ".cfg", false);
            listener.FileChanged += OnFileChanged;
        }

        private static void OnFileChanged(LiveEditEventArgs _)
        {
            configFile.Reload();
        }
    }
}
