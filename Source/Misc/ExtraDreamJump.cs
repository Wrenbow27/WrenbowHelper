using System.Runtime.CompilerServices;
using Monocle;

namespace Celeste.Mod.WrenbowHelper {

    public static class ExtraDreamJump {

        private static bool hooked;
        private const string flag = "WrenbowHelper_HasDreamJump";

        private class WrenbowStoredDreamJump {
            public bool HasDreamJump = false;
            public bool CanDreamJump = false;
            public float DreamBlockGraceTime = 0f;
            public bool Persistent = false;
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

        public static bool GiveDreamJump(Player player, bool persistent)
        {
            WrenbowStoredDreamJump data = wrenbowStoredDreamJump.GetOrCreateValue(player);
            if (!data.HasDreamJump)
            {
                data.HasDreamJump = true;
                data.Persistent = persistent;
                Level level = player.SceneAs<Level>();
                level?.Session.SetFlag(flag, true);
                return true;
            }
            return false;
        }

        private static void Player_Update(On.Celeste.Player.orig_Update orig, Player player)
        {
            //TODO: Revoke dream jump as needed, decrement DreamBlockGraceTime, etc
            if (wrenbowStoredDreamJump.TryGetValue(player, out var data))
            {
                if (data.DreamBlockGraceTime > 0f)
                {
                    data.DreamBlockGraceTime -= Engine.DeltaTime;
                }
                else if (data.CanDreamJump && !data.Persistent)
                {
                    data.CanDreamJump = false;
                    data.HasDreamJump = false;
                    Level level = player.SceneAs<Level>();
                    level?.Session.SetFlag(flag, false);
                }

                if (player.onGround && !data.Persistent && data.CanDreamJump)
                {
                    data.HasDreamJump = false;
                    data.CanDreamJump = false;
                    Level level = player.SceneAs<Level>();
                    level?.Session.SetFlag(flag, false);
                }
            }

            orig(player);
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
                data.DreamBlockGraceTime = Player.JumpGraceTime;
                data.CanDreamJump = true;
                if (!data.Persistent)
                {
                    data.HasDreamJump = false;
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
        private static void RefundCoyote(Player player)
        {
            if (!wrenbowStoredDreamJump.TryGetValue(player, out var data))
            {
                return;
            }
            if (data.CanDreamJump && data.DreamBlockGraceTime > 0f)
            {
                player.jumpGraceTimer = data.DreamBlockGraceTime;
                data.DreamBlockGraceTime = 0f;
                data.HasDreamJump = false;
                data.CanDreamJump = false;

                Level level = player.SceneAs<Level>();
                level?.Session.SetFlag(flag, false);

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