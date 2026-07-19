using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.WrenbowHelper;

public class RubiksState
{
    public RubiksLogic.CubeColors[,,] Cube { get; }
    public int Size { get; }
    public bool IsSolved => CheckSolved();

    public RubiksState(int size)
    {
        Size = size;
        Cube = new RubiksLogic.CubeColors[6, size, size];
        InitializeCube();
    }

    public bool IsLocked { get; set; }

    private bool CheckSolved()
    {
        bool[] seenColors = [false, false, false, false, false, false];
        for (int face = 0; face < 6; face++)
        {
            RubiksLogic.CubeColors color = Cube[face, 0, 0];
            if (seenColors[(int)color])
            {
                return false;
            }
            seenColors[(int)color] = true;
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    if (Cube[face, i, j] != color)
                        return false;
                }
            }
        }
        return true;
    }
    private void InitializeCube()
    {
        for (int face = 0; face < 6; face++)
        {
            RubiksLogic.FloodFace(this, (RubiksLogic.AbsoluteFaces)face, (RubiksLogic.CubeColors)face); //relies on the order of the enums not changing
        }
    }
}

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

public static class RubiksLogic
{
    public readonly struct RubiksTurn
    {
        public AbsoluteFaces Face { get; }
        public TurnDir Direction { get; }
        public int Depth { get; }

        public RubiksTurn(AbsoluteFaces face, TurnDir direction, int depth)
        {
            Face = face;
            Direction = direction;
            Depth = depth;
        }
    }

    public enum CubeColors //do not change order
    {
        White,
        Yellow,
        Blue,
        Green,
        Red,
        Orange
    }

    public enum AbsoluteFaces //do not change order
    {
        Down,
        Up,
        Left,
        Right,
        Front,
        Back
    }

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

    public static void Scramble(RubiksState state, int scrambleLength, Random rng)
    {
        for (int i = 0; i < scrambleLength; i++)
        {
            RubiksTurn randomTurn = new(
                (AbsoluteFaces)rng.Next(6),
                (TurnDir)rng.Next(2),
                rng.Next((state.Size + 1) / 2));

            DoTurn(state, randomTurn);
        }
    }

    public static void DoTurn(RubiksState state, RubiksTurn turn)
    {
        if (turn.Depth > Math.Ceiling(state.Size / 2.0) - 1.0)
        {
            throw new ArgumentOutOfRangeException("WrenbowHelper_RubiksCube: Invalid turn depth for cube size");
        }
        if (turn.Depth == 0)
        {
            TurnFaceStickers(state, turn.Face, turn.Direction);
        }
        TurnRingStickers(state, turn.Face, turn.Direction, turn.Depth);
    }

    private static void TurnFaceStickers(RubiksState state, AbsoluteFaces face, TurnDir dir)
    {
        //transpose the face in-place
        for (int i = 0; i < state.Size; i++)
        {
            for (int j = i + 1; j < state.Size; j++)
            {
                (state.Cube[(int)face, i, j], state.Cube[(int)face, j, i]) = (state.Cube[(int)face, j, i], state.Cube[(int)face, i, j]);
            }
        }

        if (dir == TurnDir.Clockwise)
        {
            //Clockwise rotation (reverse each row)
            for (int i = 0; i < state.Size; i++)
            {
                WriteLine(state, face, LineType.Row, i, false, ReadLine(state, face, LineType.Row, i, true));
            }
        }
        else if (dir == TurnDir.Widdershins)
        {
            //Widdershins rotation (reverse each column)
            for (int i = 0; i < state.Size; i++)
            {
                WriteLine(state, face, LineType.Column, i, false, ReadLine(state, face, LineType.Column, i, true));
            }
        }
    }

    private static void TurnRingStickers(RubiksState state, AbsoluteFaces face, TurnDir dir, int depth)
    {
        (AbsoluteFaces face, LineType lineType, int index, bool reverse)[] ringAccessKeys = GetRingAccessKeys(face, depth, state.Size);
        CubeColors[][] lines = new CubeColors[6][];
        for (int i = 0; i < 6; i++)
        {
            lines[i] = new CubeColors[state.Size];
        }
        //Construct the desired ring from the access keys
        for (int i = 0; i < 4; i++)
        {
            (AbsoluteFaces face, LineType lineType, int index, bool reverse) key = ringAccessKeys[i];
            CubeColors[] line = ReadLine(state, key.face, key.lineType, key.index, key.reverse);
            lines[i] = line;
        }

        //Disperse the ring back into the faces according to the keys
        for (int i = 0; i < 4; i++)
        {
            (AbsoluteFaces face, LineType lineType, int index, bool reverse) key = ringAccessKeys[i];
            int i_offset = i + (dir == TurnDir.Clockwise ? -1 : 1);
            if (i_offset == -1)
            {
                i_offset = 3;
            }
            if (i_offset == 4)
            {
                i_offset = 0;
            }
            WriteLine(state, key.face, key.lineType, key.index, key.reverse, lines[i_offset]);
        }
    }

    private static (AbsoluteFaces face, LineType lineType, int index, bool reverse)[] GetRingAccessKeys(AbsoluteFaces face, int depth, int size)
    {
        switch (face)
        {
            case AbsoluteFaces.Down:
                return [(AbsoluteFaces.Front, LineType.Row, size-depth-1, false),
                        (AbsoluteFaces.Right, LineType.Row, size-depth-1, false),
                        (AbsoluteFaces.Back, LineType.Row, size-depth-1, false),
                        (AbsoluteFaces.Left, LineType.Row, size-depth-1, false)];
            case AbsoluteFaces.Up:
                return [(AbsoluteFaces.Back, LineType.Row, depth, true),
                        (AbsoluteFaces.Right, LineType.Row, depth, true),
                        (AbsoluteFaces.Front, LineType.Row, depth, true),
                        (AbsoluteFaces.Left, LineType.Row, depth, true)];
            case AbsoluteFaces.Left:
                return [(AbsoluteFaces.Up, LineType.Column, depth, false),
                        (AbsoluteFaces.Front, LineType.Column, depth, false),
                        (AbsoluteFaces.Down, LineType.Column, depth, false),
                        (AbsoluteFaces.Back, LineType.Column, size-depth-1, true)];
            case AbsoluteFaces.Right:
                return [(AbsoluteFaces.Up, LineType.Column, size-depth-1, true),
                        (AbsoluteFaces.Back, LineType.Column, depth, false),
                        (AbsoluteFaces.Down, LineType.Column, size-depth-1, true),
                        (AbsoluteFaces.Front, LineType.Column, size-depth-1, true)];
            case AbsoluteFaces.Front:
                return [(AbsoluteFaces.Up, LineType.Row, size-depth-1, false),
                        (AbsoluteFaces.Right, LineType.Column, depth, false),
                        (AbsoluteFaces.Down, LineType.Row, depth, true),
                        (AbsoluteFaces.Left, LineType.Column, size-depth-1, true)];
            case AbsoluteFaces.Back:
                return [(AbsoluteFaces.Up, LineType.Row, depth, true),
                        (AbsoluteFaces.Left, LineType.Column, depth, false),
                        (AbsoluteFaces.Down, LineType.Row, size-depth-1, false),
                        (AbsoluteFaces.Right, LineType.Column, size-depth-1, true)];
        }
        throw new InvalidOperationException("WrenbowHelper_RubiksCube: Invalid rotation face for ring access keys");
    }

    private static  CubeColors[] ReadLine(RubiksState state, AbsoluteFaces face, LineType lineType, int index, bool reverse)
    {
        CubeColors[] line = new CubeColors[state.Size];
        if (lineType == LineType.Row)
        {
            for (int i = 0; i < state.Size; i++)
            {
                line[!reverse ? i : state.Size-i-1] = state.Cube[(int)face,index,i];
            }
        }
        else
        {
            for (int i = 0; i < state.Size; i++)
            {
                line[!reverse ? i : state.Size-i-1] = state.Cube[(int)face,i,index];
            }
        }
        return line;
    }

    private static void WriteLine(RubiksState state, AbsoluteFaces face, LineType lineType, int index, bool reverse, CubeColors[] val)
    {
        if (lineType == LineType.Row)
        {
            for (int i = 0; i < state.Size; i++)
            {
                state.Cube[(int)face,index,!reverse ? i : state.Size-i-1] = val[i];
            }
        }
        else
        {
            for (int i = 0; i < state.Size; i++)
            {
                state.Cube[(int)face,!reverse ? i : state.Size-i-1,index] = val[i];
            }
        }
    }

    internal static void FloodFace(RubiksState state, AbsoluteFaces face, CubeColors stickerColor)
    {
        for (int i = 0; i < state.Size; i++)
        {
            for (int j = 0; j < state.Size; j++)
            {
                state.Cube[(int)face,i,j] = stickerColor;
            }
        }
    }
}