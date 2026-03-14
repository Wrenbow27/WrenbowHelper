using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.WrenbowHelper {

    public static class SetLiftSpeedOverride {

        private static bool hooked;

        private class WrenbowLiftSpeedData
        {
            public Vector2? PendingLiftSpeed;
            public float overrideTimer;
        }

        private static readonly ConditionalWeakTable<Player, WrenbowLiftSpeedData> wrenbowLiftSpeed = new ConditionalWeakTable<Player, WrenbowLiftSpeedData>();

        public static void Load()
        {
            if (hooked)
            {
                return;
            }

            On.Celeste.Player.Update += Player_Update;

            hooked = true;
        }

        public static void Unload()
        {
            if (!hooked)
            {
                return;
            }

            On.Celeste.Player.Update -= Player_Update;

            hooked = false;
        }

        internal static void SetLift(Player player, Vector2 lift, bool sustainedOverride = false)
        {
            WrenbowLiftSpeedData data = wrenbowLiftSpeed.GetOrCreateValue(player);
            data.PendingLiftSpeed = lift;
            data.overrideTimer = sustainedOverride ? player.LiftSpeedGraceTime : 0f;
        }

        private static void Player_Update(On.Celeste.Player.orig_Update orig, Player player)
        {
            if (wrenbowLiftSpeed.TryGetValue(player, out WrenbowLiftSpeedData data) && data.PendingLiftSpeed.HasValue)
            {
                player.LiftSpeed = data.PendingLiftSpeed.Value;
                player.liftSpeedTimer = player.LiftSpeedGraceTime; //often redundant, except in case where forcing 0 liftspeed

                if (data.overrideTimer > 0f)
                {
                    data.overrideTimer -= Engine.DeltaTime; //TODO test off-by-one ordering here
                    if (data.overrideTimer <= 0f)
                    {
                        data.PendingLiftSpeed = null;
                    }
                }
                else
                {
                    data.PendingLiftSpeed = null;
                }
            }

            orig(player);
        }
    }
}