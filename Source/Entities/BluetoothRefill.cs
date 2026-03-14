using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.WrenbowHelper.Entities;

[CustomEntity("WrenbowHelper/BluetoothRefill")]

public class BluetoothRefill : Entity {
    //lonn declared
    private readonly string spritePath;
    private readonly Color spriteColor;
    private readonly float respawnTime;
    private readonly float freeze;
    private readonly string collectSound;
    private readonly bool playerCollect;
    private readonly bool holdableCollect;
    private readonly bool overrideRequirements;
    private readonly bool oneUse;
    private readonly bool twoDashes;

    //copied from vanilla
    private static readonly ParticleType P_Shatter = Refill.P_Shatter;
    private static readonly ParticleType P_Regen = Refill.P_Regen;
    private static readonly ParticleType P_Glow = Refill.P_Glow;

    private static readonly ParticleType P_ShatterTwo = Refill.P_ShatterTwo;
    private static readonly ParticleType P_RegenTwo = Refill.P_RegenTwo;
    private static readonly ParticleType P_GlowTwo = Refill.P_GlowTwo;

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

    public BluetoothRefill(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        
        
        spritePath = data.String("spritePath", "");
        if (!string.IsNullOrEmpty(spritePath) && !spritePath.EndsWith("/"))
        {
            spritePath += "/";
        }
        spriteColor = data.HexColor("spriteColor", Color.White);
        respawnTime = data.Float("respawnTime", 2f);
        freeze = data.Int("freezeFrames", 3)/60f;
        collectSound = data.Attr("collectSound", "event:/game/general/diamond_touch");
        playerCollect = data.Bool("playerCollect", true);
        holdableCollect = data.Bool("holdableCollect", false);
        overrideRequirements = data.Bool("overrideRequirements", false);
        oneUse = data.Bool("oneUse", false);
        twoDashes = data.Bool("twoDashes", false);
        string text;

        Collider = new Hitbox(16f, 16f, -8f, -8f);
        if (playerCollect)
        {
            Add(new PlayerCollider(OnPlayer));
        }
        if (holdableCollect)
        {
            Add(new HoldableCollider(OnHoldable));
        }

        if (twoDashes)
        {
            text = "objects/WrenbowHelper/BlueTwothRefill/";
            p_shatter = P_ShatterTwo;
            p_regen = P_RegenTwo;
            p_glow = P_GlowTwo;
        }
        else
        {
            text = "objects/WrenbowHelper/BluetoothRefill/";
            p_shatter = P_Shatter;
            p_regen = P_Regen;
            p_glow = P_Glow;
        }

        
        outline = GFX.Game.Has(spritePath + "outline") ?  new Image(GFX.Game[spritePath + "outline"]) : new Image(GFX.Game[text + "outline"]);
        sprite = GFX.Game.Has(spritePath + "idle00") ?  new Sprite(GFX.Game, spritePath + "idle") : new Sprite(GFX.Game, text + "idle");
        flash = GFX.Game.Has(spritePath + "flash00") ?  new Sprite(GFX.Game, spritePath + "flash") : new Sprite(GFX.Game, text + "flash");
        
        Add(outline);
        outline.CenterOrigin();
        outline.Visible = false;
        Add(sprite);
        sprite.AddLoop("idle", "", 0.1f);
        sprite.Play("idle");
        sprite.CenterOrigin();
        Add(flash);
        flash.Add("flash", "", 0.05f);
        flash.OnFinish = delegate
        {
            flash.Visible = false;
        };
        sprite.Color = spriteColor;
        //outline.Color = spriteColor;
        flash.Color = spriteColor;
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

    public BluetoothRefill(Vector2 position)
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
            Audio.Play(twoDashes ? "event:/new_content/game/10_farewell/pinkdiamond_return" : "event:/game/general/diamond_return", Position);
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
        if (player.UseRefill(twoDashes) || overrideRequirements)
        {
            Audio.Play(collectSound, Position);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            Collidable = false;
            Add(new Coroutine(RefillRoutine(player)));
            respawnTimer = respawnTime;
        }
    }

    public void OnHoldable(Holdable holdable)
    {
        Player player = Scene.Tracker.GetEntity<Player>();
        if (player == null) 
            return;

        if (player.UseRefill(twoDashes) || overrideRequirements)
        {
            Audio.Play(collectSound, Position);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            Collidable = false;
            Add(new Coroutine(RefillRoutine(player)));
            respawnTimer = respawnTime;
        }
    }

    public IEnumerator RefillRoutine(Player player)
    {
        Celeste.Freeze(freeze);
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