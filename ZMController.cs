using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using Klyte.ZoneMixer.Overrides;
using System;
using System.IO;
using UnityEngine;

namespace Klyte.ZoneMixer
{
    public class ZMController : BaseController<ZoneMixerMod, ZMController>
    {
        public static readonly string FOLDER_NAME = "ZoneMixer";
        public static readonly string FOLDER_PATH = FileUtils.BASE_FOLDER_PATH + FOLDER_NAME;

        public static readonly string DEFAULT_CONFIG_FILE = $"{FOLDER_PATH}{Path.DirectorySeparatorChar}DefaultConfiguration.xml";

        public static bool m_ghostMode;

        protected override void StartActions()
        {
            if (m_ghostMode)
            {

                for (ushort a = 1; a < BuildingManager.instance.m_buildings.m_buffer.Length; a++)
                {
                    if (BuildingManager.instance.m_buildings.m_buffer[a].Info.m_buildingAI is PrivateBuildingAI)
                    {
                        Vector3 position = BuildingManager.instance.m_buildings.m_buffer[a].m_position;
                        int num = Mathf.Max((int)((position.x - 35f) / 64f + 135f), 0);
                        int num2 = Mathf.Max((int)((position.z - 35f) / 64f + 135f), 0);
                        int num3 = Mathf.Min((int)((position.x + 35f) / 64f + 135f), 269);
                        int num4 = Mathf.Min((int)((position.z + 35f) / 64f + 135f), 269);
                        Array16<Building> buildings = Singleton<BuildingManager>.instance.m_buildings;
                        ushort[] buildingGrid = Singleton<BuildingManager>.instance.m_buildingGrid;
                        for (int i = num2; i <= num4; i++)
                        {
                            for (int j = num; j <= num3; j++)
                            {
                                ushort num5 = buildingGrid[i * 270 + j];
                                int num6 = 0;
                                while (num5 != 0)
                                {
                                    ushort nextGridBuilding = buildings.m_buffer[num5].m_nextGridBuilding;
                                    Building.Flags flags = buildings.m_buffer[num5].m_flags;
                                    if ((flags & (Building.Flags.Created | Building.Flags.Deleted | Building.Flags.Demolishing | Building.Flags.Collapsed)) == Building.Flags.Created)
                                    {
                                        BuildingInfo info = buildings.m_buffer[num5].Info;
                                        if (info != null && info.m_placementStyle == ItemClass.Placement.Automatic)
                                        {
                                            ItemClass.Zone zone = info.m_class.GetZone();
                                            ItemClass.Zone secondaryZone = info.m_class.GetSecondaryZone();
                                            if (zone != ItemClass.Zone.None && VectorUtils.LengthSqrXZ(buildings.m_buffer[num5].m_position - position) <= 1225f)
                                            {
                                                buildings.m_buffer[num5].CheckZoning(zone, secondaryZone, true);
                                            }
                                        }
                                    }
                                    num5 = nextGridBuilding;
                                    if (++num6 >= 49152)
                                    {
                                        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                                        break;
                                    }
                                }
                            }
                        }

                    }
                }

                for (ushort i = 1; i < ZoneManager.instance.m_blocks.m_buffer.Length; i++)
                {
                    bool changed = false;
                    for (int x = 0; x < 4; x++)
                    {
                        for (int z = 0; z < 8; z++)
                        {
                            changed = ZoneMixerOverrides.GetBlockZoneSanitize(ref ZoneManager.instance.m_blocks.m_buffer[i], x, z) | changed;
                        }
                    }
                    if (changed) { ZoneManager.instance.m_blocks.m_buffer[i].RefreshZoning(i); }
                }

                K45DialogControl.ShowModal(new K45DialogControl.BindProperties()
                {
                    icon = ZoneMixerMod.Instance.IconName,
                    title = Locale.Get("K45_ZM_GHOST_MODE_MODAL_TITLE"),
                    message = Locale.Get("K45_ZM_GHOST_MODE_MODAL_MESSAGE"),
                    showButton1 = true,
                    textButton1 = Locale.Get("K45_ZM_OK_BUTTON")
                }, (x) => true);

            }
        }
    }
}