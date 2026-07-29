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

        [DefaultButtonBinding(Buttons.RightShoulder, Keys.D)]
        public ButtonBinding RubiiksWatchtowerClockwiseTurn { get; set; }

        [DefaultButtonBinding(Buttons.RightShoulder, Keys.S)]
        public ButtonBinding RubiiksWatchtowerWiddershinsTurn { get; set; }

        [DefaultButtonBinding(Buttons.RightShoulder, Keys.A)]
        public ButtonBinding RubiiksWatchtowerDecrementLayer { get; set; }

        [DefaultButtonBinding(Buttons.RightShoulder, Keys.Z)]
        public ButtonBinding RubiiksWatchtowerIncrementLayer { get; set; }
    }
}