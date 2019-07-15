using ColossalFramework;
using ColossalFramework.UI;
using Harmony;
using Klyte.Commons.Extensors;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using Klyte.ZoneMixer.TextureAtlas;
using Klyte.ZoneMixer.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using static Klyte.ZoneMixer.TextureAtlas.ZMXCommonTextureAtlas;

[assembly: AssemblyVersion("0.0.0.2")]
namespace Klyte.ZoneMixer
{
    public class ZoneMixerMod : BasicIUserMod<ZoneMixerMod, ZMXResourceLoader, MonoBehaviour, ZMXCommonTextureAtlas, UICustomControl, SpriteNames>
    {
        public ZoneMixerMod() => Construct();

        public override string SimpleName => "Zone Mixer";

        public override string Description => "Add two new zoning types: mixed low and mixed high residential/commercial areas";

        public override void DoErrorLog(string fmt, params object[] args) => LogUtils.DoErrorLog(fmt, args);

        public override void DoLog(string fmt, params object[] args) => LogUtils.DoLog(fmt, args);

        public override void LoadSettings()
        {
            List<PositionData<ItemClass.Zone>> enumTypes = ColossalFramework.Utils.GetOrderedEnumData<ItemClass.Zone>().ToList();
            enumTypes.Add(new PositionData<ItemClass.Zone>
            {
                index = 99,
                enumName = "MixLow",
                enumValue = (ItemClass.Zone) 8
            });
            enumTypes.Add(new PositionData<ItemClass.Zone>
            {
                index = 99,
                enumName = "MixHigh",
                enumValue = (ItemClass.Zone) 9
            });

            typeof(ZoningPanel).GetField("kZones", RedirectorUtils.allFlags).SetValue(null, enumTypes.ToArray());


            m_redirector = KlyteMonoUtils.CreateElement<Redirector>(null, "ZMX");
            m_redirector.AddRedirect(typeof(UnlockManager).GetMethod("InitializeProperties"), null, GetType().GetMethod("AddZonesUnlockData"));
        }

        public override void TopSettingsUI(UIHelperExtension ext)
        {
        }

        protected override void OnLevelLoadingInternal()
        {
            ZoneManager.instance.m_properties.m_zoneColors = new Color[0x10].Select((x, i) => ZoneManager.instance.m_properties.m_zoneColors.ElementAtOrDefault(i)).ToArray();
            ZoneManager.instance.m_properties.m_zoneColors[0x8] = new Color32(0x99, 0x20, 0x21, 0xff);
            ZoneManager.instance.m_properties.m_zoneColors[0x9] = new Color32(0xcc, 0x40, 0x41, 0xff);
            ZoneManager.instance.m_zoneNotUsed = new ZoneTypeGuide[0x10].Select((x, i) => ZoneManager.instance.m_zoneNotUsed.ElementAtOrDefault(i) ?? new ZoneTypeGuide()).ToArray();
            ZoneManager.instance.m_goodAreaFound = new short[0x10].Select((x, i) => ZoneManager.instance.m_goodAreaFound.ElementAtOrDefault(i)).ToArray();
            typeof(ZoneProperties).GetMethod("InitializeProperties", RedirectorUtils.allFlags).Invoke(ZoneManager.instance.m_properties, new object[0]);
            m_redirector.AddRedirect(typeof(ItemClass).GetMethod("GetSecondaryZone"), null, typeof(ZoneMixerMod).GetMethod("SecondaryZoneOverride", RedirectorUtils.allFlags));
            m_redirector.AddRedirect(typeof(ZoneBlock).GetMethod("SimulationStep"), null, null, typeof(ZoneMixerMod).GetMethod("SimulationStepTranspiller", RedirectorUtils.allFlags));
            m_redirector.AddRedirect(typeof(ZoneBlock).GetMethod("CheckBlock", RedirectorUtils.allFlags), null, null, typeof(ZoneMixerMod).GetMethod("CheckBlockTranspiller", RedirectorUtils.allFlags));


        }

        private Redirector m_redirector;


        public static void LogReturn(int instrId) => LogUtils.DoLog($"Exited at instruction {instrId}");

        public static void LogBreak(int instrId) => LogUtils.DoLog($"Breaked at instruction {instrId}");

        public static void AddZonesUnlockData(UnlockManager __instance) => __instance.m_properties.m_ZoneMilestones = new MilestoneInfo[0x10].Select((x, i) => __instance.m_properties.m_ZoneMilestones.ElementAtOrDefault(i) ?? __instance.m_properties.m_ZoneMilestones[0]).ToArray();

        internal static void SecondaryZoneOverride(ref ItemClass.Zone __result, ref ItemClass __instance)
        {
            if (__result != ItemClass.Zone.None)
            {
                return;
            }

            ItemClass.Zone primary = __instance.GetZone();
            if (primary == ItemClass.Zone.ResidentialHigh || primary == ItemClass.Zone.CommercialHigh)
            {
                __result = (ItemClass.Zone) 9;
            }

            if (primary == ItemClass.Zone.ResidentialLow || primary == ItemClass.Zone.CommercialLow)
            {
                __result = (ItemClass.Zone) 8;
            }
        }

        public static int GetCurrentDemandFor(ref ItemClass.Zone zone, byte district)
        {
            int num4 = 0;
            DistrictManager instance2 = DistrictManager.instance;
            ZoneManager instance = Singleton<ZoneManager>.instance;
            switch (zone)
            {
                case ItemClass.Zone.ResidentialLow:
                    Case_LowRes:
                    num4 = instance.m_actualResidentialDemand;
                    num4 += instance2.m_districts.m_buffer[district].CalculateResidentialLowDemandOffset();
                    break;
                case ItemClass.Zone.ResidentialHigh:
                    Case_HiRes:
                    num4 = instance.m_actualResidentialDemand;
                    num4 += instance2.m_districts.m_buffer[district].CalculateResidentialHighDemandOffset();
                    break;
                case ItemClass.Zone.CommercialLow:
                    Case_LowCom:
                    num4 = instance.m_actualCommercialDemand;
                    num4 += instance2.m_districts.m_buffer[district].CalculateCommercialLowDemandOffset();
                    break;
                case ItemClass.Zone.CommercialHigh:
                    Case_HiCom:
                    num4 = instance.m_actualCommercialDemand;
                    num4 += instance2.m_districts.m_buffer[district].CalculateCommercialHighDemandOffset();
                    break;
                case ItemClass.Zone.Industrial:
                    num4 = instance.m_actualWorkplaceDemand;
                    num4 += instance2.m_districts.m_buffer[district].CalculateIndustrialDemandOffset();
                    break;
                case ItemClass.Zone.Office:
                    num4 = instance.m_actualWorkplaceDemand;
                    num4 += instance2.m_districts.m_buffer[district].CalculateOfficeDemandOffset();
                    break;
                case (ItemClass.Zone) 8:
                    int districtResDemmand = instance.m_actualResidentialDemand + instance2.m_districts.m_buffer[district].CalculateResidentialLowDemandOffset();
                    int districtComDemmand = instance.m_actualCommercialDemand + instance2.m_districts.m_buffer[district].CalculateCommercialLowDemandOffset();
                    if (districtComDemmand > districtResDemmand)
                    {
                        zone = ItemClass.Zone.CommercialLow;
                        LogUtils.DoLog($"Zone 8 => {zone}");
                        goto Case_LowCom;
                    }
                    else
                    {
                        zone = ItemClass.Zone.ResidentialLow;
                        LogUtils.DoLog($"Zone 8 => {zone}");
                        goto Case_LowRes;
                    }
                case (ItemClass.Zone) 9:
                    int districtHResDemmand = instance.m_actualResidentialDemand + instance2.m_districts.m_buffer[district].CalculateResidentialHighDemandOffset();
                    int districtHComDemmand = instance.m_actualCommercialDemand + instance2.m_districts.m_buffer[district].CalculateCommercialHighDemandOffset();
                    if (districtHComDemmand > districtHResDemmand)
                    {
                        zone = ItemClass.Zone.CommercialHigh;
                        LogUtils.DoLog($"Zone 9 => {zone}");
                        goto Case_HiCom;
                    }
                    else
                    {
                        zone = ItemClass.Zone.ResidentialHigh;
                        LogUtils.DoLog($"Zone 9 => {zone}");
                        goto Case_HiRes;
                    }
            }

            return num4;

        }

        public static bool ZoneSupports(ItemClass.Zone zoneA, ItemClass.Zone zoneB)
        {
            if (zoneA == zoneB)
            {
                return true;
            }

            if (zoneA < zoneB)
            {
                return ZoneSupports(zoneB, zoneA);
            }

            if (zoneA == (ItemClass.Zone) 8)
            {
                return zoneB == ItemClass.Zone.CommercialLow || zoneB == ItemClass.Zone.ResidentialLow;
            }
            if (zoneA == (ItemClass.Zone) 9)
            {
                return zoneB == ItemClass.Zone.CommercialHigh || zoneB == ItemClass.Zone.ResidentialHigh;
            }
            return false;
        }

        internal static IEnumerable<CodeInstruction> SimulationStepTranspiller(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> inst = new List<CodeInstruction>(instructions);
            Label postSwitch = new Label();
            int idxFound = 0;

            List<Tuple<int, CodeInstruction[]>> detourLogPoints = new List<Tuple<int, CodeInstruction[]>>();
            for (int i = idxFound + 1; i < inst.Count - 2; i++)
            {
                if (inst[i].opcode == OpCodes.Ret)
                {
                    detourLogPoints.Add(Tuple.New(i, new CodeInstruction[] {
                        new CodeInstruction(OpCodes.Ldc_I4, i),
                        new CodeInstruction(OpCodes.Call, typeof(ZoneMixerMod).GetMethod("LogReturn")),
                    }));
                }
                else if (inst[i].opcode == OpCodes.Break)
                {
                    detourLogPoints.Add(Tuple.New(i, new CodeInstruction[] {
                        new CodeInstruction(OpCodes.Ldc_I4, i),
                        new CodeInstruction(OpCodes.Call, typeof(ZoneMixerMod).GetMethod("LogBreak")),
                    }));
                }
            }
            detourLogPoints.Sort((a, b) => a.First - b.First);
            detourLogPoints.ForEach(x => inst.InsertRange(x.First, x.Second));

            for (int i = 1; i < inst.Count; i++)
            {
                if (inst[i - 1].opcode == OpCodes.Callvirt && inst[i - 1].operand is MethodInfo mi && mi.Name == "GetDistrict" && inst[i].opcode == OpCodes.Stloc_S)
                {
                    inst.InsertRange(i + 1, new List<CodeInstruction>()
                    {
                        new CodeInstruction(OpCodes.Ldloca_S, 6 ),
                        new CodeInstruction(OpCodes.Ldloc_S, 9 ),
                        new CodeInstruction(OpCodes.Call, typeof(ZoneMixerMod).GetMethod("GetCurrentDemandFor") ),
                        new CodeInstruction(OpCodes.Stloc_S, 10 ),
                        new CodeInstruction(OpCodes.Ldstr, "LOGGING ZONE! district: {0} - demand: {1} - zone: {2}" ),
                        new CodeInstruction(OpCodes.Ldc_I4_3),
                        new CodeInstruction(OpCodes.Newarr, typeof(object)),
                        new CodeInstruction(OpCodes.Dup),
                        new CodeInstruction(OpCodes.Ldc_I4_0),
                        new CodeInstruction(OpCodes.Ldloc_S, 9 ),
                        new CodeInstruction(OpCodes.Box,typeof(byte) ),
                        new CodeInstruction(OpCodes.Stelem_Ref ),
                        new CodeInstruction(OpCodes.Dup),
                        new CodeInstruction(OpCodes.Ldc_I4_1),
                        new CodeInstruction(OpCodes.Ldloc_S, 10 ),
                        new CodeInstruction(OpCodes.Box,typeof(int) ),
                        new CodeInstruction(OpCodes.Stelem_Ref ),
                        new CodeInstruction(OpCodes.Dup),
                        new CodeInstruction(OpCodes.Ldc_I4_2),
                        new CodeInstruction(OpCodes.Ldloc_S, 6 ),
                        new CodeInstruction(OpCodes.Box,typeof(ItemClass.Zone) ),
                        new CodeInstruction(OpCodes.Stelem_Ref ),
                        new CodeInstruction(OpCodes.Call, typeof(LogUtils).GetMethod("DoLog") ),
                        new CodeInstruction(OpCodes.Br, postSwitch)
                    });
                    idxFound = i;
                    break;
                }
            }

            for (int i = idxFound + 1; i < inst.Count - 2; i++)
            {
                if (inst[i].opcode == OpCodes.Ret)
                {
                    inst[i + 1].labels.Add(postSwitch);
                    break;
                }
            }
            return inst;
        }

        public static IEnumerable<CodeInstruction> CheckBlockTranspiller(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> inst = new List<CodeInstruction>(instructions);
            for (int i = 2; i < inst.Count - 1; i++)
            {
                if (inst[i - 2].opcode == OpCodes.Call && inst[i - 2].operand is MethodInfo mi && mi.Name == "GetZone" && inst[i - 1].opcode == OpCodes.Ldarg_3)
                {
                    inst[i].opcode = OpCodes.Call;
                    inst[i].operand = typeof(ZoneMixerMod).GetMethod("ZoneSupports");
                    inst[i + 1].opcode = OpCodes.Brfalse;
                    break;
                }
            }
            return inst;
        }
    }
}