using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;

namespace day17
{
    class Exterior
    {
        IntCode2 code;
        Dictionary<Vector2Int, char> tiles = new Dictionary<Vector2Int, char>();
        char[] map = new char[0];
        int width, height;

        Exterior()
        {
            code = new IntCode2(IntCode2.Input);
        }

        char this[Vector2Int p]
        {
            get => tiles.TryGetValue(p, out char actual) ? actual : ' ';
            set
            {
                if (InvalidPos(p))
                    throw new ArgumentException();
                tiles[p] = value;
                map[p.y * width + p.x] = value;
            }
        }

        bool InvalidPos(Vector2Int p) => p.x < 0 || p.y < 0 || p.x >= width || p.y >= height;

        void RunForInitialMap()
        {
            code.Run();
            SetMap(string.Join("", code.Output.Select(i => (char)i)));
        }

        void SetMap (string mapText)
        {
            var lines = mapText.Split("\n", StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()).Where(l => l.Length > 0).ToArray();
            width = lines[0].Length;
            height = lines.Length;
            if (lines.Any(l => l.Length != width))
                throw new InvalidProgramException("Assertion failed");
            tiles.Clear();
            map = new char[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    this[new Vector2Int(x, y)] = lines[y][x];
                }
            }
        }

        IEnumerable<Vector2Int> GetIntersections()
        {
            return tiles
                .Where(t => t.Value == '#' || t.Value == 'O')
                .Where(p => this[p.Key + Vector2Int.up] == '#')
                .Where(p => this[p.Key + Vector2Int.down] == '#')
                .Where(p => this[p.Key + Vector2Int.left] == '#')
                .Where(p => this[p.Key + Vector2Int.right] == '#')
                .Select(p => p.Key);
        }

        int AlignemntFor(Vector2Int v)
        {
            return v.x * v.y;
        }

        int IntersectionsAlignment()
        {
            return GetIntersections().Sum(AlignemntFor);
        }

        void MarkIntersections()
        {
            var intersections = GetIntersections().ToArray();
            foreach (var i in intersections)
                this[i] = 'O';
        }

        void PrintMap()
        {
            for (int y = 0; y < height; y++)
                Console.WriteLine(string.Join("", map.Skip(y * width).Take(width)));
            Console.WriteLine();
        }
        
        static void Assert<T>(T expected, T actual)
        {
            if (!expected.Equals(actual))
                throw new InvalidProgramException("Assertion failed");
        }

        static void tests()
        {
            var ex1 = new Exterior();
            ex1.SetMap(@"
..#..........
..#..........
#######...###
#.#...#...#.#
#############
..#...#...#..
..#####...^..");
            Assert(76, ex1.IntersectionsAlignment());
        }

        static void part1()
        {
            var ex = new Exterior();
            ex.RunForInitialMap();
            Console.WriteLine("Part 1: " + ex.IntersectionsAlignment());
        }

        static void Main(string[] args)
        {
            tests();
            part1();

            Console.WriteLine("done");
            Console.ReadKey(true);
        }
    }
}
