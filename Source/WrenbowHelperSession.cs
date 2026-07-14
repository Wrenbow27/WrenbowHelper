using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.WrenbowHelper
{

    public class WrenbowHelperSession : EverestModuleSession {

        internal bool DreamBlockLiftSpeedFixOverride = false;
        public bool HasDreamJump = false;
        public bool CanDreamJump = false;
        public float DreamBlockGraceTime = 0f;
        public int EarlyLeniencyFrames = 0;
        public int LateLeniencyFrames = 0;
        public bool Persistent = false;
        public bool TrailIndicator = false;
        public Color TrailColor = Calc.HexToColor("c81ec8");
        public Dictionary<string, RubiksState> RubiksCubes = new();

    }
}