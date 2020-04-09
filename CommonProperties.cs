using Klyte.ZoneMixer;

namespace Klyte.Commons
{
    public static class CommonProperties
    {
        public static bool DebugMode => ZoneMixerMod.DebugMode;
        public static string Version => ZoneMixerMod.Version;
        public static string ModName => ZoneMixerMod.Instance.SimpleName;
        public static string Acronym => "ZM";
        public static string ModRootFolder => ZMController.FOLDER_PATH;
    }
}