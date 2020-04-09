using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using UnityEngine;

namespace Klyte.ZoneMixer
{
    public class ZMController : BaseController<ZoneMixerMod, ZMController>
    {
        public static readonly string FOLDER_NAME = "ZoneMixer";
        public static readonly string FOLDER_PATH = FileUtils.BASE_FOLDER_PATH + FOLDER_NAME;
    }
}