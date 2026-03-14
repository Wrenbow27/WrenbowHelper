using System;
using Microsoft.Xna.Framework;
using System.Runtime.CompilerServices;

namespace Celeste.Mod.WrenbowHelper {

    public static class DreamBlockLiftSpeedFix {

        private static bool hooked;

        private const float AugmentedDuration = 0.26f; //LiftSpeedGraceTime (0.16) + dreamDashCanEndTimer (0.1)

        private class StoredLiftSpeed
        {
            public Vector2 Lift;
        }

        // Stores the dream block's liftspeed between frames so the player can grab the correct value when updating (before the block)
        private static readonly ConditionalWeakTable<DreamBlock, StoredLiftSpeed> liftSpeedTable = new();

        public static void Load()
        {
            if (hooked)
            {
                return;
            }
            
            On.Celeste.Player.DreamDashBegin += Player_DreamDashBegin;
            On.Celeste.Platform.Update += Platform_Update;
            On.Celeste.Solid.MoveHExact += Solid_MoveHExact;
            On.Celeste.Solid.MoveVExact += Solid_MoveVExact;

            hooked = true;
        }

        public static void Unload()
        {
            if (!hooked)
            {
                return;
            }

            On.Celeste.Player.DreamDashBegin -= Player_DreamDashBegin;
            On.Celeste.Platform.Update -= Platform_Update;
            On.Celeste.Solid.MoveHExact -= Solid_MoveHExact;
            On.Celeste.Solid.MoveVExact -= Solid_MoveVExact;
            

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
                    data.Lift = Vector2.Zero;
                }
            }
        }

        private static void Player_DreamDashBegin(On.Celeste.Player.orig_DreamDashBegin orig, Player player)
        {
            orig(player);

            if (!WrenbowHelperModule.DreamBlockLiftSpeedFixEnabled)
            {
                return;
            }

            DreamBlock block = DetectDreamBlock(player);
            if (block == null)
            {
                return;
            }

            if (liftSpeedTable.TryGetValue(block, out StoredLiftSpeed liftData))
            {
                Vector2 lift = liftData.Lift;

                if (lift != Vector2.Zero)
                {
                    player.LiftSpeed = lift;
                    player.liftSpeedTimer = AugmentedDuration;
                }
            }
        }
        
        private static DreamBlock DetectDreamBlock(Player player)
        {
            //Collision code taken from player.cs
            Vector2 dir = player.DashDir;
            Vector2 checkPos = player.Position + dir;

            DreamBlock dreamBlock = player.CollideFirst<DreamBlock>(checkPos);
            if (dreamBlock == null)
            {
                return null;
            }
            if (player.CollideCheck<Solid, DreamBlock>(checkPos))
            {
                Vector2 side = new Vector2(Math.Abs(dir.Y), Math.Abs(dir.X));

                bool checkNegative = (dir.X != 0) ? player.Speed.Y <= 0 : player.Speed.X <= 0;
                bool checkPositive = (dir.X != 0) ? player.Speed.Y >= 0 : player.Speed.X >= 0;

                int correction = Player.DashCornerCorrection;

                if (checkNegative) {
                    for (int i = -1; i >= -correction; i--)
                    {
                        Vector2 at = checkPos + side * i;
                        if (!player.CollideCheck<Solid, DreamBlock>(at))
                        {
                            return player.CollideFirst<DreamBlock>(at);
                        }
                    }
                }

                if (checkPositive) {
                    for (int i = 1; i <= correction; i++)
                    {
                        Vector2 at = checkPos + side * i;
                        if (!player.CollideCheck<Solid, DreamBlock>(at))
                        {
                            return player.CollideFirst<DreamBlock>(at);
                        }
                    }
                }

                return null;
            }

            return dreamBlock;
        }
    }
}