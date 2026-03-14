using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.WrenbowHelper.Triggers;

[CustomEntity("WrenbowHelper/DreamBlockLiftSpeedFixTrigger")]
public class DreamBlockLiftSpeedFixTrigger : Trigger {
    private readonly bool value;

    public DreamBlockLiftSpeedFixTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        value = data.Bool("value", true);
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);

        WrenbowHelperModule.Session.DreamBlockLiftSpeedFixOverride = value;

        Logger.Log(LogLevel.Debug, "WrenbowHelper", $"DreamBlockLiftSpeedFixOverride set to {value}");
    }
}