using System;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.WrenbowHelper.Triggers;

[CustomEntity("WrenbowHelper/TurnRubiksCubeTrigger")]
public class TurnRubiksCubeTrigger : Trigger {
    private readonly string face;
    private readonly int turnDepth;
    private readonly string direction;

    private readonly WrenbowHelper.Entities.RubiksCube.RubiksTurn turn;

    public TurnRubiksCubeTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        face = data.String("face", "Down");
        turnDepth = data.Int("turnDepth", 0);
        direction = data.String("direction", "Clockwise");

        turn = new WrenbowHelper.Entities.RubiksCube.RubiksTurn(
            Enum.Parse<WrenbowHelper.Entities.RubiksCube.CanonicalFaces>(face),
            Enum.Parse<WrenbowHelper.Entities.RubiksCube.TurnDir>(direction),
            turnDepth);
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);

        WrenbowHelper.Entities.RubiksCube cube = Scene.Tracker.GetEntity<WrenbowHelper.Entities.RubiksCube>();

        if (cube != null)
        {
            cube.DoTurn(turn);
        }
    }
}