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
    private readonly string cubeName;

    //private CubeColors[,,] cube; //[face, row, column] (matrix convention)
    private readonly RubiksState state;

    private MTexture sticker;

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
    private readonly Vector2[] facePositions;

    private bool queuedScramble = false;
    private int scrambleAnimationTimer; //Qty of frames to play scramble animations for
    private bool isInitialized = false;
    private static readonly Random rng = new();

    public RubiksCube(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        size = data.Int("size", 3);
        startScrambled = data.Bool("startScrambled", true);
        scrambleLength = data.Int("scrambleLength", 30);
        solvedFlag = data.String("solvedFlag", "");
        displayType = data.String("displayType", "NetL");
        cubeName = data.String("cubeName", "");
        persistent = data.Bool("persistent", true) && !string.IsNullOrEmpty(cubeName);
        lockOnSolve = data.Bool("lockOnSolve", true);

        if (persistent && WrenbowHelperModule.Session.RubiksCubes.TryGetValue(cubeName, out RubiksState savedState))
        {
            isInitialized = true;
            state = savedState;
        }
        else
        {
            state = new RubiksState(size);

            if (persistent)
            {
                WrenbowHelperModule.Session.RubiksCubes[cubeName] = state;
            }
        }

        faceSpacing = (stickerSpacing * size) + faceGap;
        facePositions = GetFacePositions();
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        if (startScrambled && !isInitialized)
        {
            queuedScramble = true;
        }
    }

    public override void Update()
    {
        base.Update();
        if (!isInitialized)
        {
            isInitialized = true;
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

    public void RubiksTurn(RubiksLogic.RubiksTurn turn)
    {
        if (state.IsLocked)
        {
            //fail turn
            return;
        }
        RubiksLogic.DoTurn(state, turn);
        RubiksSolved();
    }

    public void RubiksTurn()
    {
        if (state.IsLocked || !state.SelectedFace.HasValue || !state.SelectedLayer.HasValue || !state.SelectedDir.HasValue)
        {
            //fail turn
            return;
        }
        RubiksLogic.RubiksTurn turn = new WrenbowHelper.RubiksLogic.RubiksTurn(
                    state.SelectedFace ?? RubiksLogic.AbsoluteFaces.Down,
                    state.SelectedDir ?? RubiksLogic.TurnDir.Clockwise,
                    state.SelectedLayer ?? 0);
        RubiksLogic.DoTurn(state, turn);
        RubiksSolved();
    }

    public void RubiksSelect(RubiksLogic.AbsoluteFaces face)
    {
        state.SelectedFace = face;
    }

    public void RubiksSelect(int layer)
    {
        state.SelectedLayer = layer;
    }

       public void RubiksSelect(RubiksLogic.TurnDir dir)
    {
        state.SelectedDir = dir;
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

    public static RubiksCube Find(Scene scene, string cubeName)
    {
        if (string.IsNullOrEmpty(cubeName))
        {
            return scene.Tracker.GetEntity<RubiksCube>();

        }

        foreach (RubiksCube cube in scene.Tracker.GetEntities<RubiksCube>())
        {
            if (cube.cubeName == cubeName)
                return cube;
        }
        return null;
    }

    public override void Render()
    {
        base.Render();

        DrawFace(RubiksLogic.AbsoluteFaces.Down, Position + facePositions[0]);
        DrawFace(RubiksLogic.AbsoluteFaces.Up, Position + facePositions[1]);
        DrawFace(RubiksLogic.AbsoluteFaces.Left, Position + facePositions[2]);
        DrawFace(RubiksLogic.AbsoluteFaces.Right, Position + facePositions[3]);
        DrawFace(RubiksLogic.AbsoluteFaces.Front, Position + facePositions[4]);
        DrawFace(RubiksLogic.AbsoluteFaces.Back, Position + facePositions[5]);
    }

    private void DrawFace(RubiksLogic.AbsoluteFaces face, Vector2 origin)
    {
        for (int i = 0; i < state.Size; i++)
        {
            for (int j = 0; j < state.Size; j++)
            {
                RubiksLogic.CubeColors color = state.Cube[(int)face, i, j];
                sticker = GFX.Game["objects/WrenbowHelper/RubiksCube/RubiksSticker"+(WrenbowHelperSettings.RubiksColorBlindMode ? "CB"+((int)color).ToString() : "")];
                sticker.Draw(origin + (new Vector2(j, i) * stickerSpacing), Vector2.Zero, GetStickerColor(color));
            }
        }
    }

    private Color GetStickerColor(RubiksLogic.CubeColors color)
    {
        if (scrambleAnimationTimer > 0 && isInitialized)
        {
            return StickerColors[rng.Next(0,6)];
        }
        else
        {
            return StickerColors[(int)color];
        }

    }

    private Vector2[] GetFacePositions()
    {
        switch (displayType)
        {
            case "NetL":
                return [new Vector2(2 * faceSpacing, 2 * faceSpacing),
                        new Vector2(2 * faceSpacing, 0),
                        new Vector2(faceSpacing, faceSpacing),
                        new Vector2(3 * faceSpacing, faceSpacing),
                        new Vector2(2 * faceSpacing, faceSpacing),
                        new Vector2(0, 1 * faceSpacing),];

            case "NetR":
                return [new Vector2(faceSpacing, 2 * faceSpacing),
                        new Vector2(faceSpacing, 0),
                        new Vector2(0, faceSpacing),
                        new Vector2(2 * faceSpacing, faceSpacing),
                        new Vector2(faceSpacing, faceSpacing),
                        new Vector2(3 * faceSpacing, 1 * faceSpacing),];
        }
        throw new InvalidOperationException("WrenbowHelper_RubiksCube: Invalid display type");
    }
}