using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.WrenbowHelper.Entities;

[CustomEntity("WrenbowHelper/RubiksWatchtower")]
[Tracked]

public class RubiksWatchtower : Entity {

    public class Hud : Entity
    {
        public float Easer;
        public Hud ()
		{
            AddTag(Tags.HUD);
        }
        public override void Render()
        {
            Level level = base.Scene as Level;
			float num = Ease.CubeInOut (Easer);
			Color val = Color.White * num;
			int num2 = (int)(80f * num);
			int num3 = (int)(80f * num * 0.5625f);
			int num4 = 8;
			if (level.FrozenOrPaused || level.RetryPlayerCorpse != null) {
				val *= 0.25f;
			}
			Draw.Rect (num2, num3, 1920 - num2 * 2 - num4, num4, val);
			Draw.Rect (num2, num3 + num4, num4 + 2, 1080 - num3 * 2 - num4, val);
			Draw.Rect (1920 - num2 - num4 - 2, num3, num4 + 2, 1080 - num3 * 2 - num4, val);
			Draw.Rect (num2 + num4, 1080 - num3 - num4, 1920 - num2 * 2 - num4, num4, val);
			if (level.FrozenOrPaused || level.RetryPlayerCorpse != null) {
				return;
			}
        }
    }

    //lonn declared
    private readonly string cubeID;

    private RubiksCube cube;
    private Vector2 CameraTarget;
    public Sprite sprite;
    public TalkComponent talk;
    public Hud hud;
    public bool interacting;
    public string animPrefix = "";

    public RubiksWatchtower(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        cubeID = data.String("cubeID", "Cube0");
        base.Depth = -8500;
		Add (talk = new TalkComponent (new Rectangle (-24, -8, 48, 8), new Vector2 (-0.5f, -20f), Interact));
        talk.PlayerMustBeFacing = false;
        base.Collider = new Hitbox(4f, 4f, -2f, -4f);
        Add(sprite = GFX.SpriteBank.Create("lookout"));

        CameraTarget = data.NodesOffset (offset)[0];
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        cube = RubiksCube.Find(scene, cubeID);
    }

    public override void Removed (Scene scene)
	{
		base.Removed (scene);
		if (interacting) {
			Player entity = scene.Tracker.GetEntity<Player> ();
			if (entity != null) {
				entity.StateMachine.State = 0;
			}
		}
	}

    public void Interact (Player player)
    {
        if (player.DefaultSpriteMode == PlayerSpriteMode.MadelineAsBadeline || SaveData.Instance.Assists.PlayAsBadeline) {
			animPrefix = "badeline_";
		} else if (player.DefaultSpriteMode == PlayerSpriteMode.MadelineNoBackpack) {
			animPrefix = "nobackpack_";
		} else {
			animPrefix = "";
		}
		Coroutine coroutine = new Coroutine (LookRoutine (player));
		coroutine.RemoveOnComplete = true;
		Add (coroutine);
		interacting = true;
    }

    public void StopInteracting ()
	{
		interacting = false;
		sprite.Play (animPrefix + "idle");
    }

    public override void Update()
    {
        if (talk.UI != null) {
			talk.UI.Visible = !CollideCheck<Solid> ();
		}
		base.Update ();
		Player entity = base.Scene.Tracker.GetEntity<Player> ();
		if (entity != null) {
			sprite.Active = interacting || entity.StateMachine.State != 11;
			if (!sprite.Active) {
				sprite.SetAnimationFrame (0);
			}
		}
    }

    private IEnumerator LookRoutine(Player player)
    {
        Level level = SceneAs<Level> ();
		SandwichLava sandwichLava = Scene.Entities.FindFirst<SandwichLava> ();
		if (sandwichLava != null) {
			sandwichLava.Waiting = true;
		}
		if (player.Holding != null) {
			player.Drop ();
		}
		player.StateMachine.State = 11;
        yield return player.DummyWalkToExact((int)X, walkBackwards: false, 1f, cancelOnFall: true);

        //level.Camera.Position = CameraTarget - new Vector2(160, 90);

        if (Math.Abs (X - player.X) > 4f || player.Dead || !player.OnGround ()) {
			if (!player.Dead) {
				player.StateMachine.State = 0;
			}
			yield break;
        }
        Audio.Play("event:/game/general/lookout_use", Position);
        if (player.Facing == Facings.Right) {
			sprite.Play (animPrefix + "lookRight");
		} else {
			sprite.Play (animPrefix + "lookLeft");
		}
        PlayerSprite playerSprite = player.Sprite;
		PlayerHair hair = player.Hair;
		bool visible = false;
		hair.Visible = false;
		playerSprite.Visible = visible;
        yield return 0.2f;
        Scene.Add(hud = new Hud());

        Audio.Play ("event:/ui/game/lookout_on");
		while ((hud.Easer = Calc.Approach (hud.Easer, 1f, Engine.DeltaTime * 3f)) < 1f) {
			level.ScreenPadding = (int)(Ease.CubeInOut (hud.Easer) * 16f);
			yield return null;
		}
		Vector2 cam = level.Camera.Position;
		Vector2 speed = Vector2.Zero;
		Vector2 lastDir = Vector2.Zero;
		Vector2 camStart = level.Camera.Position;
		Vector2 camStartCenter = camStart + new Vector2 (160f, 90f);

        while (!Input.MenuCancel.Pressed && !Input.MenuConfirm.Pressed && !Input.Dash.Pressed && !Input.Jump.Pressed && interacting)
        {
			Vector2 target = CameraTarget - new Vector2(160, 90);
			target.X = Calc.Clamp( target.X, level.Bounds.Left, level.Bounds.Right - 320);
            target.Y = Calc.Clamp(target.Y, level.Bounds.Top, level.Bounds.Bottom - 180);

            level.Camera.Position = Calc.Approach( level.Camera.Position, target, 600f * Engine.DeltaTime);

            HandleInputs();

            yield return null;
        }

        PlayerSprite playerSprite2 = player.Sprite;
		PlayerHair hair2 = player.Hair;
		visible = true;
		hair2.Visible = true;
		playerSprite2.Visible = visible;
		sprite.Play (animPrefix + "idle");
        Audio.Play("event:/ui/game/lookout_off");
		while ((hud.Easer = Calc.Approach (hud.Easer, 0f, Engine.DeltaTime * 3f)) > 0f) {
			level.ScreenPadding = (int)(Ease.CubeInOut (hud.Easer) * 16f);
			yield return null;
        }

        Audio.SetMusicParam ("escape", 0f);
		level.ScreenPadding = 0f;
        level.ZoomSnap(Vector2.Zero, 1f);

        interacting = false;
		player.StateMachine.State = 0;
		yield return null;
    }

    private void HandleInputs()
    {

    }
}