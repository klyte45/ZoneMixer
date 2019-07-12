using ColossalFramework;
using ColossalFramework.UI;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using Klyte.ZoneMixer.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using static Klyte.ZoneMixer.TextureAtlas.ZMXCommonTextureAtlas;

namespace Klyte.ZoneMixer.TextureAtlas
{
    public class ZMXCommonTextureAtlas : TextureAtlasDescriptor<ZMXCommonTextureAtlas, ZMXResourceLoader, SpriteNames>
    {
        protected override string ResourceName => "UI.Images.sprites.png";
        protected override string CommonName => "ZoneMixerSprites";
        public enum SpriteNames  {
                    ZoneMixerIcon,ZoneMixerIconSmall,ToolbarIconGroup6Hovered,ToolbarIconGroup6Focused
                }
    }
}
