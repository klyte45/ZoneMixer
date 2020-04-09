using ColossalFramework;
using ColossalFramework.UI;
using Harmony;
using Klyte.Commons.Extensors;
using Klyte.Commons.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using static TerrainManager;

namespace Klyte.ZoneMixer.Overrides
{

    public class ZoneMixerOverrides : Redirector, IRedirectable
    {

        public void Awake()
        {

            var enumTypes = ColossalFramework.Utils.GetOrderedEnumData<ItemClass.Zone>().Where(x =>
            {
                return (int)x.enumValue < 8 || (int)x.enumValue > 14;
            }).ToList();
            enumTypes.AddRange(new List<PositionData<ItemClass.Zone>>(){
                new PositionData<ItemClass.Zone>
            {
                index = 90,
                enumName = "Z1",
                enumValue = (ItemClass.Zone)8
            },
             new PositionData<ItemClass.Zone>
             {
                index = 91,
                enumName = "Z2",
                enumValue = (ItemClass.Zone)9
            },
             new PositionData<ItemClass.Zone>
             {
                index = 92,
                enumName = "Z3",
                enumValue = (ItemClass.Zone)10
            },
             new PositionData<ItemClass.Zone>
             {
                index = 93,
                enumName = "Z4",
                enumValue = (ItemClass.Zone)11
            },
             new PositionData<ItemClass.Zone>
             {
                index = 94,
                enumName = "Z5",
                enumValue = (ItemClass.Zone)12
            },
             new PositionData<ItemClass.Zone>
             {
                index = 95,
                enumName = "Z6",
                enumValue = (ItemClass.Zone)13
            },
             new PositionData<ItemClass.Zone>
             {
                index = 96,
                enumName = "Z7",
                enumValue = (ItemClass.Zone)14
            }
            });

            typeof(ZoningPanel).GetField("kZones", RedirectorUtils.allFlags).SetValue(null, enumTypes.ToArray());

            AddRedirect(typeof(UnlockManager).GetMethod("InitializeProperties"), null, GetType().GetMethod("AddZonesUnlockData"));
            AddRedirect(typeof(ZoneBlock).GetMethod("SimulationStep"), null, null, typeof(ZoneMixerOverrides).GetMethod("SimulationStepTranspiller", RedirectorUtils.allFlags));
            AddRedirect(typeof(ZoneBlock).GetMethod("CheckBlock", RedirectorUtils.allFlags), null, null, typeof(ZoneMixerOverrides).GetMethod("CheckBlockTranspiller", RedirectorUtils.allFlags));
            AddRedirect(typeof(TerrainPatch).GetMethod("Refresh", RedirectorUtils.allFlags), null, null, typeof(ZoneMixerOverrides).GetMethod("TranspilePatchRefresh", RedirectorUtils.allFlags));
            AddRedirect(typeof(Building).GetMethod("CheckZoning", RedirectorUtils.allFlags, null, new Type[] { typeof(ItemClass.Zone), typeof(ItemClass.Zone), typeof(uint).MakeByRefType(), typeof(bool).MakeByRefType(), typeof(ZoneBlock).MakeByRefType() }, null), null, null, typeof(ZoneMixerOverrides).GetMethod("TranspileCheckZoning", RedirectorUtils.allFlags));
            AddRedirect(typeof(GeneratedScrollPanel).GetMethod("SpawnEntry", RedirectorUtils.allFlags, null, new Type[] { typeof(string), typeof(string), typeof(string), typeof(UITextureAtlas), typeof(UIComponent), typeof(bool)}, null),  typeof(ZoneMixerOverrides).GetMethod("ZonePanelSpawnEntryPre", RedirectorUtils.allFlags));
    

            AddRedirect(typeof(BuildingManager).GetMethod("ReleaseBuilding"), typeof(ZoneMixerOverrides).GetMethod("LogStacktrace"));
        }

        public static void ZonePanelSpawnEntryPre(ref string thumbnail, ref UITextureAtlas atlas)
        {
            if (thumbnail.StartsWith("ZoningZ"))
            {
                atlas = TextureAtlasUtils.DefaultTextureAtlas;
            }
        }

        public static void LogStacktrace() => LogUtils.DoWarnLog($"RELEASE BUILDING: {Environment.StackTrace}");

        public static void LogReturn(int instrId) => LogUtils.DoLog($"Exited at instruction {instrId}");

        public static void LogBreak(int instrId) => LogUtils.DoLog($"Breaked at instruction {instrId}");

        public static void AddZonesUnlockData(UnlockManager __instance) => __instance.m_properties.m_ZoneMilestones = new MilestoneInfo[0x10].Select((x, i) => __instance.m_properties.m_ZoneMilestones.ElementAtOrDefault(i) ?? __instance.m_properties.m_ZoneMilestones[0]).ToArray();

        //internal static void SecondaryZoneOverride(ref ItemClass.Zone __result, ref ItemClass __instance)
        //{
        //    if (__result != ItemClass.Zone.None)
        //    {
        //        return;
        //    }

        //    ItemClass.Zone primary = __instance.GetZone();
        //    if (primary == ItemClass.Zone.ResidentialHigh || primary == ItemClass.Zone.CommercialHigh)
        //    {
        //        __result = (ItemClass.Zone)9;
        //    }

        //    if (primary == ItemClass.Zone.ResidentialLow || primary == ItemClass.Zone.CommercialLow)
        //    {
        //        __result = (ItemClass.Zone)8;
        //    }
        //}

        public static ItemClass.Zone GetBlockZoneOverride(ref ZoneBlock block, int x, int y, ItemClass.Zone zone1, ItemClass.Zone zone2)
        {
            ItemClass.Zone targetZone = block.GetZone(x, y);
            switch ((int)targetZone)
            {
                case 8:
                    return zone1 == ItemClass.Zone.CommercialLow || zone1 == ItemClass.Zone.ResidentialLow ? zone1 : zone2 == ItemClass.Zone.CommercialLow || zone2 == ItemClass.Zone.ResidentialLow ? zone2 : targetZone;
                case 9:
                    return zone1 == ItemClass.Zone.CommercialHigh || zone1 == ItemClass.Zone.ResidentialHigh ? zone1 : zone2 == ItemClass.Zone.CommercialHigh || zone2 == ItemClass.Zone.ResidentialHigh ? zone2 : targetZone;
                case 10:
                    return zone1 == ItemClass.Zone.ResidentialHigh || zone1 == ItemClass.Zone.ResidentialLow ? zone1 : zone2 == ItemClass.Zone.ResidentialHigh || zone2 == ItemClass.Zone.ResidentialLow ? zone2 : targetZone;
                case 11:
                    return zone1 == ItemClass.Zone.CommercialLow || zone1 == ItemClass.Zone.CommercialHigh ? zone1 : zone2 == ItemClass.Zone.CommercialLow || zone2 == ItemClass.Zone.CommercialHigh ? zone2 : targetZone;
                case 12:
                    return zone1 == ItemClass.Zone.CommercialLow || zone1 == ItemClass.Zone.CommercialHigh || zone1 == ItemClass.Zone.Office ? zone1 : zone2 == ItemClass.Zone.CommercialLow || zone2 == ItemClass.Zone.CommercialHigh || zone2 == ItemClass.Zone.Office ? zone2 : targetZone;
                case 13:
                    return zone1 == ItemClass.Zone.Office || zone1 == ItemClass.Zone.Industrial ? zone1 : zone2 == ItemClass.Zone.Office || zone2 == ItemClass.Zone.Industrial ? zone2 : targetZone;
                case 14:
                    return zone1 == ItemClass.Zone.CommercialLow || zone1 == ItemClass.Zone.CommercialHigh || zone1 == ItemClass.Zone.Industrial ? zone1 : zone2 == ItemClass.Zone.CommercialLow || zone2 == ItemClass.Zone.CommercialHigh || zone2 == ItemClass.Zone.Industrial ? zone2 : targetZone;
                default:
                    return targetZone;
            }
        }

        public static int GetCurrentDemandFor(ref ItemClass.Zone zone, byte district)
        {
            int num4 = 0;
            DistrictManager instance2 = DistrictManager.instance;
            ZoneManager instance = Singleton<ZoneManager>.instance;
            LogUtils.DoWarnLog($"GetCurrentDemandFor {zone},{district}");
            switch (zone)
            {
                case ItemClass.Zone.ResidentialLow:
                    num4 = instance.m_actualResidentialDemand;
                    num4 += instance2.m_districts.m_buffer[district].CalculateResidentialLowDemandOffset();
                    break;
                case ItemClass.Zone.ResidentialHigh:
                    num4 = instance.m_actualResidentialDemand;
                    num4 += instance2.m_districts.m_buffer[district].CalculateResidentialHighDemandOffset();
                    break;
                case ItemClass.Zone.CommercialLow:
                    num4 = instance.m_actualCommercialDemand;
                    num4 += instance2.m_districts.m_buffer[district].CalculateCommercialLowDemandOffset();
                    break;
                case ItemClass.Zone.CommercialHigh:
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
                case (ItemClass.Zone)8:
                    int districtResDemmand = GetDistrictLResDemmand(district, instance2, instance);
                    int districtComDemmand = GetDistrictLComDemmand(district, instance2, instance);
                    if (districtComDemmand > districtResDemmand)
                    {
                        zone = ItemClass.Zone.CommercialLow;
                        LogUtils.DoLog($"Zone 8 => {zone}");
                        return districtComDemmand;
                    }
                    else
                    {
                        zone = ItemClass.Zone.ResidentialLow;
                        LogUtils.DoLog($"Zone 8 => {zone}");
                        return districtResDemmand;
                    }
                case (ItemClass.Zone)9:
                    int districtHResDemmand = GetDistrictHResDemmand(district, instance2, instance);
                    int districtHComDemmand = GetDistrictHComDemmand(district, instance2, instance);
                    if (districtHComDemmand > districtHResDemmand)
                    {
                        zone = ItemClass.Zone.CommercialHigh;
                        LogUtils.DoLog($"Zone 9 => {zone}");
                        return districtHComDemmand;
                    }
                    else
                    {
                        zone = ItemClass.Zone.ResidentialHigh;
                        LogUtils.DoLog($"Zone 9 => {zone}");
                        return districtHResDemmand;
                    }
                case (ItemClass.Zone)10:
                    int hres10 = GetDistrictHResDemmand(district, instance2, instance);
                    int lres10 = GetDistrictLResDemmand(district, instance2, instance);
                    if (hres10 > lres10)
                    {
                        zone = ItemClass.Zone.ResidentialHigh;
                        LogUtils.DoLog($"Zone 10 => {zone}");
                        return hres10;
                    }
                    else
                    {
                        zone = ItemClass.Zone.ResidentialLow;
                        LogUtils.DoLog($"Zone 10 => {zone}");
                        return lres10;
                    }
                case (ItemClass.Zone)11:
                    int hcom11 = GetDistrictHComDemmand(district, instance2, instance);
                    int lcom11 = GetDistrictLComDemmand(district, instance2, instance);
                    if (hcom11 > lcom11)
                    {
                        zone = ItemClass.Zone.CommercialHigh;
                        LogUtils.DoLog($"Zone 11 => {zone}");
                        return hcom11;
                    }
                    else
                    {
                        zone = ItemClass.Zone.CommercialLow;
                        LogUtils.DoLog($"Zone 11 => {zone}");
                        return lcom11;
                    }
                case (ItemClass.Zone)12:
                    int demOffice12 = GetDistrictOffcDemmand(district, instance2, instance);
                    int demHCom12 = GetDistrictHComDemmand(district, instance2, instance);
                    int demLCom12 = GetDistrictLComDemmand(district, instance2, instance);
                    if (demLCom12 > demHCom12)
                    {
                        if (demLCom12 > demOffice12)
                        {
                            zone = ItemClass.Zone.CommercialLow;
                            LogUtils.DoLog($"Zone 12 => {zone}");
                            return demLCom12;
                        }
                        else
                        {
                            zone = ItemClass.Zone.Office;
                            LogUtils.DoLog($"Zone 12 => {zone}");
                            return demOffice12;
                        }
                    }
                    else
                    {
                        if (demHCom12 > demOffice12)
                        {
                            zone = ItemClass.Zone.CommercialHigh;
                            LogUtils.DoLog($"Zone 12 => {zone}");
                            return demHCom12;
                        }
                        else
                        {
                            zone = ItemClass.Zone.Office;
                            LogUtils.DoLog($"Zone 12 => {zone}");
                            return demOffice12;
                        }
                    }
                case (ItemClass.Zone)13:
                    int ind13 = GetDistrictIndtDemmand(district, instance2, instance);
                    int off13 = GetDistrictOffcDemmand(district, instance2, instance);
                    if (ind13 > off13)
                    {
                        zone = ItemClass.Zone.Industrial;
                        LogUtils.DoLog($"Zone 13 => {zone}");
                        return ind13;
                    }
                    else
                    {
                        zone = ItemClass.Zone.Office;
                        LogUtils.DoLog($"Zone 13 => {zone}");
                        return off13;
                    }
                case (ItemClass.Zone)14:
                    int demInd14 = GetDistrictIndtDemmand(district, instance2, instance);
                    int demHCom14 = GetDistrictHComDemmand(district, instance2, instance);
                    int demLCom14 = GetDistrictLComDemmand(district, instance2, instance);
                    if (demLCom14 > demHCom14)
                    {
                        if (demLCom14 > demInd14)
                        {
                            zone = ItemClass.Zone.CommercialLow;
                            LogUtils.DoLog($"Zone 14 => {zone}");
                            return demLCom14;
                        }
                        else
                        {
                            zone = ItemClass.Zone.Industrial;
                            LogUtils.DoLog($"Zone 14 => {zone}");
                            return demInd14;
                        }
                    }
                    else
                    {
                        if (demHCom14 > demInd14)
                        {
                            zone = ItemClass.Zone.CommercialHigh;
                            LogUtils.DoLog($"Zone 14 => {zone}");
                            return demHCom14;
                        }
                        else
                        {
                            zone = ItemClass.Zone.Industrial;
                            LogUtils.DoLog($"Zone 14 => {zone}");
                            return demInd14;
                        }
                    }
            }

            return num4;

        }

        private static int GetDistrictLComDemmand(byte district, DistrictManager instance2, ZoneManager instance) => instance.m_actualCommercialDemand + instance2.m_districts.m_buffer[district].CalculateCommercialLowDemandOffset();
        private static int GetDistrictLResDemmand(byte district, DistrictManager instance2, ZoneManager instance) => instance.m_actualResidentialDemand + instance2.m_districts.m_buffer[district].CalculateResidentialLowDemandOffset();
        private static int GetDistrictHComDemmand(byte district, DistrictManager instance2, ZoneManager instance) => instance.m_actualCommercialDemand + instance2.m_districts.m_buffer[district].CalculateCommercialHighDemandOffset();
        private static int GetDistrictHResDemmand(byte district, DistrictManager instance2, ZoneManager instance) => instance.m_actualResidentialDemand + instance2.m_districts.m_buffer[district].CalculateResidentialHighDemandOffset();
        private static int GetDistrictOffcDemmand(byte district, DistrictManager instance2, ZoneManager instance) => instance.m_actualWorkplaceDemand + instance2.m_districts.m_buffer[district].CalculateOfficeDemandOffset();
        private static int GetDistrictIndtDemmand(byte district, DistrictManager instance2, ZoneManager instance) => instance.m_actualWorkplaceDemand + instance2.m_districts.m_buffer[district].CalculateIndustrialDemandOffset();

        internal static IEnumerable<CodeInstruction> SimulationStepTranspiller(IEnumerable<CodeInstruction> instructions)
        {
            var inst = new List<CodeInstruction>(instructions);
            var postSwitch = new Label();
            int idxFound = 0;
            if (ZoneMixerMod.DebugMode)
            {
                var detourLogPoints = new List<Tuple<int, CodeInstruction[]>>();
                for (int i = idxFound + 1; i < inst.Count - 2; i++)
                {
                    if (inst[i].opcode == OpCodes.Ret)
                    {
                        detourLogPoints.Add(Tuple.New(i, new CodeInstruction[] {
                        new CodeInstruction(OpCodes.Ldc_I4, i),
                        new CodeInstruction(OpCodes.Call, typeof(ZoneMixerOverrides).GetMethod("LogReturn")),
                    }));
                    }
                    else if (inst[i].opcode == OpCodes.Break)
                    {
                        detourLogPoints.Add(Tuple.New(i, new CodeInstruction[] {
                        new CodeInstruction(OpCodes.Ldc_I4, i),
                        new CodeInstruction(OpCodes.Call, typeof(ZoneMixerOverrides).GetMethod("LogBreak")),
                    }));
                    }
                }
                detourLogPoints.Sort((a, b) => a.First - b.First);
                detourLogPoints.ForEach(x => inst.InsertRange(x.First, x.Second));
            }
            for (int i = 1; i < inst.Count; i++)
            {
                if (inst[i - 1].opcode == OpCodes.Callvirt && inst[i - 1].operand is MethodInfo mi && mi.Name == "GetDistrict" && inst[i].opcode == OpCodes.Stloc_S)
                {
                    inst.InsertRange(i + 1, new List<CodeInstruction>()
                    {
                        new CodeInstruction(OpCodes.Ldloca_S, 6 ),
                        new CodeInstruction(OpCodes.Ldloc_S, 9 ),
                        new CodeInstruction(OpCodes.Call, typeof(ZoneMixerOverrides).GetMethod("GetCurrentDemandFor") ),
                        new CodeInstruction(OpCodes.Stloc_S, 10 ),
                        //new CodeInstruction(OpCodes.Ldstr, "LOGGING ZONE! district: {0} - demand: {1} - zone: {2}" ),
                        //new CodeInstruction(OpCodes.Ldc_I4_3),
                        //new CodeInstruction(OpCodes.Newarr, typeof(object)),
                        //new CodeInstruction(OpCodes.Dup),
                        //new CodeInstruction(OpCodes.Ldc_I4_0),
                        //new CodeInstruction(OpCodes.Ldloc_S, 9 ),
                        //new CodeInstruction(OpCodes.Box,typeof(byte) ),
                        //new CodeInstruction(OpCodes.Stelem_Ref ),
                        //new CodeInstruction(OpCodes.Dup),
                        //new CodeInstruction(OpCodes.Ldc_I4_1),
                        //new CodeInstruction(OpCodes.Ldloc_S, 10 ),
                        //new CodeInstruction(OpCodes.Box,typeof(int) ),
                        //new CodeInstruction(OpCodes.Stelem_Ref ),
                        //new CodeInstruction(OpCodes.Dup),
                        //new CodeInstruction(OpCodes.Ldc_I4_2),
                        //new CodeInstruction(OpCodes.Ldloc_S, 6 ),
                        //new CodeInstruction(OpCodes.Box,typeof(ItemClass.Zone) ),
                        //new CodeInstruction(OpCodes.Stelem_Ref ),
                        //new CodeInstruction(OpCodes.Call, typeof(LogUtils).GetMethod("DoLog") ),
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
            LogUtils.PrintMethodIL(inst);
            return inst;
        }

        public static IEnumerable<CodeInstruction> CheckBlockTranspiller(IEnumerable<CodeInstruction> instructions)
        {
            var inst = new List<CodeInstruction>(instructions);
            MethodInfo getBlockZoneOverride = typeof(ZoneMixerOverrides).GetMethod("GetBlockZoneOverride");
            for (int i = 2; i < inst.Count - 1; i++)
            {
                if (inst[i].opcode == OpCodes.Call && inst[i].operand == GET_BLOCK_ZONE)
                {

                    inst[i].operand = getBlockZoneOverride;
                    inst.InsertRange(i, new List<CodeInstruction>() {
                        new CodeInstruction(OpCodes.Ldarg_3),
                        new CodeInstruction(OpCodes.Ldarg_3),
                    });
                    i += 4;
                }
            }
            LogUtils.PrintMethodIL(inst);
            return inst;
        }



        public static float ConvertZoneToFloat(byte zone)
        {
            switch (zone & 0xf)
            {
                case 8:
                    return 1 + (zone & 0xf0);
                case 9:
                    return 3 + (zone & 0xf0);
                case 10:
                    return 2 + (zone & 0xf0);
                case 11:
                    return 4 + (zone & 0xf0);
                case 12:
                    return 7 + (zone & 0xf0);
                case 13:
                    return 5 + (zone & 0xf0);
                case 14:
                    return 6 + (zone & 0xf0);
                default:
                    return zone;
            }
        }


        public static float ConvertAngleToFloat(ref ZoneCell zone)
        {
            switch (zone.m_zone & 0xf)
            {
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                    return (zone.m_angle + 225) % 360;
                default:
                    return zone.m_angle;
            }
        }

        public static IEnumerable<CodeInstruction> TranspilePatchRefresh(IEnumerable<CodeInstruction> instructions)
        {
            var inst = new List<CodeInstruction>(instructions);
            for (int i = 1; i < inst.Count; i++)
            {
                if (inst[i - 1].operand == ZONE_FIELD && inst[i].opcode == OpCodes.Conv_R4)
                {
                    inst[i] = new CodeInstruction(OpCodes.Call, typeof(ZoneMixerOverrides).GetMethod("ConvertZoneToFloat"));
                }
                if (inst[i - 1].operand == ANGLE_FIELD && inst[i].opcode == OpCodes.Conv_R4)
                {
                    inst[i] = new CodeInstruction(OpCodes.Call, typeof(ZoneMixerOverrides).GetMethod("ConvertAngleToFloat"));
                    inst.RemoveAt(i - 1);
                }
            }
            LogUtils.PrintMethodIL(inst);
            return inst;
        }
        public static IEnumerable<CodeInstruction> TranspileCheckZoning(IEnumerable<CodeInstruction> instructions)
        {
            var inst = new List<CodeInstruction>(instructions);
            MethodInfo getBlockZoneOverride = typeof(ZoneMixerOverrides).GetMethod("GetBlockZoneOverride");
            for (int i = 5; i < inst.Count; i++)
            {
                if (inst[i].operand == GET_BLOCK_ZONE && inst[i].opcode == OpCodes.Call)
                {
                    inst[i].operand = getBlockZoneOverride;
                    inst.InsertRange(i, new List<CodeInstruction>() {
                        new CodeInstruction(OpCodes.Ldarg_1),
                        new CodeInstruction(OpCodes.Ldarg_2),
                    });
                    i += 4;
                }
            }
            LogUtils.PrintMethodIL(inst);
            return inst;
        }
        public static FieldInfo ZONE_FIELD = typeof(ZoneCell).GetField("m_zone");
        public static FieldInfo ANGLE_FIELD = typeof(ZoneCell).GetField("m_angle");
        public static MethodInfo GET_BLOCK_ZONE = typeof(ZoneBlock).GetMethod("GetZone");
    }

}
