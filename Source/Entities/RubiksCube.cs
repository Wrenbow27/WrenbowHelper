using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
    private readonly int scrambleDepth;
    private readonly string solvedFlag;
    private readonly string displayType;

    public enum CubeColors //do not change order
    {
        White,
        Yellow,
        Blue,
        Green,
        Red,
        Orange
    }
    public enum CanonicalFaces //do not change order
    {
        Down,
        Up,
        Left,
        Right,
        Front,
        Back
    }
    private CubeColors[,,] cube; //[face, row, column] (matrix convention)
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

    private int scrambling;
    private bool isLocked;

    public enum TurnDir
    {
        Clockwise,
        Widdershins //Widdershins in the big 26 raah
    }

    private enum LineType
    {
        Row,
        Column
    }

    public readonly struct RubiksTurn
    {
        public CanonicalFaces Face { get; }
        public TurnDir Direction { get; }
        public int Depth { get; }

        public RubiksTurn(CanonicalFaces face, TurnDir direction, int depth)
        {
            Face = face;
            Direction = direction;
            Depth = depth;
        }
    }


    public RubiksCube(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        size = data.Int("size", 3);
        startScrambled = data.Bool("startScrambled", true);
        scrambleDepth = data.Int("scrambleDepth", 30);
        solvedFlag = data.String("solvedFlag", "");
        displayType = data.String("displayType", "NetL");

        faceSpacing = stickerSpacing*size + faceGap;

        cube = new CubeColors[6,size,size];
        InitializeCube();
        sticker = GFX.Game["objects/WrenbowHelper/RubiksCube/RubiksSticker"];
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        // scramble here once I have turning
    }

    public override void Render()
    {
        base.Render();

        DrawFace(CanonicalFaces.Down, Position + new Vector2(2 * faceSpacing, 2 * faceSpacing));
        DrawFace(CanonicalFaces.Up, Position + new Vector2(2 * faceSpacing, 0));
        DrawFace(CanonicalFaces.Left, Position + new Vector2(1 * faceSpacing, 1 * faceSpacing));
        DrawFace(CanonicalFaces.Right, Position + new Vector2(3 * faceSpacing, 1 * faceSpacing));
        DrawFace(CanonicalFaces.Front, Position + new Vector2(2 * faceSpacing, 1 * faceSpacing));
        DrawFace(CanonicalFaces.Back, Position + new Vector2(0, 1*faceSpacing));
    }

    private void DrawFace(CanonicalFaces face, Vector2 origin)
    {
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                CubeColors color = cube[(int)face, i, j];

                sticker.Draw(origin + new Vector2(j, i) * stickerSpacing, Vector2.Zero, GetStickerColor(color));
            }
        }
    }

    public bool IsSolved()
    {
        HashSet<CubeColors> seenColors = new();
        for (int face = 0; face < 6; face++)
        {
            CubeColors color = cube[face, 0, 0];
            if (seenColors.Contains(color))
            {
                return false;
            }

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (cube[face, i, j] != color)
                        return false;
                }
            }
            if (!seenColors.Add(color))
            {
                return false;
            }
        }

        return true;
    }

    private Color GetStickerColor(CubeColors color)
    {
        return StickerColors[(int)color];
    }

    private void InitializeCube()
    {
        for (int face = 0; face < 6; face++)
        {
            FloodFace((CanonicalFaces)face, (CubeColors)face); //relies on the order of the enums not changing
        }
    }

    private CubeColors[,] ReadFace(CanonicalFaces face)
    {
        CubeColors[,] faceStickers = new CubeColors[size,size];
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                faceStickers[i,j] = cube[(int)face,i,j];
            }
        }
        return faceStickers;
    }

    private CubeColors[] ReadLine(CanonicalFaces face, LineType lineType, int index, bool reverse)
    {
        CubeColors[] line = new CubeColors[size];
        if (lineType == LineType.Row)
        {
            for (int i = 0; i < size; i++)
            {
                line[!reverse ? i : size-i-1] = cube[(int)face,index,i];
            }
        }
        else
        {
            for (int i = 0; i < size; i++)
            {
                line[!reverse ? i : size-i-1] = cube[(int)face,i,index];
            }
        }
        return line;
    }

    private void WriteLine(CanonicalFaces face, LineType lineType, int index, bool reverse, CubeColors[] val)
    {
        if (lineType == LineType.Row)
        {
            for (int i = 0; i < size; i++)
            {
                cube[(int)face,index,!reverse ? i : size-i-1] = val[i];
            }
        }
        else
        {
            for (int i = 0; i < size; i++)
            {
                cube[(int)face,!reverse ? i : size-i-1,index] = val[i];
            }
        }
    }
    
    private void FloodFace(CanonicalFaces face, CubeColors stickerColor)
    {
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                cube[(int)face,i,j] = stickerColor;
            }
        }
    }

    public void DoTurn(RubiksTurn turn)
    {
        if (turn.Depth > Math.Ceiling(size / 2.0) - 1.0)
        {
            throw new ArgumentOutOfRangeException("WrenbowHelper_RubiksCube: Invalid turn depth for cube size");
        }
        if (turn.Depth == 0)
        {
            TurnFaceStickers(turn.Face, turn.Direction);
        }
        TurnRingStickers(turn.Face, turn.Direction, turn.Depth);

        if (!string.IsNullOrEmpty(solvedFlag))
        {
            Level level = SceneAs<Level>();
            level.Session.SetFlag(solvedFlag, IsSolved());
        }
    }

    private void TurnFaceStickers(CanonicalFaces face, TurnDir dir)
    {
        //transpose the face in-place
        for (int i = 0; i < size; i++)
        {
            for (int j = i + 1; j < size; j++)
            {
                (cube[(int)face, i, j], cube[(int)face, j, i]) = (cube[(int)face, j, i], cube[(int)face, i, j]);
            }
        }

        if (dir == TurnDir.Clockwise)
        {
            //Clockwise rotation (reverse each row)
            for (int i = 0; i < size; i++)
            {
                WriteLine(face, LineType.Row, i, false, ReadLine(face, LineType.Row, i, true));
            }
        }
        else if (dir == TurnDir.Widdershins)
        {
            //Widdershins rotation (reverse each column)
            for (int i = 0; i < size; i++)
            {
                WriteLine(face, LineType.Column, i, false, ReadLine(face, LineType.Column, i, true));
            }
        }
    }

    private void TurnRingStickers(CanonicalFaces face, TurnDir dir, int depth)
    {
        (CanonicalFaces face, LineType lineType, int index, bool reverse)[] ringAccessKeys = GetRingAccessKeys(face, depth);
        CubeColors[][] lines = new CubeColors[6][];
        for (int i = 0; i < 6; i++)
        {
            lines[i] = new CubeColors[size];
        }
        //Construct the desired ring from the access keys
        for (int i = 0; i < 4; i++)
        {
            (CanonicalFaces face, LineType lineType, int index, bool reverse) key = ringAccessKeys[i];
            CubeColors[] line = ReadLine(key.face, key.lineType, key.index, key.reverse);
            lines[i] = line;
        }

        //Disperse the ring back into the faces according to the keys
        for (int i = 0; i < 4; i++)
        {
            (CanonicalFaces face, LineType lineType, int index, bool reverse) key = ringAccessKeys[i];
            int i_offset = i + (dir == TurnDir.Clockwise ? -1 : 1);
            if (i_offset == -1)
            {
                i_offset = 3;
            }
            if (i_offset == 4)
            {
                i_offset = 0;
            }
            WriteLine(key.face, key.lineType, key.index, key.reverse, lines[i_offset]);
        }
    }

    private (CanonicalFaces face, LineType lineType, int index, bool reverse)[] GetRingAccessKeys(CanonicalFaces face, int depth)
    {
        switch (face)
        {
            case CanonicalFaces.Down:
                return [(CanonicalFaces.Front, LineType.Row, size-depth-1, false),
                        (CanonicalFaces.Right, LineType.Row, size-depth-1, false),
                        (CanonicalFaces.Back, LineType.Row, size-depth-1, false),
                        (CanonicalFaces.Left, LineType.Row, size-depth-1, false)];;
            case CanonicalFaces.Up:
                return [(CanonicalFaces.Back, LineType.Row, depth, true),
                        (CanonicalFaces.Right, LineType.Row, depth, true),
                        (CanonicalFaces.Front, LineType.Row, depth, true),
                        (CanonicalFaces.Left, LineType.Row, depth, true)];;
            case CanonicalFaces.Left:
                return [(CanonicalFaces.Up, LineType.Column, depth, false),
                        (CanonicalFaces.Front, LineType.Column, depth, false),
                        (CanonicalFaces.Down, LineType.Column, depth, false),
                        (CanonicalFaces.Back, LineType.Column, size-depth-1, true)];;
            case CanonicalFaces.Right:
                return [(CanonicalFaces.Up, LineType.Column, size-depth-1, true),
                        (CanonicalFaces.Back, LineType.Column, depth, false),
                        (CanonicalFaces.Down, LineType.Column, size-depth-1, true),
                        (CanonicalFaces.Front, LineType.Column, size-depth-1, true)];;
            case CanonicalFaces.Front:
                return [(CanonicalFaces.Up, LineType.Row, size-depth-1, false),
                        (CanonicalFaces.Right, LineType.Column, depth, false),
                        (CanonicalFaces.Down, LineType.Row, depth, true),
                        (CanonicalFaces.Left, LineType.Column, size-depth-1, true)];;
            case CanonicalFaces.Back:
                return [(CanonicalFaces.Up, LineType.Row, depth, true),
                        (CanonicalFaces.Left, LineType.Column, depth, false),
                        (CanonicalFaces.Down, LineType.Row, size-depth-1, false),
                        (CanonicalFaces.Right, LineType.Column, size-depth-1, true)];;
        }
        throw new InvalidOperationException("WrenbowHelper_RubiksCube: Invalid rotation face for ring access keys");
    }
}