using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.WrenbowHelper {

    public static class DreamBlockLiftSpeedFix {

        private static bool hooked;

        private const float AugmentedDuration = 0.26f; //LiftSpeedGraceTime (0.16) + dreamDashCanEndTimer (0.1)

        private class StoredLiftSpeed
        {
            public Vector2 Lift;
            public int age;
        }

        // Stores the dream block's liftspeed between frames so the player can grab the correct value when updating (before the block)
        private static readonly ConditionalWeakTable<DreamBlock, StoredLiftSpeed> liftSpeedTable = new();

        public static void Load()
        {
            if (hooked)
            {
                return;
            }
            On.Celeste.Platform.Update += Platform_Update;
            On.Celeste.Solid.MoveHExact += Solid_MoveHExact;
            On.Celeste.Solid.MoveVExact += Solid_MoveVExact;
            On.Celeste.Player.DreamDashCheck += Player_DreamDashCheck;

            hooked = true;
        }

        public static void Unload()
        {
            if (!hooked)
            {
                return;
            }
            On.Celeste.Platform.Update -= Platform_Update;
            On.Celeste.Solid.MoveHExact -= Solid_MoveHExact;
            On.Celeste.Solid.MoveVExact -= Solid_MoveVExact;
            On.Celeste.Player.DreamDashCheck -= Player_DreamDashCheck;


            hooked = false;
        }
        private static void Solid_MoveHExact(On.Celeste.Solid.orig_MoveHExact orig, Solid solid, int move)
        {
            orig(solid, move);
            if (!WrenbowHelperModule.DreamBlockLiftSpeedFixEnabled)
            {
                return;
            }

            if (solid is DreamBlock block) {
                StoredLiftSpeed data = liftSpeedTable.GetOrCreateValue(block);
                data.Lift.X = block.LiftSpeed.X;
                data.age = 0;
            }
        }

        private static void Solid_MoveVExact(On.Celeste.Solid.orig_MoveVExact orig, Solid solid, int move)
        {
            orig(solid, move);
            if (!WrenbowHelperModule.DreamBlockLiftSpeedFixEnabled)
            {
                return;
            }

            if (solid is DreamBlock block)
            {
                StoredLiftSpeed data = liftSpeedTable.GetOrCreateValue(block);
                data.Lift.Y = block.LiftSpeed.Y;
                data.age = 0;
            }
     }

        private static void Platform_Update(On.Celeste.Platform.orig_Update orig, Platform platform)
        {
            orig(platform);

            if (!WrenbowHelperModule.DreamBlockLiftSpeedFixEnabled)
            {
                return;
            }

            if (platform is DreamBlock block)
            {
                if (liftSpeedTable.TryGetValue(block, out StoredLiftSpeed data))
                {
                    if (data.age <= 0) // delay the liftspeed decay by a frame in order to hopefully prevent skipping frames when the speed is < 60
                    {
                        data.age++;
                    }
                    else
                    {
                        data.Lift = Vector2.Zero;
                    }
                }
            }
        }

        private static bool Player_DreamDashCheck(On.Celeste.Player.orig_DreamDashCheck orig, Player player, Vector2 dir)
        {
            bool result = orig(player, dir);
            if (!WrenbowHelperModule.DreamBlockLiftSpeedFixEnabled || !result || player.dreamBlock == null)
            {
                return result;
            }
            if (liftSpeedTable.TryGetValue(player.dreamBlock, out StoredLiftSpeed liftData))
            {
                Vector2 lift = liftData.Lift;
                if (lift != Vector2.Zero)
                {
                    player.LiftSpeed = lift;
                    player.liftSpeedTimer = AugmentedDuration;
                }
            }
            return result;
        }
    }
}