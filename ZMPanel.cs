using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensors;
using Klyte.Commons.Interfaces;
using Klyte.Commons.UI.SpriteNames;
using Klyte.Commons.Utils;
using Klyte.ZoneMixer.Data;
using Klyte.ZoneMixer.Overrides;
using UnityEngine;

namespace Klyte.ZoneMixer
{
    public class ZMPanel : BasicKPanel<ZoneMixerMod, ZMController, ZMPanel>
    {
        public override float PanelWidth { get; } = 600;

        public override float PanelHeight { get; } = 410;
        public object ClassesData { get; private set; }

        protected override void AwakeActions()
        {
            CreateTopButton(MainPanel, "ExportAsDefault", "K45_ZM_EXPORT_DEFAULT_BTN", CommonsSpriteNames.K45_Save.ToString(), new Vector2(10, 50), (x, y) => CustomZoneData.Instance.SaveAsDefault());
            CreateTopButton(MainPanel, "ImportDefault", "K45_ZM_IMPORT_DEFAULT_BTN", CommonsSpriteNames.K45_Load.ToString(), new Vector2(95, 50), (x, y) => CustomZoneData.Instance.LoadDefaults());
            CreateTopButton(MainPanel, "Reset", "K45_ZM_RESET_BTN", CommonsSpriteNames.K45_Reload.ToString(), new Vector2(180, 50), (x, y) => CustomZoneData.Instance.Reset());

            KlyteMonoUtils.CreateUIElement(out UIPanel listPanel, MainPanel.transform, "ZoneList", new Vector4(0, 100, PanelWidth, PanelHeight - 105));
            listPanel.autoLayout = true;
            listPanel.autoLayoutDirection = LayoutDirection.Vertical;
            listPanel.autoLayoutPadding = new RectOffset(0, 0, 5, 5);
            listPanel.padding = new RectOffset(5, 5, 5, 5);

            for (int i = 1; i <= 7; i++)
            {
                int idx = i;

                KlyteMonoUtils.CreateUIElement(out UIPanel zonePanel, listPanel.transform, "ConfigZ" + i);
                zonePanel.autoLayout = true;
                zonePanel.autoLayoutDirection = LayoutDirection.Horizontal;
                zonePanel.autoLayoutPadding = new RectOffset(5, 5, 0, 0);
                zonePanel.wrapLayout = false;
                zonePanel.autoFitChildrenHorizontally = true;
                zonePanel.autoFitChildrenVertically = true;

                var uiHelperExt = new UIHelperExtension(zonePanel);
                uiHelperExt.AddUiSprite("ZoningZ" + idx, UIView.GetAView().defaultAtlas).size = new Vector2(40, 32);
                UITextField nameInput;
                nameInput = CreateMiniTextField(zonePanel, CustomZoneData.Instance[idx].ZoneName, "ConfigZ" + idx);
                nameInput.eventTextSubmitted += (x, y) =>
                {
                    if (!m_loading)
                    {
                        CustomZoneData.Instance[idx].ZoneName = y;
                        nameInput.text = CustomZoneData.Instance[idx].ZoneName;
                    }
                };
                CustomZoneData.EventOneChanged += (x) =>
                {
                    if (x == idx)
                    {
                        nameInput.text = CustomZoneData.Instance[idx].ZoneName;
                    }
                };
                CustomZoneData.EventAllChanged += () => nameInput.text = CustomZoneData.Instance[idx].ZoneName;


                foreach (ItemClass.Zone zone in ZoneMixerOverrides.ZONES_TO_CHECK)
                {
                    AddCheckboxZone(idx, uiHelperExt, zone);
                }
            }


            CustomZoneData.EventAllChanged += () =>
            {
                FindObjectOfType<ZoningPanel>()?.RefreshPanel();

            };
            CustomZoneData.EventOneChanged += (x) =>
            {
                FindObjectOfType<ZoningPanel>()?.RefreshPanel();

            };
        }

        private void AddCheckboxZone(int idx, UIHelperExtension uiHelperExt, ItemClass.Zone zone)
        {
            UITextureAtlas thumbAtlas = FindObjectOfType<ZoningPanel>().GetComponentInChildren<UIScrollablePanel>().atlas;
            UICheckBox checkLR = uiHelperExt.AddCheckboxNoLabel($"ConfigZ{idx}{zone}");
            var checkedObj = (UISprite)checkLR.checkedBoxObject;
            UISprite uncheckedObj = checkLR.Find<UISprite>("Unchecked");
            checkedObj.spriteName = $"Zoning{zone}";
            uncheckedObj.spriteName = $"Zoning{zone}Disabled";
            checkedObj.atlas = thumbAtlas;
            uncheckedObj.atlas = thumbAtlas;
            checkLR.size = new Vector2(47, 32);
            checkedObj.size = new Vector2(47, 32);
            uncheckedObj.size = new Vector2(47, 32);
            checkedObj.relativePosition = default;
            uncheckedObj.relativePosition = default;

            checkLR.tooltip = Locale.Get("ZONEDBUILDING_TITLE", zone.ToString());

            checkLR.isChecked = CustomZoneData.Instance[idx].HasZone(zone);
            checkLR.eventCheckChanged += (x, y) =>
            {
                if (!m_loading)
                {
                    if (y)
                    {
                        CustomZoneData.Instance[idx].AddToZone(zone);
                    }
                    else
                    {
                        CustomZoneData.Instance[idx].RemoveFromZone(zone);
                    }
                }
            };

            CustomZoneData.EventOneChanged += (x) =>
            {
                if (x == idx)
                {
                    try
                    {
                        m_loading = true;
                        checkLR.isChecked = CustomZoneData.Instance[idx].HasZone(zone);
                    }
                    finally
                    {
                        m_loading = false;
                    }
                }
            };

            CustomZoneData.EventAllChanged += () =>
            {
                try
                {
                    m_loading = true;
                    checkLR.isChecked = CustomZoneData.Instance[idx].HasZone(zone);
                }
                finally
                {
                    m_loading = false;
                }
            };
        }

        private bool m_loading = false;

        private UITextField CreateMiniTextField(UIComponent parent, string initVal, string name)
        {
            UITextField ddObj = UIHelperExtension.AddTextfield(parent, null, initVal, out UILabel label, out UIPanel container);
            container.autoFitChildrenHorizontally = false;
            container.autoLayoutDirection = LayoutDirection.Horizontal;
            container.autoLayout = true;
            container.autoFitChildrenHorizontally = true;
            container.autoFitChildrenVertically = true;
            container.padding = new RectOffset(0, 0, 3, 3);

            ddObj.isLocalized = false;
            ddObj.autoSize = false;
            ddObj.name = name;
            ddObj.size = new Vector3(190, 22);
            ddObj.textScale = 1;


            return ddObj;
        }

        private static void CreateTopButton(UIPanel _mainPanel, string name, string tooltipLocale, string sprite, Vector2 position, MouseEventHandler onClicked)
        {
            KlyteMonoUtils.CreateUIElement(out UIButton button, _mainPanel.transform, name, new Vector4(10, 50, 40, 40));
            KlyteMonoUtils.InitButtonFull(button, false, "OptionBase");
            button.focusedBgSprite = "";
            button.normalFgSprite = sprite;
            button.relativePosition = position;
            button.tooltipLocaleID = tooltipLocale;
            button.eventClicked += onClicked;
            button.scaleFactor = .6f;
        }

    }
}