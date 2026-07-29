using System;
using Celeste.Mod.Entities;
using Celeste.Mod.WrenbowHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.WrenbowHelper.Triggers;

[CustomEntity("WrenbowHelper/ScrambleRubiksCubeTrigger")]
public class ScrambleRubiksCubeTrigger : Trigger {
    private readonly string cubeName;

    private RubiksCube cube;
    private int scrambleLength;
    private bool doOnce;

    public ScrambleRubiksCubeTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        cubeName = data.String("cubeName", "");
        scrambleLength = data.Int("scrambleLength", 0);
        doOnce = data.Bool("doOnce", true);
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        cube = RubiksCube.Find(scene, cubeName);
    }

    public override void OnEnter(Player player)
    {
        if (cube is not null && !cube.state.IsLocked)
        {
            cube.RubiksScramble(scrambleLength);
            if (doOnce)
            {
                RemoveSelf();
            }
        }
    }
}