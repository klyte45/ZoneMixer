using ColossalFramework.UI;
using Klyte.Commons.Interfaces;
using Klyte.Commons.UI.SpriteNames;
using Klyte.Commons.Utils;
using Klyte.ZoneMixer.Data;
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

            CustomZoneData.EventAllChanged += () =>
            {
                FindObjectOfType<ZoningPanel>()?.RefreshPanel();

            };
            CustomZoneData.EventAllChanged += () =>
            {
                FindObjectOfType<ZoningPanel>()?.RefreshPanel();

            };
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