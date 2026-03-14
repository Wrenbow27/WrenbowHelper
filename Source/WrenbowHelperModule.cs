using System;

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
        Logger.SetLogLevel("WrenbowHelper", LogLevel.Info); //DEBUG LOGGING SETING DON"T KEEP THIS ENABLED
        DreamBlockLiftSpeedFix.Load();
        SetLiftSpeedOverride.Load();
        ExtraDreamJump.Load();
    }

    public override void Unload()
    {
        DreamBlockLiftSpeedFix.Unload();
        SetLiftSpeedOverride.Unload();
        ExtraDreamJump.Unload();
    }

    // Logical OR between menu setting and map setting
    public static bool DreamBlockLiftSpeedFixEnabled => Session.DreamBlockLiftSpeedFixOverride || Settings.DreamBlockLiftSpeedFixSetting;

    }
}