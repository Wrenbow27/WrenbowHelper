namespace Celeste.Mod.WrenbowHelper {
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
    }
}