using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.WrenbowHelper.Entities;

[CustomEntity("WrenbowHelper/DreamJumpRefill")]

public class DreamJumpRefill : Entity {
    //lonn declared
    private readonly bool oneUse;
    private readonly bool persistent;

    //copied from vanilla
    private static readonly ParticleType P_Shatter = Refill.P_Shatter;
    private static readonly ParticleType P_Regen = Refill.P_Regen;
    private static readonly ParticleType P_Glow = Refill.P_Glow;

    private Sprite sprite;

    private Sprite flash;

    private Image outline;

    private Wiggler wiggler;

    private BloomPoint bloom;

    private VertexLight light;

    private Level level;

    private SineWave sine;

    private readonly ParticleType p_shatter;

    private readonly ParticleType p_regen;

    private readonly ParticleType p_glow;

    private float respawnTimer;

    public DreamJumpRefill(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        oneUse = data.Bool("oneUse", false);
        persistent = data.Bool("persistent", true);

        Collider = new Hitbox(16f, 16f, -8f, -8f);
        Add(new PlayerCollider(OnPlayer));
        string text = "objects/WrenbowHelper/DreamJumpRefill/";
        p_shatter = P_Shatter;
        p_regen = P_Regen;
        p_glow = P_Glow;

        Add(outline = new Image(GFX.Game[text + "outline"]));
        outline.CenterOrigin();
        outline.Visible = false;
        Add(sprite = new Sprite(GFX.Game, text + "idle"));
        sprite.AddLoop("idle", "", 0.1f);
        sprite.Play("idle");
        sprite.CenterOrigin();
        Add(flash = new Sprite(GFX.Game, text + "flash"));
        flash.Add("flash", "", 0.05f);
        flash.OnFinish = delegate
        {
            flash.Visible = false;
        };
        flash.CenterOrigin();
        Add(wiggler = Wiggler.Create(1f, 4f, (float v) =>
        {
            sprite.Scale = flash.Scale = Vector2.One * (1f + v * 0.2f);
        }));
        Add(new MirrorReflection());
        Add(bloom = new BloomPoint(0.8f, 16f));
        Add(light = new VertexLight(Color.White, 1f, 16, 48));
        Add(sine = new SineWave(0.6f, 0f));
        sine.Randomize();
        UpdateY();
        Depth = -100;
    }

    public DreamJumpRefill(Vector2 position)
        : this(new EntityData(), position)
    {
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        level = SceneAs<Level>();
    }

    public override void Update()
    {
        base.Update();
        if (respawnTimer > 0f)
        {
            respawnTimer -= Engine.DeltaTime;
            if (respawnTimer <= 0f)
            {
                Respawn();
            }
        }
        else if (Scene.OnInterval(0.1f))
        {
            level.ParticlesFG.Emit(p_glow, 1, Position, Vector2.One * 5f);
        }

        UpdateY();
        light.Alpha = Calc.Approach(light.Alpha, sprite.Visible ? 1f : 0f, 4f * Engine.DeltaTime);
        bloom.Alpha = light.Alpha * 0.8f;
        if (Scene.OnInterval(2f) && sprite.Visible)
        {
            flash.Play("flash", restart: true);
            flash.Visible = true;
        }
    }

    public void Respawn()
    {
        if (!Collidable)
        {
            Collidable = true;
            sprite.Visible = true;
            outline.Visible = false;
            Depth = -100;
            wiggler.Start();
            Audio.Play("event:/game/general/diamond_return", Position);
            level.ParticlesFG.Emit(p_regen, 16, Position, Vector2.One * 2f);
        }
    }
    public void UpdateY()
    {
        Sprite obj = flash;
        Sprite obj2 = sprite;
        float num = (bloom.Y = sine.Value * 2f);
        float y = (obj2.Y = num);
        obj.Y = y;
    }
    public override void Render()
    {
        if (sprite.Visible)
        {
            sprite.DrawOutline();
        }

        base.Render();
    }

    public void OnPlayer(Player player)
    {
        if (ExtraDreamJump.GiveDreamJump(player, persistent)) // <= return whether collected after check
        {
            Audio.Play("event:/game/general/diamond_touch", Position);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            Collidable = false;
            Add(new Coroutine(RefillRoutine(player)));
            respawnTimer = 2.5f;
        }
    }

    public IEnumerator RefillRoutine(Player player)
    {
        Celeste.Freeze(0.05f);
        yield return null;
        level.Shake();
        Sprite obj = sprite;
        Sprite obj2 = flash;
        bool visible = false;
        obj2.Visible = false;
        obj.Visible = visible;
        if (!oneUse)
        {
            outline.Visible = true;
        }

        Depth = 8999;
        yield return 0.05f;
        float num = player.Speed.Angle();
        level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, num - MathF.PI / 2f);
        level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, num + MathF.PI / 2f);
        SlashFx.Burst(Position, num);
        if (oneUse)
        {
            RemoveSelf();
        }
    }
}