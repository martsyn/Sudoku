using System;
using System.Collections.Generic;
using System.Drawing;
using Console = Colorful.Console;

namespace Sudoku2
{
    class Sudoku
    {
        public static readonly byte[,] Hardest = {
            { 8, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 3, 6, 0, 0, 0, 0, 0 },
            { 0, 7, 0, 0, 9, 0, 2, 0, 0 },
            { 0, 5, 0, 0, 0, 7, 0, 0, 0 },
            { 0, 0, 0, 0, 4, 5, 7, 0, 0 },
            { 0, 0, 0, 1, 0, 0, 0, 3, 0 },
            { 0, 0, 1, 0, 0, 0, 0, 6, 8 },
            { 0, 0, 8, 5, 0, 0, 0, 1, 0 },
            { 0, 9, 0, 0, 0, 0, 4, 0, 0 },
        };

        public static readonly byte[,] Easy = {
            { 0, 9, 0,  0, 0, 0,  1, 0, 4, },
            { 6, 0, 0,  0, 9, 1,  0, 5, 7, },
            { 0, 8, 0,  0, 5, 0,  6, 0, 0, },

            { 0, 0, 0,  3, 1, 6,  7, 2, 8, },
            { 1, 0, 7,  0, 0, 0,  4, 0, 9, },
            { 2, 3, 8,  7, 4, 9,  0, 0, 0, },

            { 0, 0, 6,  0, 2, 0,  0, 4, 0, },
            { 9, 2, 0,  1, 3, 0,  0, 0, 6, },
            { 3, 0, 5,  0, 0, 0,  0, 1, 0, },
        };

        public static readonly byte[,] Medium = {
            {9,1,0, 0,0,5, 0,0,8,},
            {5,0,0, 8,0,1, 0,0,0,},
            {4,0,0, 0,0,3, 6,1,0,},

            {2,9,7, 0,0,0, 0,0,0,},
            {0,0,4, 0,0,0, 2,0,0,},
            {0,0,0, 0,0,0, 7,4,6,},

            {0,6,5, 1,0,0, 0,0,9,},
            {0,0,0, 5,0,8, 0,0,4,},
            {3,0,0, 2,0,0, 0,8,7,},
        };

        private readonly byte[,] _grid;

        private readonly bool[,,] _possible = new bool [9, 9, 10];

        private readonly (byte, byte)[][] _rows = new (byte, byte)[9][];
        private readonly (byte, byte)[][] _columns = new (byte, byte)[9][];
        private readonly (byte, byte)[][] _squares = new (byte, byte)[9][];

        public Sudoku(byte[,] board)
        {
            _grid = board;

            for (byte i = 0; i < 9; ++i)
            {
                _rows[i] = new (byte, byte)[9];
                for (byte j = 0; j < 9; ++j)
                    _rows[i][j] = (i, j);
            }

            for (byte j = 0; j < 9; ++j)
            {
                _columns[j] = new (byte, byte)[9];
                for (byte i = 0; i < 9; ++i)
                    _columns[j][i] = (i, j);
            }

            for (byte k = 0; k < 9; ++k)
            {
                _squares[k] = new (byte, byte)[9];
                for (byte l = 0; l < 9; ++l)
                {
                    var bCol = k%3;
                    var bRow = k/3;

                    var sCol = l%3;
                    var sRow = l/3;

                    _squares[k][l] = ((byte) (bCol*3 + sCol), (byte) (bRow*3 + sRow));
                }
            }

            for (byte i = 0; i < 9; ++i)
            for (byte j = 0; j < 9; ++j)
                if (_grid[i, j] > 0)
                    _possible[i, j, _grid[i, j]] = true;
                else
                    for (byte k = 1; k <= 9; ++k)
                        _possible[i, j, k] = true;
        }

        private IEnumerable<(byte, byte)[]> EnumRegions()
        {
            foreach (var a in _rows)
                yield return a;
            foreach (var a in _columns)
                yield return a;
            foreach (var a in _squares)
                yield return a;
        }

        public static void DrawGrid()
        {
            Console.ForegroundColor = Color.FromArgb(255, 32, 32, 32);
            for (var i = 0; i < 10; ++i)
            {
                Console.SetCursorPosition(0, i * 4);
                for (var j = 0; j < 55; ++j)
                {
                    var main = i % 3 == 0;
                    Console.Write(main ? '█' : '━');
                }
            }

            for (var i = 0; i < 36; ++i)
            for (var j = 0; j < 10; ++j)
            {
                var main = j % 3 == 0 || i % 12 == 0;
                Console.SetCursorPosition(j * 6, i);
                Console.Write(main ? '█' : i % 4 == 0 ? '╋' : '┃');
            }

        }

        public void Show()
        {
            for (var i = 0; i < 9; ++i)
            for (var j = 0; j < 9; ++j)
            {
                var know = _grid[i, j] > 0;
                Console.ForegroundColor = know ? Color.White : Color.DarkGray;
                for (var k = 1; k <= 9; ++k)
                {
                    Console.SetCursorPosition(j * 6 + (k - 1) % 3 * 2 + 1, i * 4 + (k - 1) / 3 + 1);
                    if (know)
                        Console.Write(k == 5 ? (char) ('0' + _grid[i, j]) : ' ');
                    else
                        Console.Write(_possible[i, j, k] ? (char)('₀' + k) : ' ');
                }
            }

            Console.WriteLine();
            Console.WriteLine();
        }

        public bool SetPossible()
        {
            var changed = false;
            foreach (var region in EnumRegions())
            {
                foreach (var (c, r) in region)
                {
                    if (_grid[c, r] != 0)
                        foreach (var (c1, r1) in region)
                            if ((c1, r1) != (c, r) && _possible[c1, r1, _grid[c, r]])
                            {
                                _possible[c1, r1, _grid[c, r]] = false;
                                changed = true;
                            }
                }
            }

            return changed;
        }

        public bool FindSingles()
        {
            bool complete = true;
            for (byte i = 0; i < 9; ++i)
            for (byte j = 0; j < 9; ++j)
                if (_grid[i, j] == 0)
                {
                    byte number = 0;
                    for (byte k = 1; k <= 9; ++k)
                        if (_possible[i, j, k])
                            if (number == 0)
                                number = k;
                            else
                            {
                                number = 0;
                                break;
                            }

                    if (number > 0)
                        _grid[i, j] = number;
                    else
                        complete = false;
                }

            return complete;
        }
    }

    static class Program
    {

        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.SetWindowSize(70, 45);
            Console.SetBufferSize(70, 45);
            Sudoku.DrawGrid();

            var s = new Sudoku(Sudoku.Medium);

            bool complete;
            do
            {
                if (!s.SetPossible())
                {
                    Console.WriteLine("I dunno!");
                    break;
                }

                complete = s.FindSingles();
                s.Show();
                //Console.ReadLine();
            } while (!complete);
        }
    }
}
