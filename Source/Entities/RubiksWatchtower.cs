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
    //lonn declared
    private readonly string cubeID;

    private RubiksCube cube;
    private Vector2 CameraTarget;
    public Sprite sprite;
    public TalkComponent talk;
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

        level.Camera.Position = CameraTarget - new Vector2(160, 90);
        
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

        Audio.Play ("event:/ui/game/lookout_on");

        while (!Input.MenuCancel.Pressed && !Input.MenuConfirm.Pressed && !Input.Dash.Pressed && !Input.Jump.Pressed && interacting)
        {
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