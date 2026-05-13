using System;
using System.Runtime.CompilerServices;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.WrenbowHelper {

    public static class ExtraDreamJump {

        private static bool hooked;
        private const string flag = "WrenbowHelper_HasDreamJump";

        private class WrenbowStoredDreamJump {
            public bool HasDreamJump = false;
            public bool CanDreamJump = false;
            public float DreamBlockGraceTime = 0f;
            public int EarlyLeniencyFrames = 0;
            public int LateLeniencyFrames = 0;
            public bool Persistent = false;
            public bool TrailIndicator = false;
            public Color TrailColor = Calc.HexToColor("c81ec8");
        }

        private static readonly ConditionalWeakTable<Player, WrenbowStoredDreamJump> wrenbowStoredDreamJump = new ConditionalWeakTable<Player, WrenbowStoredDreamJump>();

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
            On.Celeste.Player.SuperWallJump += Player_SuperWallJump;
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
            On.Celeste.Player.SuperWallJump -= Player_SuperWallJump;
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
            WrenbowStoredDreamJump data = wrenbowStoredDreamJump.GetOrCreateValue(player);
            if (!data.HasDreamJump)
            {
                data.HasDreamJump = true;
                data.EarlyLeniencyFrames = earlyLeniencyFrames;
                data.LateLeniencyFrames = lateLeniencyFrames;
                data.Persistent = persistent;
                data.TrailIndicator = trailIndicator;
                data.TrailColor = trailColor;
                FlagUpdate(player, data);
                return true;
            }
            return false;
        }

        private static void Player_Update(On.Celeste.Player.orig_Update orig, Player player)
        {
            orig(player);
            
            if (wrenbowStoredDreamJump.TryGetValue(player, out var data))
            {
                if (data.DreamBlockGraceTime > 0f)
                {
                    data.DreamBlockGraceTime -= Engine.DeltaTime;
                }
                else if (data.CanDreamJump)
                {
                    data.CanDreamJump = false;
                    if (!data.Persistent)
                    {
                        data.HasDreamJump = false;
                        FlagUpdate(player, data);
                    }
                }

                if (player.onGround && data.CanDreamJump)
                {
                    data.CanDreamJump = false;
                    if (!data.Persistent)
                    {
                        data.HasDreamJump = false;
                        FlagUpdate(player, data);
                    }
                }

                if ((data.HasDreamJump || data.CanDreamJump) && player.Scene.OnInterval(0.1f))
                {
                    Vector2 scale = new(
                        Math.Abs(player.Sprite.Scale.X),
                        player.Sprite.Scale.Y
                    );
                    TrailManager.Add(player, scale, data.TrailColor);
                }

            }            
        }
        private static void Player_DreamDashEnd(On.Celeste.Player.orig_DreamDashEnd orig, Player player)
        {
            orig(player);
            if (!wrenbowStoredDreamJump.TryGetValue(player, out var data))
            {
                return;
            }
            if (data.HasDreamJump)
            {
                player.jumpGraceTimer += data.EarlyLeniencyFrames * Engine.DeltaTime;
                data.DreamBlockGraceTime = player.jumpGraceTimer;
                data.CanDreamJump = true;
                if (!data.Persistent)
                {
                    data.HasDreamJump = false;
                    FlagUpdate(player, data);
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
        private static void Player_SuperWallJump(On.Celeste.Player.orig_SuperWallJump orig, Player player, int dir)
        {
            orig(player, dir);
            RefundCoyote(player);
        }
        private static void FlagUpdate(Player player, WrenbowStoredDreamJump data)
        {
            Level level = player.SceneAs<Level>();
            level?.Session.SetFlag(flag, data.HasDreamJump || data.CanDreamJump);
        }
        private static void RefundCoyote(Player player)
        {
            if (!wrenbowStoredDreamJump.TryGetValue(player, out var data))
            {
                return;
            }
            if (data.CanDreamJump && data.DreamBlockGraceTime > 0f)
            {
                player.jumpGraceTimer = data.DreamBlockGraceTime + data.LateLeniencyFrames * Engine.DeltaTime;
                data.DreamBlockGraceTime = 0f;
                data.HasDreamJump = false;
                data.CanDreamJump = false;
                FlagUpdate(player, data);

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
        }
    }
}