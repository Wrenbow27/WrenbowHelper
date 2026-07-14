using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.WrenbowHelper.Entities;

[CustomEntity("WrenbowHelper/RubiksCube")]
[Tracked]

public class RubiksCube : Entity {
    //lonn declared
    private readonly int size;
    private readonly bool startScrambled;
    private readonly int scrambleLength;
    private readonly string solvedFlag;
    private readonly string displayType;
    private readonly bool lockOnSolve;
    private readonly bool persistent;
    private readonly string cubeID;

    //private CubeColors[,,] cube; //[face, row, column] (matrix convention)
    private readonly RubiksState state;

    private readonly MTexture sticker;

    private static readonly Color[] StickerColors =
    {
        Color.White,
        Color.Yellow,
        Color.Blue,
        Color.Green,
        Color.Red,
        Color.Orange
    };

    private readonly int stickerSpacing = 9;
    private readonly int faceGap = 1;
    private readonly int faceSpacing;

    private bool queuedScramble = false;
    private int scrambleAnimationTimer; //Qty of frames to play scramble animations for
    private bool wasLoaded = false;
    private static readonly Random rng = new();

    public RubiksCube(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        size = data.Int("size", 3);
        startScrambled = data.Bool("startScrambled", true);
        scrambleLength = data.Int("scrambleLength", 30);
        solvedFlag = data.String("solvedFlag", "");
        displayType = data.String("displayType", "NetL");
        cubeID = data.String("cubeID", "Cube0");
        persistent = data.Bool("persistent", true) && !string.IsNullOrEmpty(cubeID);
        //persistent = true;
        lockOnSolve = data.Bool("lockOnSolve", true);

        if (persistent && WrenbowHelperModule.Session.RubiksCubes.TryGetValue(cubeID, out RubiksState savedState))
        {
            wasLoaded = true;
            state = savedState;
        }
        else
        {
            state = new RubiksState(size);

            if (persistent)
            {
                WrenbowHelperModule.Session.RubiksCubes[cubeID] = state;
            }
        }

        faceSpacing = (stickerSpacing*size) + faceGap;

        sticker = GFX.Game["objects/WrenbowHelper/RubiksCube/RubiksSticker"];
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        if (startScrambled && !wasLoaded)
        {
            queuedScramble = true;
        }
    }

    public override void Update()
    {
        base.Update();
        if (!wasLoaded)
        {
            wasLoaded = true;
        }
        if (queuedScramble)
        {
            queuedScramble = false;
            InitScramble(scrambleLength);
            RubiksSolved();
        }

        if (scrambleAnimationTimer > 0)
        {
        scrambleAnimationTimer--;
        if (scrambleAnimationTimer == 0)
            {
                state.IsLocked = false;
            }
        }
    }

    public void RubiksTurn(RubiksLogic.RubiksTurn turn, bool overrideLock)
    {
        if (state.IsLocked && !overrideLock)
        {
            //fail turn
            return;
        }
        RubiksLogic.DoTurn(state, turn);
        RubiksSolved();
    }

    public void InitScramble(int scrambleLength)
    {
        state.IsLocked = true;
        scrambleAnimationTimer = 30;
        RubiksLogic.Scramble(state, scrambleLength, rng);
    }

    public bool RubiksSolved()
    {
        bool isSolved = state.IsSolved;

        if (!string.IsNullOrEmpty(solvedFlag))
        {
            SceneAs<Level>().Session.SetFlag(solvedFlag, isSolved);
        }
        if (lockOnSolve && isSolved)
        {
            state.IsLocked = true;
        }
        return isSolved;
    }

    public static RubiksCube Find(Scene scene, string cubeID)
    {
        if (string.IsNullOrEmpty(cubeID))
        {
            return scene.Tracker.GetEntity<RubiksCube>();

        }
        
        foreach (RubiksCube cube in scene.Tracker.GetEntities<RubiksCube>())
        {
            if (cube.cubeID == cubeID)
                return cube;
        }

        return null;
    }

    public override void Render()
    {
        base.Render();

        DrawFace(RubiksLogic.AbsoluteFaces.Down, Position + new Vector2(2 * faceSpacing, 2 * faceSpacing));
        DrawFace(RubiksLogic.AbsoluteFaces.Up, Position + new Vector2(2 * faceSpacing, 0));
        DrawFace(RubiksLogic.AbsoluteFaces.Left, Position + new Vector2(1 * faceSpacing, 1 * faceSpacing));
        DrawFace(RubiksLogic.AbsoluteFaces.Right, Position + new Vector2(3 * faceSpacing, 1 * faceSpacing));
        DrawFace(RubiksLogic.AbsoluteFaces.Front, Position + new Vector2(2 * faceSpacing, 1 * faceSpacing));
        DrawFace(RubiksLogic.AbsoluteFaces.Back, Position + new Vector2(0, 1*faceSpacing));
    }

    private void DrawFace(RubiksLogic.AbsoluteFaces face, Vector2 origin)
    {
        for (int i = 0; i < state.Size; i++)
        {
            for (int j = 0; j < state.Size; j++)
            {
                RubiksLogic.CubeColors color = state.Cube[(int)face, i, j];

                sticker.Draw(origin + (new Vector2(j, i) * stickerSpacing), Vector2.Zero, GetStickerColor(color));
            }
        }
    }

    private Color GetStickerColor(RubiksLogic.CubeColors color)
    {
        if (scrambleAnimationTimer > 0 && wasLoaded)
        {
            return StickerColors[rng.Next(0,6)];
        }
        else
        {
            return StickerColors[(int)color];
        }

    }
}