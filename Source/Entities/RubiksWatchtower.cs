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
        internal float Easer;
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
    private readonly string cubeLayout;
    private readonly string cubeName;

    private RubiksState state;
    private int size;
    private RubiksLogic.AbsoluteFaces? mySelectedFace;
    private RubiksLogic.AbsoluteFaces? currentFace;
    private RubiksLogic.AbsoluteFaces? pendingFace;
    private int mySelectedLayer = 0;
    private int currentLayer = 0;
    private int maxLayer;
    private Vector2 CameraTarget;
    public Sprite sprite;
    public TalkComponent talk;
    public Hud hud;
    public bool interacting;
    public string animPrefix = "";

    public RubiksWatchtower(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        cubeLayout = data.String("cubeLayouts", "NetL");
        cubeName = data.String("cubeName", "cube0");
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
        state = RubiksState.Find(cubeName);
        if (state is not null)
        {
            size = state.Size;
	        maxLayer = (int)(Math.Ceiling(size / 2.0) - 1);
        }
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
        /*
        Vector2 speed = Vector2.Zero;
		Vector2 lastDir = Vector2.Zero;
		Vector2 camStart = level.Camera.Position;
        Vector2 camStartCenter = camStart + new Vector2(160f, 90f);
		*/

        mySelectedFace = null;
		currentFace = null;
        mySelectedLayer = 0;
        state?.RubiksSelect(mySelectedLayer);

        while (!Input.MenuCancel.Pressed && interacting)
        {
			Vector2 target = CameraTarget - new Vector2(160, 90);
			target.X = Calc.Clamp( target.X, level.Bounds.Left, level.Bounds.Right - 320);
            target.Y = Calc.Clamp(target.Y, level.Bounds.Top, level.Bounds.Bottom - 180);

            level.Camera.Position = Calc.Approach( level.Camera.Position, target, 600f * Engine.DeltaTime);

            if (state is not null)
            {
				HandleInputs();
            }

            yield return null;
        }
        if (state is not null)
		{
            state.SelectedFace = null;
            state.SelectedLayer = null;
            state.SelectedDir = null;
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
		Scene.Remove (hud);
        interacting = false;
		player.StateMachine.State = 0;
		yield return null;
    }

    private void HandleInputs()
    {
        //Face selection inputs
        Vector2 aim = Input.Aim.Value;
        if (aim.X < 0)
        {
            if (aim.Y < 0)
            {
                switch (cubeLayout) //UpLeft
                {
                    case "NetL":
                        currentFace = (WrenbowHelperSettings.RubiksWatchtowerInvertDiagonals ?
                                        null:
                                        RubiksLogic.AbsoluteFaces.Back);
                        break;
                    case "NetR":
                        currentFace = (WrenbowHelperSettings.RubiksWatchtowerInvertDiagonals ?
                                        RubiksLogic.AbsoluteFaces.Front:
                                        null);
                        break;
                }
            }
            else if (aim.Y == 0){
				currentFace = RubiksLogic.AbsoluteFaces.Left;   //Left
            }
            else
            {
                switch (cubeLayout) //DownLeft
                {
                    case "NetL":
                        currentFace = (WrenbowHelperSettings.RubiksWatchtowerInvertDiagonals ?
                                        RubiksLogic.AbsoluteFaces.Back:
                                        null);
                        break;
                    case "NetR":
                        currentFace = (WrenbowHelperSettings.RubiksWatchtowerInvertDiagonals ?
                                        null:
                                        RubiksLogic.AbsoluteFaces.Front);
                        break;
                }
            }
        }
        else if (aim.X == 0)
        {
            if (aim.Y < 0)
			{
				currentFace = RubiksLogic.AbsoluteFaces.Up;   //Up
			}
            else if (aim.Y == 0)
            {
                currentFace = null;   //Neutral
			}
            else
            {
				currentFace = RubiksLogic.AbsoluteFaces.Down;   //Down
            }
        }
        else if (aim.X > 0)
        {
            if (aim.Y < 0)
            {
                switch (cubeLayout) //UpRight
                {
                    case "NetL":
                        currentFace = (WrenbowHelperSettings.RubiksWatchtowerInvertDiagonals ?
                                        RubiksLogic.AbsoluteFaces.Front:
                                        null);
                        break;
                    case "NetR":
                        currentFace = (WrenbowHelperSettings.RubiksWatchtowerInvertDiagonals ?
                                        null:
                                        RubiksLogic.AbsoluteFaces.Back);
                        break;
                }
            }
            else if (aim.Y == 0){
				currentFace = RubiksLogic.AbsoluteFaces.Right;   //Right
            }
            else
            {
                switch (cubeLayout) //DownRight
                {
                    case "NetL":
                        currentFace = (WrenbowHelperSettings.RubiksWatchtowerInvertDiagonals ?
                                        null:
                                        RubiksLogic.AbsoluteFaces.Front);
                        break;
                    case "NetR":
                        currentFace = (WrenbowHelperSettings.RubiksWatchtowerInvertDiagonals ?
                                        RubiksLogic.AbsoluteFaces.Back:
                                        null);
                        break;
                }
            }
        }

        if (mySelectedFace is null && currentFace is not null)
        {
        	//make "first" selection
            mySelectedFace = currentFace;
            state.RubiksSelect((RubiksLogic.AbsoluteFaces)mySelectedFace);
        }
        else if (currentFace is not null) //runs only with both non-null values for currentFace and mySelectedFace
        {
            if (currentFace != mySelectedFace)
            {
                if (currentFace != pendingFace)
                {
					pendingFace = currentFace;
                }
                else
                {
                    mySelectedFace = currentFace;
                    state.RubiksSelect((RubiksLogic.AbsoluteFaces)mySelectedFace);
                    pendingFace = null;
                }
            }
            else
            {
				pendingFace = null;
            }
        }

        // Layer selection inputs
        if (WrenbowHelperModule.Settings.RubiksWatchtowerIncrementLayer.Pressed)
        {
            currentLayer++;
        }
        else if (WrenbowHelperModule.Settings.RubiksWatchtowerDecrementLayer.Pressed)
        {
			currentLayer--;
        }
        if (currentLayer > maxLayer)
        {
			currentLayer = 0;
        }
        if (currentLayer < 0)
        {
			currentLayer = maxLayer;
        }
        if (currentLayer != mySelectedLayer)
        {
            mySelectedLayer = currentLayer;
            state.RubiksSelect(mySelectedLayer);
        }

        //turn inputs
        if (WrenbowHelperModule.Settings.RubiksWatchtowerClockwiseTurn.Pressed)
        {
            state.RubiksSelect(RubiksLogic.TurnDir.Clockwise);
            if (state.RubiksTurn())
            {
                Audio.Play("event:/WrenbowHelper/RubiksSFX", Position);
                state.RubiksSelect((RubiksLogic.AbsoluteFaces)mySelectedFace);
                state.RubiksSelect((int)mySelectedLayer);
            }
        }
        else if (WrenbowHelperModule.Settings.RubiksWatchtowerWiddershinsTurn.Pressed)
        {
			state.RubiksSelect(RubiksLogic.TurnDir.Widdershins);
            if (state.RubiksTurn())
            {
                Audio.Play("event:/WrenbowHelper/RubiksSFX", Position);
                state.RubiksSelect((RubiksLogic.AbsoluteFaces)mySelectedFace);
                state.RubiksSelect((int)mySelectedLayer);
            }
        }
    }
}