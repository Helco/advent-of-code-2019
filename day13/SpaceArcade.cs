using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;

namespace day13
{
    enum Tile
    {
        Empty = 0,
        Wall,
        Block,
        HorizontalPaddle,
        Ball
    }

    class SpaceArcade
    {
        static readonly IReadOnlyDictionary<Tile, char> TileToChar = new Dictionary<Tile, char>()
        {
            { Tile.Empty, ' ' },
            { Tile.Wall, '#' },
            { Tile.Block, 'X' },
            { Tile.HorizontalPaddle, '_' },
            { Tile.Ball, 'o' }
        };

        Dictionary<Vector2Int, Tile> tiles = new Dictionary<Vector2Int, Tile>();
        IntCode2 code;
        
        public int Score { get; private set; }

        public SpaceArcade(params long[] program)
        {
            code = new IntCode2(program);
        }

        public Tile this[Vector2Int pos]
        {
            get => tiles.GetValueOrDefault(pos, Tile.Empty);
            set => tiles[pos] = value;
        }

        public Tile this[int x, int y]
        {
            get => this[new Vector2Int(x, y)];
            set => this[new Vector2Int(x, y)] = value;
        }

        public void Run(params long[] input)
        {
            code.Run(input);
            var output = code.Output.ToArray();
            for (int i = 0; i < output.Length; i += 3)
            {
                var x = (int)output[i + 0];
                var y = (int)output[i + 1];
                var value = (int)output[i + 2];
                if (x == -1 && y == 0)
                    Score = value;
                else
                    this[x, y] = (Tile)value;
            }
        }

        public void PrintMap()
        {
            int minX = tiles.Min(p => p.Key.x);
            int maxX = tiles.Max(p => p.Key.x);
            int minY = tiles.Min(p => p.Key.y);
            int maxY = tiles.Max(p => p.Key.y);
            int width = maxX - minX + 1;

            for (int y = minY; y <= maxY; y++)
            {
                var line = new string(' ', width).ToCharArray();
                foreach (var p in tiles.Where(p => p.Key.y == y))
                    line[p.Key.x] = TileToChar[p.Value];
                Console.WriteLine(line);
            }
            Console.WriteLine("Score: " + Score);
        }

        static void part1()
        {
            var arcade = new SpaceArcade(IntCode2.Input);
            arcade.Run();
            Console.WriteLine("Part 1: " + arcade.tiles.Count(p => p.Value == Tile.Block));
        }

        static void part2_play()
        {
            var hackedInput = IntCode2.Input;
            hackedInput[0] = 2;
            var arcade = new SpaceArcade(hackedInput);

            int nextInput = 0;
            while (!arcade.code.HasHalted)
            {
                Console.Clear();
                arcade.Run(nextInput);
                arcade.PrintMap();

                System.Threading.Thread.Sleep(240);
                nextInput = 0;
                while (Console.KeyAvailable && nextInput == 0)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.LeftArrow)
                        nextInput = -1;
                    if (key.Key == ConsoleKey.RightArrow)
                        nextInput = 1;
                }
                while (Console.KeyAvailable)
                    Console.ReadKey(true);
            }
        }

        long BallMemoryAddr()
        {
            Vector2Int ballPos = tiles.First(p => p.Value == Tile.Ball).Key;
            return code.Map.First(p => p.Value == ballPos.x && code[p.Key + 1] == ballPos.y).Key;
        }

        static void part2_analyse()
        {
            var hackedInput = IntCode2.Input;
            hackedInput[0] = 2;
            var arcade = new SpaceArcade(hackedInput);

            int nextInput = 0;
            while (!arcade.code.HasHalted)
            {
                Console.Clear();
                arcade.Run(nextInput);
                arcade.PrintMap();


                var ballMemAddr = arcade.BallMemoryAddr();
                Console.WriteLine("Ball Memory pos: {0} with {1},{2}", ballMemAddr, arcade.code[ballMemAddr + 0], arcade.code[ballMemAddr + 1]);

                System.Threading.Thread.Sleep(240);
                nextInput = 0;
                while (Console.KeyAvailable && nextInput == 0)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.LeftArrow)
                        nextInput = -1;
                    if (key.Key == ConsoleKey.RightArrow)
                        nextInput = 1;
                }
                while (Console.KeyAvailable)
                    Console.ReadKey(true);
            }
        }

        System.Random r = new System.Random();

        void RunHacked(int nextInput)
        {
            Run(r.Next(-1, 2));
            var ballMemAddr = BallMemoryAddr();
            //Console.WriteLine("Ball Memory pos: {0} with {1},{2}", ballMemAddr, arcade.code[ballMemAddr + 0], arcade.code[ballMemAddr + 1]);
            if (code[ballMemAddr + 1] >= 22)
            {
                code[ballMemAddr + 1] = 1;
                Vector2Int ballPos = tiles.First(p => p.Value == Tile.Ball).Key;
                this[ballPos] = Tile.Empty;
                ballPos.y = 1;
                this[ballPos] = Tile.Ball;
            }
            for (int i = 0; i < 30; i++)
                this[i, 21] = Tile.Wall;
        }
        

        static void part2_hacked()
        {
            var hackedInput = IntCode2.Input;
            hackedInput[0] = 2;
            var arcade = new SpaceArcade(hackedInput);

            arcade.RunHacked(0);
            
            for (int i = 0; i < 1000000; i++)
                arcade.RunHacked(0);

            int nextInput = 0;
            while (!arcade.code.HasHalted)
            {
                arcade.RunHacked(nextInput);
                Console.Clear();
                arcade.PrintMap();

                System.Threading.Thread.Sleep(24);
                nextInput = 0;
                while (Console.KeyAvailable && nextInput == 0)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.LeftArrow)
                        nextInput = -1;
                    if (key.Key == ConsoleKey.RightArrow)
                        nextInput = 1;
                }
                while (Console.KeyAvailable)
                    Console.ReadKey(true);
            }
            Console.Clear();
            arcade.PrintMap();
        }

        static void Main(string[] args)
        {
            IntCode2.tests(args);
            part2_hacked();

            Console.WriteLine("done");
            Console.ReadKey(true);
        }
    }
}
