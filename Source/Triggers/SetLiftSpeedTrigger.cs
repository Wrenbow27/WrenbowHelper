using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.WrenbowHelper.Triggers;

[CustomEntity("WrenbowHelper/SetLiftSpeedTrigger")]
public class SetLiftSpeedTrigger : Trigger {
    private readonly float liftSpeedX;
    private readonly float liftSpeedY;
    private Vector2 myLiftSpeed => new Vector2(liftSpeedX, liftSpeedY);
    private readonly bool onlyOnce;
    private readonly bool sustainedOverride;
    private readonly string flag;

    private bool isEnabled(Level level)
    {
        if (level == null) return false;
        return string.IsNullOrEmpty(flag) || level.Session.GetFlag(flag);
    }

    public SetLiftSpeedTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        liftSpeedX = data.Float("liftSpeedX", 0.0f);
        liftSpeedY = data.Float("liftSpeedY", 0.0f);
        onlyOnce = data.Bool("onlyOnce", false);
        sustainedOverride = data.Bool("sustainedOverride", true);
        flag = data.Attr("flag", "");
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        Level level = SceneAs<Level>();

        if (isEnabled(level))
        {
            SetLiftSpeed(player);
        }
    }

    public override void OnStay(Player player)
    {
        base.OnStay(player);
        Level level = SceneAs<Level>();
        if (!onlyOnce && isEnabled(level))
        {
            SetLiftSpeed(player);
        }
    }

    private void SetLiftSpeed(Player player)
    {
        SetLiftSpeedOverride.SetLift(player, myLiftSpeed, sustainedOverride);

        if (onlyOnce)
        {
            RemoveSelf();
        }
    }
}