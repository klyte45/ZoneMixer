using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensors;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using Klyte.ZoneMixer.Overrides;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static ColossalFramework.UI.UITextureAtlas;

[assembly: AssemblyVersion("1.0.0.2")]
namespace Klyte.ZoneMixer
{
    public class ZoneMixerMod : BasicIUserMod<ZoneMixerMod, ZMController, ZMPanel>
    {

        public override string SimpleName => "Custom Zone Mixer";

        public override string Description => "Create 7 new configurable zoning types which can combine default game zones' behaviors.";

        protected override bool LoadUI => !ZMController.m_ghostMode;

        public override void TopSettingsUI(UIHelperExtension ext)
        {
            base.TopSettingsUI(ext);
            var ghostModeChk = ext.AddCheckbox(Locale.Get("K45_ZM_GHOST_MODE_OPTION"), ZMController.m_ghostMode, (x) =>
            {
                ZMController.m_ghostMode = x;
                ZoneMixerOverrides.FixZonePanel();
            }) as UICheckBox;
            ghostModeChk.tooltip = Locale.Get("K45_ZM_GHOST_MODE_OPTION_TOOLTIP");
            if (SimulationManager.exists && SimulationManager.instance.m_metaData != null)
            {
                ghostModeChk.Disable();
            }
        }

        protected override void OnLevelLoadingInternal()
        {
            if (!ZMController.m_ghostMode)
            {
                ZoneManager.instance.m_properties.m_zoneColors = new Color[0x10].Select((x, i) => ZoneManager.instance.m_properties.m_zoneColors.ElementAtOrDefault(i)).ToArray();
                ZoneManager.instance.m_properties.m_zoneColors[0x8] = new Color32(0x99, 0x20, 0x21, 0xff);
                ZoneManager.instance.m_properties.m_zoneColors[0x9] = new Color32(0xcc, 0x40, 0x41, 0xff);
                ZoneManager.instance.m_properties.m_zoneColors[0xA] = new Color32(0x00, 0xFF, 0x00, 0xff);
                ZoneManager.instance.m_properties.m_zoneColors[0xB] = new Color32(0xFF, 0x00, 0xFF, 0xff);
                ZoneManager.instance.m_properties.m_zoneColors[0xC] = new Color32(0x00, 0xFF, 0xFF, 0xff);
                ZoneManager.instance.m_properties.m_zoneColors[0xD] = new Color32(0x88, 0x88, 0x88, 0xff);
                ZoneManager.instance.m_properties.m_zoneColors[0xE] = new Color32(0xFF, 0xFF, 0x00, 0xff);
                ZoneManager.instance.m_zoneNotUsed = new ZoneTypeGuide[0x10].Select((x, i) => ZoneManager.instance.m_zoneNotUsed.ElementAtOrDefault(i) ?? new ZoneTypeGuide()).ToArray();
                ZoneManager.instance.m_goodAreaFound = new short[0x10].Select((x, i) => ZoneManager.instance.m_goodAreaFound.ElementAtOrDefault(i)).ToArray();
                typeof(ZoneProperties).GetMethod("InitializeProperties", RedirectorUtils.allFlags).Invoke(ZoneManager.instance.m_properties, new object[0]);


                var newSprites = new List<SpriteInfo>();
                TextureAtlasUtils.LoadImagesFromResources("UI.Images.InfoTooltip", ref newSprites);
                TextureAtlasUtils.RegenerateTextureAtlas(GeneratedScrollPanel.tooltipBox.Find<UISprite>("Sprite").atlas, newSprites);
            }
        }


    }
}