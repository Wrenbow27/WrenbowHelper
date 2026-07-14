using System;
using Celeste.Mod.Entities;
using Celeste.Mod.WrenbowHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.WrenbowHelper.Triggers;

[CustomEntity("WrenbowHelper/TurnRubiksCubeTrigger")]
public class TurnRubiksCubeTrigger : Trigger {
    private readonly string face;
    private readonly int turnDepth;
    private readonly string direction;

    private readonly WrenbowHelper.RubiksLogic.RubiksTurn turn;

    public TurnRubiksCubeTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        face = data.String("face", "Down");
        turnDepth = data.Int("turnDepth", 0);
        direction = data.String("direction", "Clockwise");

        turn = new WrenbowHelper.RubiksLogic.RubiksTurn(
            Enum.Parse<WrenbowHelper.RubiksLogic.AbsoluteFaces>(face),
            Enum.Parse<WrenbowHelper.RubiksLogic.TurnDir>(direction),
            turnDepth);
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);

        WrenbowHelper.Entities.RubiksCube cube = Scene.Tracker.GetEntity<WrenbowHelper.Entities.RubiksCube>();

        if (cube != null)
        {
            cube?.RubiksTurn(turn, false);
        }
    }
}