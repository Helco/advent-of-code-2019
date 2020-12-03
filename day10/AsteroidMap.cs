using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace day10
{
    class AsteroidMap
    {
        IVec2[] asteroids;
        int width, height;

        AsteroidMap(string map)
        {
            var result = new List<IVec2>();
            var lines = map.Split("\n").Select(l => l.Trim()).Where(l => l.Length > 0);
            int y = 0;
            foreach (var line in lines)
            {
                int x = 0;
                foreach (var c in line.ToArray())
                {
                    if (c == '#')
                        result.Add(new IVec2(x, y));
                    x++;
                }
                y++;
            }

            this.width = lines.First().Length;
            this.height = lines.Count();
            this.asteroids = result.ToArray();
        }

        public bool IsInsideMap(IVec2 v) => v.x >= 0 && v.y >= 0 && v.x < width && v.y < height;

        public int? AsteroidAt(IVec2 v)
        { 
            for (int i = 0; i < asteroids.Length; i++)
            {
                if (asteroids[i] == v)
                    return i;
            }
            return null;
        }

        public IEnumerable<IVec2> Line(IVec2 source, IVec2 target)
        {
            var slope = (target - source).Normalized;
            var cur = source;
            do
            {
                yield return cur;
                cur += slope;
            } while (cur != target && IsInsideMap(cur));
        }

        public bool IsVisible(IVec2 source, IVec2 target)
        {
            return Line(source, target)
                .Skip(1) // we might start on an asteroid
                .All(v => AsteroidAt(v) == null);
        }

        public IVec2 BestCoordinates() => asteroids.OrderByDescending(CountVisibleAsteroids).First();
        public int BestCount() => asteroids.Max(CountVisibleAsteroids);
        public int CountVisibleAsteroids(IVec2 source) => asteroids.Except(new[] { source }).Count(t => IsVisible(source, t));

        public IEnumerable<IVec2> VaporizeAsteroids(IVec2 source)
        {
            var asteroidsLeft = asteroids.Except(new[] { source }).Select(a => a - source).ToArray();
            while (asteroidsLeft.Any())
            {
                var nextRound = asteroidsLeft
                    .GroupBy(v => v.Angle)
                    .OrderBy(v => v.Key < 0.0f ? 2 * Math.PI + v.Key : v.Key)
                    .Select(vg => vg.OrderBy(v => v.LengthSqr).First())
                    .ToArray();
                foreach (var nr in nextRound)
                    yield return source + nr;

                asteroidsLeft = asteroidsLeft.Except(nextRound).ToArray();
            }
        }

        static void Assert<T>(T expected, T actual, string id = "")
        {
            if (!expected.Equals(actual))
                Console.WriteLine($"ASSERTION FAILED {id}: {expected} != {actual}");
        }

        private const string CommonExample = @"
#.........
...#......
...#..a...
.####....a
..#.c.b...
.....c....
..efd.c.gb
.......c..
....f...c.
...e..d..c";

        private const string CommonExample2 = @"
.#..#
.....
#####
....#
...##";

        private const string CommonExample3 = @"
.#....#####...#..
##...##.#####..##
##...#...#.#####.
..#.....X...###..
..#.#.....#....##";
        private static readonly IVec2 XFor3Pos = new IVec2(8, 3);
        private static readonly IVec2 XFor4Pos = new IVec2(11, 13);

        private const string CommonExample4=@"
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
###.##.####.##.#..##";


        static void tests()
        {
            Assert(true, new AsteroidMap(CommonExample).IsVisible(IVec2.Zero, new IVec2(3, 1)), "0");
            Assert(true, new AsteroidMap(CommonExample).IsVisible(IVec2.Zero, new IVec2(3, 2)), "1");
            Assert(true, new AsteroidMap(CommonExample).IsVisible(IVec2.Zero, new IVec2(1, 3)), "2");
            Assert(true, new AsteroidMap(CommonExample).IsVisible(IVec2.Zero, new IVec2(2, 3)), "3");
            Assert(true, new AsteroidMap(CommonExample).IsVisible(IVec2.Zero, new IVec2(3, 3)), "4");
            Assert(true, new AsteroidMap(CommonExample).IsVisible(IVec2.Zero, new IVec2(4, 3)), "5");
            Assert(true, new AsteroidMap(CommonExample).IsVisible(IVec2.Zero, new IVec2(2, 4)), "6");
            Assert(false, new AsteroidMap(CommonExample).IsVisible(IVec2.Zero, new IVec2(6, 2)), "7");
            Assert(false, new AsteroidMap(CommonExample).IsVisible(IVec2.Zero, new IVec2(9, 3)), "8");
            Assert(false, new AsteroidMap(CommonExample).IsVisible(IVec2.Zero, new IVec2(4, 4)), "9");
            Assert(false, new AsteroidMap(CommonExample).IsVisible(IVec2.Zero, new IVec2(5, 5)), "a");
            Assert(false, new AsteroidMap(CommonExample).IsVisible(IVec2.Zero, new IVec2(6, 6)), "b");
            Assert(false, new AsteroidMap(CommonExample).IsVisible(IVec2.Zero, new IVec2(6, 4)), "c");
            Assert(false, new AsteroidMap(CommonExample).IsVisible(IVec2.Zero, new IVec2(9, 6)), "d");
            Assert(false, new AsteroidMap(CommonExample).IsVisible(IVec2.Zero, new IVec2(8, 6)), "e");

            Assert(7, new AsteroidMap(CommonExample2).CountVisibleAsteroids(new IVec2(1, 0)), "f");
            Assert(7, new AsteroidMap(CommonExample2).CountVisibleAsteroids(new IVec2(4, 0)), "g");
            Assert(6, new AsteroidMap(CommonExample2).CountVisibleAsteroids(new IVec2(0, 2)), "h");
            Assert(7, new AsteroidMap(CommonExample2).CountVisibleAsteroids(new IVec2(1, 2)), "i");
            Assert(7, new AsteroidMap(CommonExample2).CountVisibleAsteroids(new IVec2(2, 2)), "j");
            Assert(7, new AsteroidMap(CommonExample2).CountVisibleAsteroids(new IVec2(3, 2)), "k");
            Assert(5, new AsteroidMap(CommonExample2).CountVisibleAsteroids(new IVec2(4, 2)), "l");
            Assert(7, new AsteroidMap(CommonExample2).CountVisibleAsteroids(new IVec2(4, 3)), "m");
            Assert(7, new AsteroidMap(CommonExample2).CountVisibleAsteroids(new IVec2(4, 4)), "n");
            Assert(8, new AsteroidMap(CommonExample2).CountVisibleAsteroids(new IVec2(3, 4)), "o");

            Assert(new IVec2(3, 4), new AsteroidMap(CommonExample2).BestCoordinates());

            Assert(new IVec2(5, 8), new AsteroidMap(@"
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

            Assert(new IVec2(1, 2), new AsteroidMap(@"
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

            Assert(new IVec2(6, 3), new AsteroidMap(@"
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

            Assert(new IVec2(11, 13), new AsteroidMap(CommonExample4).BestCoordinates());

            var vaporizeList = new AsteroidMap(CommonExample3).VaporizeAsteroids(XFor3Pos).ToArray();
            var expectList = new[]
            {
                new IVec2(8, 1), new IVec2(9, 0), new IVec2(9, 1), new IVec2(10, 0), new IVec2(9, 2), new IVec2(11, 1), new IVec2(12, 1), new IVec2(11, 2), new IVec2(15, 1),
                new IVec2(12, 2), new IVec2(13, 2), new IVec2(14, 2), new IVec2(15, 2), new IVec2(12, 3), new IVec2(16, 4), new IVec2(15, 4), new IVec2(10, 4), new IVec2(4, 4),
                new IVec2(2, 4), new IVec2(2, 3), new IVec2(0, 2), new IVec2(1, 2), new IVec2(0, 1), new IVec2(1, 1), new IVec2(5, 2), new IVec2(1, 0), new IVec2(5, 1),
                new IVec2(6, 1), new IVec2(6, 0), new IVec2(7,0), new IVec2(8, 0), new IVec2(10, 1), new IVec2(14, 0), new IVec2(16, 1), new IVec2(13, 3), new IVec2(14, 3)
            };
            for (int i = 0; i < expectList.Length; i++)
                Assert(expectList[i], vaporizeList[i], $"V{i/9 + 1}.{(i%9) + 1}");

            var expectMap = new Dictionary<int, IVec2>()
            {
                {1, new IVec2(11, 12) },
                {2, new IVec2(12, 1) },
                {3, new IVec2(12, 2) },
                {10, new IVec2(12, 8) },
                {20, new IVec2(16, 0) },
                {50, new IVec2(16, 9) },
                {100, new IVec2(10, 16) },
                {199, new IVec2(9, 6) },
                {200, new IVec2(8, 2) },
                {201, new IVec2(10, 9) },
                {299, new IVec2(11, 1) }
            };
            var vaporizeList2 = new AsteroidMap(CommonExample4).VaporizeAsteroids(XFor4Pos).ToArray();
            foreach (var pair in expectMap)
                Assert(pair.Value, vaporizeList2[pair.Key - 1], $"L{pair.Key}");
        }

        static void Main(string[] args)
        {
            tests();
            Console.WriteLine("done testing");

            var map = new AsteroidMap(File.ReadAllText("input.txt"));
            Console.WriteLine(map.BestCount());

            var vaporizeList = map.VaporizeAsteroids(map.BestCoordinates()).ToArray();
            Console.WriteLine(vaporizeList[199]);
        }
    }
}
