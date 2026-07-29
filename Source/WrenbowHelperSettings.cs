﻿using Microsoft.Xna.Framework.Input;

namespace Celeste.Mod.WrenbowHelper
{
    public class WrenbowHelperSettings : EverestModuleSettings {
        private bool dreamBlockLiftSpeedFix = false;

        [SettingName("wrenbowHelper_dreamblockliftspeedfix")]
        public bool DreamBlockLiftSpeedFixSetting
        {
            get => dreamBlockLiftSpeedFix;
            set
            {
                dreamBlockLiftSpeedFix = value;
                Logger.Log(LogLevel.Debug, "WrenbowHelper", $"Setting DreamBlockLiftSpeedFix to {value}");
            }
        }

        [SettingName("wrenbowHelper_rubiksColorBlindMode")]
        public static bool RubiksColorBlindMode { get; set; }

        [SettingName("wrenbowHelper_rubiksColorBlindMode")]
        public static bool RubiksWatchtowerInvertDiagonals { get; set; }

        [DefaultButtonBinding(Buttons.RightThumbstickRight, Keys.D)]
        public ButtonBinding RubiksWatchtowerClockwiseTurn { get; set; }

        [DefaultButtonBinding(Buttons.RightThumbstickLeft, Keys.S)]
        public ButtonBinding RubiksWatchtowerWiddershinsTurn { get; set; }

        [DefaultButtonBinding(Buttons.RightShoulder, Keys.A)]
        public ButtonBinding RubiksWatchtowerDecrementLayer { get; set; }

        [DefaultButtonBinding(Buttons.LeftShoulder, Keys.Z)]
        public ButtonBinding RubiksWatchtowerIncrementLayer { get; set; }
    }
}