using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;

namespace day10
{
    class AsteroidMap
    {
        Vector2Int[] asteroids;
        int width, height;

        AsteroidMap(string map)
        {
            var result = new List<Vector2Int>();
            var lines = map.Split("\n").Select(l => l.Trim()).Where(l => l.Length > 0);
            int y = 0;
            foreach (var line in lines)
            {
                int x = 0;
                foreach (var c in line.ToArray())
                {
                    if (c == '#')
                        result.Add(new Vector2Int(x, y));
                    x++;
                }
                y++;
            }

            this.width = lines.First().Length;
            this.height = lines.Count();
            this.asteroids = result.ToArray();
        }

        public bool IsInsideMap(Vector2Int v) => v.x >= 0 && v.y >= 0 && v.x < width && v.y < height;

        public int? AsteroidAt(Vector2Int v)
        { 
            for (int i = 0; i < asteroids.Length; i++)
            {
                if (asteroids[i] == v)
                    return i;
            }
            return null;
        }

        public IEnumerable<int> VisibleFrom(int rootI)
        {
            var visible = Enumerable.Range(0, asteroids.Length).ToHashSet();
            for (int i = 0; i < asteroids.Length; i++)
            {
                if (i == rootI)
                    continue;

                Vector2Int step = asteroids[i] - asteroids[rootI];
                Vector2Int cur = asteroids[i] + step;
                while (IsInsideMap(cur))
                {
                    var invisible = AsteroidAt(cur);
                    if (invisible.HasValue)
                        visible.Remove(invisible.Value);
                    cur += step;
                }
            }
            return visible;
        }

        public int BestIndex()
        {
            return Enumerable
                .Range(0, asteroids.Length)
                .Select(i => (index: i, visible: VisibleFrom(i).ToArray()))
                .OrderByDescending(p => p.visible.Length)
                .First().index;
        }

        public Vector2Int BestCoordinates()
        {
            Vector2 c = asteroids[BestIndex()];
            return new Vector2Int((int)c.x, (int)c.y);
        }

        static void Assert<T>(T expected, T actual)
        {
            if (!expected.Equals(actual))
                throw new InvalidProgramException("Assertion failed");
        }

        static void tests()
        {
            Assert(new Vector2Int(3, 4), new AsteroidMap(@"
                .#..#
                .....
                #####
                ....#
                ...##").BestCoordinates());

            Assert(new Vector2Int(5, 8), new AsteroidMap(@"
                ......#.#.
                #..#.#....
                ..#######.
                .#.#.###..
                .#..#.....
                ..#....#.#
                #..#....#.
                .##.#..###
                ##...#..#.
                .#....####").BestCoordinates());

            Assert(new Vector2Int(1, 2), new AsteroidMap(@"
                #.#...#.#.
                .###....#.
                .#....#...
                ##.#.#.#.#
                ....#.#.#.
                .##..###.#
                ..#...##..
                ..##....##
                ......#...
                .####.###.").BestCoordinates());

            Assert(new Vector2Int(6, 3), new AsteroidMap(@"
                .#..#..###
                ####.###.#
                ....###.#.
                ..###.##.#
                ##.##.#.#.
                ....###..#
                ..#.#..#.#
                #..#.#.###
                .##...##.#
                .....#.#..").BestCoordinates());

            Assert(new Vector2Int(11, 13), new AsteroidMap(@"
                .#..##.###...#######
                ##.############..##.
                .#.######.########.#
                .###.#######.####.#.
                #####.##.#.##.###.##
                ..#####..#.#########
                ####################
                #.####....###.#.#.##
                ##.#################
                #####.##.###..####..
                ..######..##.#######
                ####.##.####...##..#
                .#####..#.######.###
                ##...#.##########...
                #.##########.#######
                .####.#.###.###.#.##
                ....##.##.###..#####
                .#.#.###########.###
                #.#.#.#####.####.###
                ###.##.####.##.#..##").BestCoordinates());
        }

        static void Main(string[] args)
        {
            tests();
            Console.WriteLine("done");
            Console.ReadKey(true);
        }
    }
}
