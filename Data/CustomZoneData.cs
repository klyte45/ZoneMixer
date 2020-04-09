using ColossalFramework;
using ColossalFramework.Globalization;
using Klyte.Commons.i18n;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using Klyte.ZoneMixer.Overrides;
using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Klyte.ZoneMixer.Data
{

    public class CustomZoneData : DataExtensorBase<CustomZoneData>
    {
        public static event Action EventAllChanged;
        public static event Action<int> EventOneChanged;

        private ZoneItem m_z1 = new ZoneItem(1, 0b00010100);
        private ZoneItem m_z2 = new ZoneItem(2, 0b00101000);
        private ZoneItem m_z3 = new ZoneItem(3, 0b00001100);
        private ZoneItem m_z4 = new ZoneItem(4, 0b00110000);
        private ZoneItem m_z5 = new ZoneItem(5, 0b10110000);
        private ZoneItem m_z6 = new ZoneItem(6, 0b11000000);
        private ZoneItem m_z7 = new ZoneItem(7, 0b01110000);

        public void Reset()
        {
            Z1 = new ZoneItem(1, 0b00010100);
            Z2 = new ZoneItem(2, 0b00101000);
            Z3 = new ZoneItem(3, 0b00001100);
            Z4 = new ZoneItem(4, 0b00110000);
            Z5 = new ZoneItem(5, 0b10110000);
            Z6 = new ZoneItem(6, 0b11000000);
            Z7 = new ZoneItem(7, 0b01110000);
            EventAllChanged?.Invoke();
        }

        public void SaveAsDefault() => File.WriteAllBytes(ZMController.DEFAULT_CONFIG_FILE, Serialize());
        public override void LoadDefaults()
        {
            if (File.Exists(ZMController.DEFAULT_CONFIG_FILE))
            {
                try
                {
                    if (Deserialize(typeof(CustomZoneData), File.ReadAllBytes(ZMController.DEFAULT_CONFIG_FILE)) is CustomZoneData defaultData)
                    {
                        Z1 = defaultData.Z1;
                        Z2 = defaultData.Z2;
                        Z3 = defaultData.Z3;
                        Z4 = defaultData.Z4;
                        Z5 = defaultData.Z5;
                        Z6 = defaultData.Z6;
                        Z7 = defaultData.Z7;
                        EventAllChanged?.Invoke();
                    }
                }
                catch (Exception e)
                {
                    LogUtils.DoErrorLog($"EXCEPTION WHILE LOADING: {e.GetType()} - {e.Message}\n {e.StackTrace}");

                    K45DialogControl.ShowModal(new K45DialogControl.BindProperties()
                    {
                        icon = ZoneMixerMod.Instance.IconName,
                        title = Locale.Get("K45_ZM_ERROR_LOADING_DEFAULTS_TITLE"),
                        message = string.Format(Locale.Get("K45_ZM_ERROR_LOADING_DEFAULTS_MESSAGE"), ZMController.DEFAULT_CONFIG_FILE, e.GetType(), e.Message, e.StackTrace),
                        showButton1 = true,
                        textButton1 = Locale.Get("K45_ZM_OK_BUTTON"),
                        showButton2 = true,
                        textButton2 = Locale.Get("K45_ZM_OPEN_FOLDER_ON_EXPLORER_BUTTON"),
                        showButton3 = true,
                        textButton3 = Locale.Get("K45_ZM_GO_TO_MOD_PAGE_BUTTON"),
                        useFullWindowWidth = true
                    }, (x) =>
                    {
                        if (x == 2)
                        {
                            ColossalFramework.Utils.OpenInFileBrowser(ZMController.FOLDER_PATH);
                            return false;
                        }
                        else if (x == 3)
                        {
                            ColossalFramework.Utils.OpenUrlThreaded("https://steamcommunity.com/sharedfiles/filedetails/?id=" );
                            return false;
                        }
                        return true;
                    });

                }
            }
        }

        [XmlElement] public ZoneItem Z1 { get => m_z1; set => SetZ(ref m_z1, value ?? m_z1); }
        [XmlElement] public ZoneItem Z2 { get => m_z2; set => SetZ(ref m_z2, value ?? m_z2); }
        [XmlElement] public ZoneItem Z3 { get => m_z3; set => SetZ(ref m_z3, value ?? m_z3); }
        [XmlElement] public ZoneItem Z4 { get => m_z4; set => SetZ(ref m_z4, value ?? m_z4); }
        [XmlElement] public ZoneItem Z5 { get => m_z5; set => SetZ(ref m_z5, value ?? m_z5); }
        [XmlElement] public ZoneItem Z6 { get => m_z6; set => SetZ(ref m_z6, value ?? m_z6); }
        [XmlElement] public ZoneItem Z7 { get => m_z7; set => SetZ(ref m_z7, value ?? m_z7); }

        public ZoneItem this[int idx] => (idx & 7) switch
        {
            1 => Z1,
            2 => Z2,
            3 => Z3,
            4 => Z4,
            5 => Z5,
            6 => Z6,
            7 => Z7,
            _ => default
        };
        public ZoneItem this[ItemClass.Zone idx] => this[(int)(idx - 7)];

        public override string SaveId => "K45_ZM_CustomZoneData";

        private void SetZ(ref ZoneItem field, ZoneItem value)
        {
            field = value;
            field.UpdateZoneName();
            field.UpdateZoneConfig();
        }
        public class ZoneItem
        {
            public ZoneItem() { }
            public ZoneItem(int zoneNumber, byte config)
            {
                m_zoneNumber = zoneNumber;
                ZoneConfig = config;
                ZoneName = null;
            }

            [XmlAttribute("zoneNumber")]
            public int m_zoneNumber;

            private string m_zoneName;
            private byte m_zoneConfig;

            [XmlAttribute("name")]
            public string ZoneName
            {
                get => m_zoneName.IsNullOrWhiteSpace() ? string.Format(Locale.Get("K45_ZM_DEFAULT_ZONE_TITLE"), m_zoneNumber) : m_zoneName;

                set {
                    m_zoneName = value;
                    UpdateZoneName();
                    EventOneChanged?.Invoke(m_zoneNumber);
                }
            }

            public void UpdateZoneName()
            {
                KlyteLocaleManager.SetLocaleEntry(new ColossalFramework.Globalization.Locale.Key
                {
                    m_Identifier = "ZONING_TITLE",
                    m_Key = "Z" + m_zoneNumber
                }, m_zoneName.IsNullOrWhiteSpace() ? string.Format(Locale.Get("K45_ZM_DEFAULT_ZONE_TITLE"), m_zoneNumber) : m_zoneName);
            }

            [XmlAttribute("zoneConfig")]
            public byte ZoneConfig
            {
                get => m_zoneConfig;
                set {
                    m_zoneConfig = value;
                    UpdateZoneConfig();
                    EventOneChanged?.Invoke(m_zoneNumber);
                }
            }

            public void UpdateZoneConfig()
            {
                KlyteLocaleManager.SetLocaleEntry(new ColossalFramework.Globalization.Locale.Key
                {
                    m_Identifier = "ZONING_DESC",
                    m_Key = "Z" + m_zoneNumber
                }, string.Format(Locale.Get("K45_ZM_ZONE_DESC_Z" + m_zoneNumber) + GetGenerationString()));
            }

            public void AddToZone(ItemClass.Zone zone) => ZoneConfig |= (byte)(1 << (int)zone);
            public void RemoveFromZone(ItemClass.Zone zone) => ZoneConfig &= (byte)~(1 << (int)zone);
            public bool HasZone(ItemClass.Zone zone) => (ZoneConfig & (1 << (int)zone)) != 0;

            private string GetGenerationString() => string.Join("\n", ZoneMixerOverrides.ZONES_TO_CHECK.Where(x => HasZone(x)).Select(x => $"\t• {Locale.Get("ZONEDBUILDING_TITLE", x.ToString())}").ToArray());
        }
    }

}
