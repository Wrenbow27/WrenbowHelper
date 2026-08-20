using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace Celeste.Mod.WrenbowHelper
{
    [CustomEntity("WrenbowHelper/FeatherController")]
    [Tracked]
    class FeatherController : Entity
    {
        private readonly float myFlyTime;
        private readonly float myHighSpeed;
        private readonly float myTurnSpeed;
        private readonly float myLowSpeed;
        private readonly float myTurnRate;
        private readonly bool octalAnalog;
        private readonly bool fuper;
        private readonly bool fyper;
        private readonly bool founce;
        private readonly bool noStartSlowdown;
        private readonly bool noFeatherBoost;
        private readonly Color trailColor;

        private static ILHook starFlyCoroutineHooks;

        public FeatherController(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            myFlyTime = data.Float("flyTime", 2f);
            myHighSpeed = data.Float("highFlySpeed", 190.0f);
            myTurnSpeed = data.Float("turnFlySpeed", 140.0f);
            myLowSpeed = data.Float("lowFlySpeed", 91.0f);
            myTurnRate = data.Float("turnRateMultiplier", 1.0f);
            octalAnalog = data.Bool("octalAnalog", false);
            fuper = data.Bool("featherHyper", false);
            fyper = data.Bool("trueFeatherSuper", false);
            founce = data.Bool("trueFeatherWallBounce", false);
            noStartSlowdown = data.Bool("noStartSlowdown", false);
            noFeatherBoost = data.Bool("noFeatherBoost", false);
            trailColor = data.HexColor("trailColor", Calc.HexToColor("ffd65c"));
        }

        public static void Load()
        {
            On.Celeste.Player.StartStarFly += Player_StartStarFly;
            On.Celeste.Player.StarFlyBegin += Player_StarFlyBegin;
            On.Celeste.Player.StarFlyUpdate += Player_StarFlyUpdate;

            starFlyCoroutineHooks = new ILHook(typeof(Player)
                                            .GetMethod("StarFlyCoroutine",
                                                        BindingFlags.NonPublic | BindingFlags.Instance
                                            ).GetStateMachineTarget(),
                                            StarFlyCoroutineIL
            );
        }

        public static void Unload()
        {
            On.Celeste.Player.StartStarFly -= Player_StartStarFly;
            On.Celeste.Player.StarFlyBegin -= Player_StarFlyBegin;
            On.Celeste.Player.StarFlyUpdate -= Player_StarFlyUpdate;

            starFlyCoroutineHooks?.Dispose();
            starFlyCoroutineHooks = null;
        }

        //Runs when colliding with a feather entity. Only really used to expose the fly time from flying through feathers in StStarFly as a refresh
        private static bool Player_StartStarFly(On.Celeste.Player.orig_StartStarFly orig, Player player)
        {
            FeatherController controller = GetController();
            bool result = orig(player);
            if (controller is not null)
            {
                player.starFlyTimer = controller.myFlyTime;
                player.Sprite.Color = controller.trailColor;
            }
            return result;
        }

        private static void Player_StarFlyBegin(On.Celeste.Player.orig_StarFlyBegin orig, Player player)
        {
            FeatherController controller = GetController();
            orig(player);
            if (controller is not null)
            {
                player.starFlyTimer = controller.myFlyTime;
            }
            return;
        }

        //Pretty much entirely copied fom ILSpy. Fingers crossed it works
        private static int Player_StarFlyUpdate(On.Celeste.Player.orig_StarFlyUpdate orig, Player player)
        {
            FeatherController controller = GetController();
            if (controller is null)
            {
                return orig(player);
            }
            player.starFlyBloom.Alpha = Calc.Approach(player.starFlyBloom.Alpha, 0.7f, Engine.DeltaTime * 2f);
            Input.Rumble(RumbleStrength.Climb, RumbleLength.Short);
            if (player.starFlyTransforming && !controller.noStartSlowdown)
            {
                player.Speed = Calc.Approach(player.Speed, Vector2.Zero, 1000f * Engine.DeltaTime);
            }
            else
            {
                Vector2 aim = OctalSnapping(Input.Feather.Value); //controller condition checked inside
                bool slow = false;
                if (aim == Vector2.Zero)
                {
                    slow = true;
                    aim = player.starFlyLastDir;
                }
                Vector2 currentDir = player.Speed.SafeNormalize(Vector2.Zero);
                currentDir = (player.starFlyLastDir = ((!(currentDir == Vector2.Zero)) ? currentDir.RotateTowards(aim.Angle(), 5.5850534f * Engine.DeltaTime * controller.myTurnRate) : aim));
                float target;
                if (slow)
                {
                    player.starFlySpeedLerp = 0f;
                    target = controller.myLowSpeed;
                }
                else if (currentDir != Vector2.Zero && Vector2.Dot(currentDir, aim) >= 0.45f)
                {
                    player.starFlySpeedLerp = Calc.Approach(player.starFlySpeedLerp, 1f, Engine.DeltaTime / 1f);
                    target = MathHelper.Lerp(controller.myTurnSpeed, controller.myHighSpeed, player.starFlySpeedLerp);
                }
                else
                {
                    player.starFlySpeedLerp = 0f;
                    target = controller.myTurnSpeed;
                }
                player.starFlyLoopSfx.Param("feather_speed", (!slow) ? 1 : 0);
                float currentspeed = ((Vector2)(player.Speed)).Length();
                currentspeed = Calc.Approach(currentspeed, target, 1000f * Engine.DeltaTime);
                player.Speed = currentDir * currentspeed;
                if (player.level.OnInterval(0.02f))
                {
                    player.level.Particles.Emit(FlyFeather.P_Flying, 1, player.Center, Vector2.One * 2f, (-player.Speed).Angle());
                }
                if (Input.Jump.Pressed)
                {
                    if (player.OnGround(3))
                    {
                        if (controller.fyper && Vector2.Dot(aim, Vector2.UnitY) > 0.45 && Math.Abs(Vector2.Dot(currentDir, Vector2.UnitX)) > 0.45)
                        {
                            player.Ducking = true;
                            player.SuperJump();
                        }
                        else if (controller.fuper && Math.Abs(Vector2.Dot(currentDir, Vector2.UnitX)) > 0.45) //ducked feather state should still pass a hyper through this regardless of input
                        {
                            player.SuperJump();
                        }
                        else
                        {
                            player.Jump();
                        }
                        return Player.StNormal;
                    }
                    if (player.WallJumpCheck(-1))
                    {
                        if (controller.founce && Vector2.Dot(currentDir, Vector2.UnitY) < -0.45)
                        {
                            player.SuperWallJump(1);
                        }
                        else
                        {
                            player.WallJump(1);
                        }
                        return Player.StNormal;
                    }
                    if (player.WallJumpCheck(1))
                    {
                        if (controller.founce && Vector2.Dot(currentDir, Vector2.UnitY) < -0.45)
                        {
                            player.SuperWallJump(-1);
                        }
                        else
                        {
                            player.WallJump(-1);
                        }
                        return Player.StNormal;
                    }
                }
                if (Input.GrabCheck)
                {
                    bool flag2 = false;
                    int dir = 0;
                    if (Input.MoveX.Value != -1 && player.ClimbCheck(1))
                    {
                        player.Facing = Facings.Right;
                        dir = 1;
                        flag2 = true;
                    }
                    else if (Input.MoveX.Value != 1 && player.ClimbCheck(-1))
                    {
                        player.Facing = Facings.Left;
                        dir = -1;
                        flag2 = true;
                    }
                    if (flag2)
                    {
                        if (SaveData.Instance.Assists.NoGrabbing)
                        {
                            player.Speed = Vector2.Zero;
                            player.ClimbTrigger(dir);
                            return Player.StNormal;
                        }
                        return Player.StClimb;
                    }
                }
                if (player.CanDash)
                {
                    return player.StartDash();
                }
                player.starFlyTimer -= Engine.DeltaTime;
                if (player.starFlyTimer <= 0f)
                {
                    if (Input.MoveY.Value == -1)
                    {
                        player.Speed.Y = -100f;
                    }
                    if (Input.MoveY.Value < 1)
                    {
                        player.varJumpSpeed = player.Speed.Y;
                        player.AutoJump = true;
                        player.AutoJumpTimer = 0f;
                        player.varJumpTimer = 0.2f;
                    }
                    if (player.Speed.Y > 0f)
                    {
                        player.Speed.Y = 0f;
                    }
                    if (Math.Abs(player.Speed.X) > 140f)
                    {
                        player.Speed.X = 140f * (float)Math.Sign(player.Speed.X);
                    }
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                    return Player.StNormal;
                }
                if (player.starFlyTimer < 0.5f && player.Scene.OnInterval(0.05f))
                {
                    if (player.Sprite.Color == controller.trailColor)
                    {
                        player.Sprite.Color = Player.NormalHairColor;
                    }
                    else
                    {
                        player.Sprite.Color = controller.trailColor;
                    }
                }
            }
            return Player.StStarFly;
        }

        private static FeatherController GetController()
        {
            return Engine.Scene?.Tracker.GetEntity<FeatherController>();
        }

        private static bool injectNoStartSlowdown(bool value)
        {
            FeatherController controller = GetController();
            return (controller is null) ? value : value && !controller.noStartSlowdown;
        }

        private static Color injectTrailColor(Color value)
        {
            FeatherController controller = GetController();
            return (controller is null) ? value : controller.trailColor;
        }

        private static float injectFlyTime(float value)
        {
            FeatherController controller = GetController();
            return (controller is null) ? value : controller.myFlyTime;
        }

        private static Vector2 OctalSnapping(Vector2 input)
        {
            if (input == Vector2.Zero)
                return Vector2.Zero;

            FeatherController controller = GetController();
            if (controller is null || !controller.octalAnalog)
                return input;

            input.Normalize();
            Vector2 snapped = Vector2.Zero;
            if (Math.Abs(input.X) >= 0.38268f)
                snapped.X = Math.Sign(input.X);

            if (Math.Abs(input.Y) >= 0.38268f)
                snapped.Y = Math.Sign(input.Y);

            return snapped;
        }

        private static bool injectNoFeatherBoost()
        {
            FeatherController controller = GetController();
            return (controller is null || !controller.noFeatherBoost);
        }

        //Using IL hooks to modify the coroutine in-place
        private static void StarFlyCoroutineIL(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            //noStartSlowdown injection
            if (!cursor.TryGotoNext(MoveType.After,
            instr => instr.MatchCall(typeof(Vector2).GetMethod("op_Inequality",BindingFlags.Public | BindingFlags.Static))))
            {
                throw new InvalidOperationException("Couldn't find noStartSlowdown IL");
            }
            cursor.EmitDelegate(injectNoStartSlowdown);

            //trailColor injection
            if (!cursor.TryGotoNext(MoveType.After,
            instr => instr.MatchLdfld(typeof(Player).GetField("starFlyColor", BindingFlags.Instance | BindingFlags.NonPublic))))
            {
                throw new InvalidOperationException("Couldn't find color IL");
            }
            cursor.EmitDelegate(injectTrailColor);

            //flyTime injection
            if (!cursor.TryGotoNext(MoveType.Before,
            instr => instr.MatchStfld(typeof(Player).GetField("starFlyTimer", BindingFlags.Instance | BindingFlags.NonPublic))))
            {
                throw new InvalidOperationException("Couldn't find fly timer IL");
            }
            cursor.EmitDelegate(injectFlyTime);

            // feather input snapping
            if (!cursor.TryGotoNext(MoveType.After,
            instr => instr.MatchCallvirt(typeof(VirtualJoystick).GetProperty("Value").GetGetMethod())))
            {
                throw new InvalidOperationException("Couldn't find feather input IL");
            }
            cursor.EmitDelegate(OctalSnapping);

            //noFeatherBoost injection
            if (!cursor.TryGotoNext(MoveType.After,
            instr => instr.MatchStfld(typeof(Player).GetField("Speed", BindingFlags.Public | BindingFlags.Instance))))
            {
                throw new InvalidOperationException("Couldn't find post feather boost IL");
            }
            ILLabel skipBoost = cursor.MarkLabel();

            if (!cursor.TryGotoPrev(MoveType.Before,
            instr => instr.MatchLdloc(1)))
            {
                throw new InvalidOperationException("Couldn't find pre feather boost IL");
            }
            cursor.MoveAfterLabels();
            cursor.EmitDelegate(injectNoFeatherBoost);
            cursor.EmitBrfalse(skipBoost);
        }
    }
}