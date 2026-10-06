using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace Celeste.Mod.WrenbowHelper {
    public class WrenbowHelperModule : EverestModule {

        public static WrenbowHelperModule Instance;

        public WrenbowHelperModule()
        {
            Instance = this;
        }

        public override Type SessionType => typeof(WrenbowHelperSession);
        public static WrenbowHelperSession Session => (WrenbowHelperSession) Instance._Session;
        public override Type SettingsType => typeof(WrenbowHelperSettings);
        public static WrenbowHelperSettings Settings => (WrenbowHelperSettings) Instance._Settings;

        public override void Load()
        {
            //Logger.SetLogLevel("WrenbowHelper", LogLevel.Debug); //DEBUG LOGGING SETING DON"T KEEP THIS ENABLED
            DreamBlockLiftSpeedFix.Load();
            SetLiftSpeedOverride.Load();
            ExtraDreamJump.Load();
            FeatherController.Load();
        }

        public override void Unload()
        {
            DreamBlockLiftSpeedFix.Unload();
            SetLiftSpeedOverride.Unload();
            ExtraDreamJump.Unload();
            FeatherController.Unload();
        }

        // Logical OR between menu setting and map setting
        public static bool DreamBlockLiftSpeedFixEnabled => Session.DreamBlockLiftSpeedFixOverride || Settings.DreamBlockLiftSpeedFixSetting;
    }
}