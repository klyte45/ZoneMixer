using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using System.IO;
using UnityEngine;

namespace Klyte.ZoneMixer
{
    public class ZMController : BaseController<ZoneMixerMod, ZMController>
    {
        public static readonly string FOLDER_NAME = "ZoneMixer";
        public static readonly string FOLDER_PATH = FileUtils.BASE_FOLDER_PATH + FOLDER_NAME;

        public static readonly string DEFAULT_CONFIG_FILE = $"{FOLDER_PATH}{Path.DirectorySeparatorChar}DefaultConfiguration.xml";
    }
}