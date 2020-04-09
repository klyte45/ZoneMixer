using ColossalFramework;
using ColossalFramework.UI;
using Harmony;
using Klyte.Commons.Extensors;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

[assembly: AssemblyVersion("0.0.1.*")]
namespace Klyte.ZoneMixer
{
    public class ZoneMixerMod : BasicIUserModSimplified<ZoneMixerMod, ZMController>
    {

        public override string SimpleName => "Zone Mixer";

        public override string Description => "Add two new zoning types: mixed low and mixed high residential/commercial areas";                  


        protected override void OnLevelLoadingInternal()
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
        }
        

    }
}