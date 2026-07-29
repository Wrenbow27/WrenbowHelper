using System;
using Celeste.Mod.Entities;
using Celeste.Mod.WrenbowHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.WrenbowHelper.Triggers;

[CustomEntity("WrenbowHelper/TurnRubiksCubeTrigger")]
public class TurnRubiksCubeTrigger : Trigger {
    private readonly string face;
    private readonly int layer;
    private readonly string direction;
    private readonly string operation;
    private readonly string cubeName;

    private RubiksState state;

    private WrenbowHelper.RubiksLogic.RubiksTurn turn;

    public TurnRubiksCubeTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        face = data.String("face", "Down");
        layer = data.Int("layer", 0);
        direction = data.String("direction", "Clockwise");
        operation = data.String("operation", "All");
        cubeName = data.String("cubeName", "cube0");

        turn = new WrenbowHelper.RubiksLogic.RubiksTurn(
            Enum.Parse<WrenbowHelper.RubiksLogic.AbsoluteFaces>(face),
            Enum.Parse<WrenbowHelper.RubiksLogic.TurnDir>(direction),
            layer);
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        state = RubiksState.Find(cubeName);
    }

    public override void OnEnter(Player player)
    {
        switch (operation)
        {
            case "All":
                turn = new WrenbowHelper.RubiksLogic.RubiksTurn(
                    Enum.Parse<WrenbowHelper.RubiksLogic.AbsoluteFaces>(face),
                    Enum.Parse<WrenbowHelper.RubiksLogic.TurnDir>(direction),
                    layer);
                if (state?.RubiksTurn(turn) ?? false)
                {
                    Audio.Play("event:/WrenbowHelper/RubiksSFX", Position);
                }
                return;

            case "Select Face":
                state?.RubiksSelect(Enum.Parse<WrenbowHelper.RubiksLogic.AbsoluteFaces>(face));
                return;

            case "Select Layer":
                state?.RubiksSelect(layer);
                return;

            case "Select Direction":
                state?.RubiksSelect(Enum.Parse<WrenbowHelper.RubiksLogic.TurnDir>(direction));
                return;

            case "Turn Selection":
                if (state?.RubiksTurn() ?? false)
                {
                    Audio.Play("event:/WrenbowHelper/RubiksSFX", Position);
                }
                return;
        }
    }
}