using ColossalFramework;
using ColossalFramework.DataBinding;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using Klyte.Commons.Extensors;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using Klyte.ZoneMixer;
using Klyte.ZoneMixer.TextureAtlas;
using Klyte.ZoneMixer.Utils;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static Klyte.ZoneMixer.TextureAtlas.ZMXCommonTextureAtlas;

namespace Klyte.Commons
{
    public static class CommonProperties 
    {
        public static bool DebugMode => ZoneMixerMod.DebugMode;
        public static string Version => ZoneMixerMod.Version;
        public static string ModName => ZoneMixerMod.Instance.SimpleName;        
        public static object Acronym => "ZMX";
    }
}