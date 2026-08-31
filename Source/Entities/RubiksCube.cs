using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.WrenbowHelper.Entities;

[CustomEntity("WrenbowHelper/RubiksCube")]

public class RubiksCube : Entity {
    //lonn declared
    private readonly int size;
    private readonly bool startScrambled;
    private readonly int scrambleLength;
    private readonly string solvedFlag;
    private readonly string cubeLayout;
    private readonly bool lockOnSolve;
    private readonly bool persistent;
    private readonly string cubeName;
    private readonly Color[] colorPalette;
    private readonly Color[] backgroundPalette;
    internal readonly RubiksState state;

    private MTexture sticker;

    private readonly Color[] StickerColors =
    {
        Color.White,
        Color.Yellow,
        Color.Blue,
        Color.Green,
        Color.Red,
        Color.Orange
    };

    private readonly Color[] BackgroundColors =
    {
        Color.Black,
        Color.White,
        Color.Gray
    };

    private readonly int stickerSpacing = 9;
    private readonly int faceGap = 1;
    private readonly int faceSpacing;
    private readonly Vector2[] facePositions;

    private bool queuedScramble = false;
    private bool isInitialized = false;
    private static readonly Random rng = new();
    private EventInstance scrambleSFX;

    public RubiksCube(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        size = data.Int("size", 3);
        startScrambled = data.Bool("startScrambled", true);
        scrambleLength = data.Int("scrambleLength", 30);
        solvedFlag = data.String("solvedFlag", "");
        cubeLayout = data.String("cubeLayout", "NetL");
        cubeName = data.String("cubeName", "cube0");
        persistent = data.Bool("persistent", true) && !string.IsNullOrEmpty(cubeName);
        lockOnSolve = data.Bool("lockOnSolve", true);


        base.Depth = 1500;

        if (WrenbowHelperModule.Session.RubiksCubes.TryGetValue(cubeName, out RubiksState savedState))
        {
            isInitialized = true;
            state = savedState;
            persistent = state.Persistent;
            colorPalette = state.ColorPalette;
            backgroundPalette = state.BackgroundPalette;
        }
        else
        {
            state = new RubiksState(size);
            state.Persistent = persistent;
            state.SolvedFlag = solvedFlag;
            state.LockOnSolve = lockOnSolve;

            string[] colorsAsStrings = data.Attr("colorPalette", "FFFFFF,FFFF00,0000FF,008000,FF0000,FFA500").Split(',');
            colorPalette = new Color[6];
            for (int i = 0; i < 6; i++)
            {
                colorPalette[i] = Calc.HexToColor(colorsAsStrings[i]);
            }
            state.ColorPalette = colorPalette;

            colorsAsStrings = data.Attr("backgroundPalette", "000000,FFFFFF,808080,").Split(',');
            backgroundPalette = new Color[3];
            for (int i = 0; i < 3; i++)
            {
                backgroundPalette[i] = Calc.HexToColor(colorsAsStrings[i]);
            }
            state.BackgroundPalette = backgroundPalette;

            if (!string.IsNullOrEmpty(cubeName))
            {
                WrenbowHelperModule.Session.RubiksCubes[cubeName] = state;
            }
        }

        StickerColors = state.ColorPalette;
        BackgroundColors = state.BackgroundPalette;

        faceSpacing = (stickerSpacing * state.Size) + faceGap;
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

        //Perform initial scramble
        if (!isInitialized)
        {
            isInitialized = true;
        }
        if (queuedScramble)
        {
            queuedScramble = false;
            state.RubiksScramble(scrambleLength);
        }

        //Play scrambling audio
        if (Engine.FrameCounter < state.endScrambleAnimFrame &&
           (state.endScrambleAnimFrame - Engine.FrameCounter) % 3 == 0)
        {
            if (scrambleSFX?.isValid() ?? false)
            {
                scrambleSFX.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                scrambleSFX.release();
                scrambleSFX = null;
            }

            scrambleSFX = Audio.Play("event:/WrenbowHelper/RubiksSFX", Position);
        }

        //Update flag from state
        if (state.FlagUpdatePending is not null)
        {
            SceneAs<Level>().Session.SetFlag(state.SolvedFlag, state.FlagUpdatePending.Value);
            state.FlagUpdatePending = null;
        }
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

    public override void Removed(Scene scene)
    {
        base.Removed(scene);
        if (!state.Persistent)
        {
            WrenbowHelperModule.Session.RubiksCubes.Remove(cubeName);
        }
    }

    private void DrawFace(RubiksLogic.AbsoluteFaces face, Vector2 origin)
    {
        Draw.Rect(origin.X, origin.Y, state.Size * stickerSpacing + 1, state.Size * stickerSpacing + 1, BackgroundColors[0]);
        for (int i = 0; i < state.Size; i++)
        {
            for (int j = 0; j < state.Size; j++)
            {
                if (isInitialized)
                {
                    RubiksLogic.HighlightLevel highlight = state.Highlights[(int)face, i, j];
                    if (highlight != RubiksLogic.HighlightLevel.None)
                    {
                        Draw.Rect(origin + (new Vector2(j, i) * stickerSpacing), stickerSpacing+1, stickerSpacing+1, (highlight == RubiksLogic.HighlightLevel.Primary ? BackgroundColors[1] : BackgroundColors[2]));
                    }
                }
                RubiksLogic.CubeColors color = state.Cube[(int)face, i, j];
                sticker = GFX.Game["objects/WrenbowHelper/RubiksCube/RubiksSticker"+(WrenbowHelperSettings.RubiksColorBlindMode ? "CB"+((int)color).ToString() : "")];
                sticker.Draw(origin + (new Vector2(j, i) * stickerSpacing) + Vector2.One, Vector2.Zero, GetStickerColor(color));
            }
        }
    }

    private Color GetStickerColor(RubiksLogic.CubeColors color)
    {
        if (Engine.FrameCounter < state.endScrambleAnimFrame && isInitialized)
        {
            return StickerColors[rng.Next(0, 6)];
        }
        else
        {
            return StickerColors[(int)color];
        }

    }

    private Vector2[] GetFacePositions()
    {
        switch (cubeLayout)
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
        throw new InvalidOperationException("WrenbowHelper_RubiksCube: Invalid cube Layout");
    }
}