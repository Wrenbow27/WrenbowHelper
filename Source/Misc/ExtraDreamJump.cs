using System;
using System.Runtime.CompilerServices;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.WrenbowHelper {

    public static class ExtraDreamJump {

        private static bool hooked;
        private const string flag = "WrenbowHelper_HasDreamJump";

        public static void Load()
        {
            if (hooked)
            {
                return;
            }

            On.Celeste.Player.DreamDashEnd += Player_DreamDashEnd;
            On.Celeste.Player.Update += Player_Update;
            On.Celeste.Player.Jump += Player_Jump;
            On.Celeste.Player.SuperJump += Player_SuperJump;
            On.Celeste.Player.Added += Player_Added;

            hooked = true;
        }

        public static void Unload()
        {
            if (!hooked)
            {
                return;
            }

            On.Celeste.Player.DreamDashEnd -= Player_DreamDashEnd;
            On.Celeste.Player.Update -= Player_Update;
            On.Celeste.Player.Jump -= Player_Jump;
            On.Celeste.Player.SuperJump -= Player_SuperJump;
            On.Celeste.Player.Added -= Player_Added;

            hooked = false;
        }

        public static bool GiveDreamJump(
            Player player,
            int earlyLeniencyFrames,
            int lateLeniencyFrames,
            bool persistent,
            bool trailIndicator,
            Color trailColor)
        {
            if (!WrenbowHelperModule.Session.HasDreamJump) 
            {
                WrenbowHelperModule.Session.HasDreamJump = true;
                WrenbowHelperModule.Session.EarlyLeniencyFrames = earlyLeniencyFrames;
                WrenbowHelperModule.Session.LateLeniencyFrames = lateLeniencyFrames;
                WrenbowHelperModule.Session.Persistent = persistent;
                WrenbowHelperModule.Session.TrailIndicator = trailIndicator;
                WrenbowHelperModule.Session.TrailColor = trailColor;
                FlagUpdate(player);
                return true;
            }
            return false;
        }

        private static void Player_Update(On.Celeste.Player.orig_Update orig, Player player)
        {
            orig(player);
            
            if (WrenbowHelperModule.Session.DreamBlockGraceTime > 0f)
                {
                    WrenbowHelperModule.Session.DreamBlockGraceTime -= Engine.DeltaTime;
                }
                else if (WrenbowHelperModule.Session.CanDreamJump)
                {
                    WrenbowHelperModule.Session.CanDreamJump = false;
                    if (!WrenbowHelperModule.Session.Persistent)
                    {
                        WrenbowHelperModule.Session.HasDreamJump = false;
                        FlagUpdate(player);
                    }
                }

                if (player.onGround && WrenbowHelperModule.Session.CanDreamJump)
                {
                    WrenbowHelperModule.Session.CanDreamJump = false;
                    if (!WrenbowHelperModule.Session.Persistent)
                    {
                        WrenbowHelperModule.Session.HasDreamJump = false;
                        FlagUpdate(player);
                    }
                }

                if ((WrenbowHelperModule.Session.HasDreamJump || WrenbowHelperModule.Session.CanDreamJump) && player.Scene.OnInterval(0.1f))
                {
                    Vector2 scale = new(
                        Math.Abs(player.Sprite.Scale.X),
                        player.Sprite.Scale.Y
                    );
                    TrailManager.Add(player, scale, WrenbowHelperModule.Session.TrailColor);
                }
        }
        private static void Player_DreamDashEnd(On.Celeste.Player.orig_DreamDashEnd orig, Player player)
        {
            orig(player);
            if (WrenbowHelperModule.Session.HasDreamJump)
            {
                player.jumpGraceTimer += WrenbowHelperModule.Session.EarlyLeniencyFrames * Engine.DeltaTime;
                WrenbowHelperModule.Session.DreamBlockGraceTime = player.jumpGraceTimer;
                WrenbowHelperModule.Session.CanDreamJump = true;
                if (!WrenbowHelperModule.Session.Persistent)
                {
                    WrenbowHelperModule.Session.HasDreamJump = false;
                    FlagUpdate(player);
                }
            }
        }
        private static void Player_Jump(On.Celeste.Player.orig_Jump orig, Player player, bool particles, bool playSfx)
        {
            orig(player, particles, playSfx);
            if (player.dreamBlock == null)
            {
                RefundCoyote(player);
            }
        }
        private static void Player_SuperJump(On.Celeste.Player.orig_SuperJump orig, Player player)
        {
            orig(player);
            RefundCoyote(player);
        }

        private static void FlagUpdate(Player player)
        {
            Level level = player.SceneAs<Level>();
            level?.Session.SetFlag(flag, WrenbowHelperModule.Session.HasDreamJump || WrenbowHelperModule.Session.CanDreamJump);
        }
        private static void RefundCoyote(Player player)
        {
            if (WrenbowHelperModule.Session.CanDreamJump && WrenbowHelperModule.Session.DreamBlockGraceTime > 0f)
            {
                player.jumpGraceTimer = WrenbowHelperModule.Session.DreamBlockGraceTime + WrenbowHelperModule.Session.LateLeniencyFrames * Engine.DeltaTime;
                WrenbowHelperModule.Session.DreamBlockGraceTime = 0f;
                WrenbowHelperModule.Session.HasDreamJump = false;
                WrenbowHelperModule.Session.CanDreamJump = false;
                FlagUpdate(player);

                Audio.Play("event:/game/general/diamond_return", player.Position);
            }
        }
        private static void Player_Added(On.Celeste.Player.orig_Added orig, Player player, Scene scene)
        {
            orig(player, scene);
            Level level = scene as Level;
            if (level != null)
            {
                level?.Session.SetFlag(flag, false);
            }
            WrenbowHelperModule.Session.HasDreamJump = false;
            WrenbowHelperModule.Session.CanDreamJump = false;
        }
    }
}