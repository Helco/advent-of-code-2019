// Only part 1 :(
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace day3
{
    class WireGrid
    {
        public struct Segment
        {
            public char dir;
            public int length;
        }

        public struct Point
        {
            public int x, y;

            public int Length => Math.Abs(x) + Math.Abs(y);

            public Point(int xx, int yy)
            {
                this.x = xx;
                this.y = yy;
            }

            public Point(char dir)
            {
                x = 0; y = 0;
                switch(dir)
                {
                    case 'R': x = 1; break;
                    case 'L': x = -1; break;
                    case 'U': y = 1; break;
                    case 'D': y = -1; break;
                }
            }

            public static Point operator + (Point a, Point b)
            {
                return new Point(a.x + b.x, a.y + b.y);
            }
        }

        HashSet<Point> visited = new HashSet<Point>();
        HashSet<Point> intersections = new HashSet<Point>();

        public Segment[] ParsePath(string path)
        {
            return path
                .Split(",")
                .Select(s => new Segment
                {
                    dir = s[0],
                    length = Convert.ToInt32(s.Substring(1))
                })
                .ToArray();
        }

        public WireGrid Follow(Segment[] segments)
        {
            var p = new HashSet<Point>();
            Point cur = new Point(0, 0);
            foreach (var segment in segments)
            {
                Point dir = new Point(segment.dir);
                for (int i = 0; i < segment.length; i++)
                {
                    cur = cur + dir;
                    if (cur.x == 0 && cur.y == 0)
                        continue;
                    p.Add(cur);
                }
            }
            foreach (var c in p)
            {
                if (visited.Contains(c))
                    intersections.Add(c);
                else
                    visited.Add(c);
            }
            return this;
        }

        public WireGrid Follow(string path)
        {
            return Follow(ParsePath(path));
        }

        public Point GetShortest()
        {
            return intersections
                .OrderBy(p => p.Length)
                .First();
        }

        public void AssertShortestLength(int length)
        {
            var shortest = GetShortest();
            if (shortest.Length != length)
            {
                throw new InvalidProgramException("Assertion failed");
            }
        }

        static void tests()
        {
            new WireGrid()
                .Follow("R8,U5,L5,D3")
                .Follow("U7,R6,D4,L4")
                .AssertShortestLength(6);

            new WireGrid()
                .Follow("R75,D30,R83,U83,L12,D49,R71,U7,L72")
                .Follow("U62,R66,U55,R34,D71,R55,D58,R83")
                .AssertShortestLength(159);

            new WireGrid()
                .Follow("R98,U47,R26,D63,R33,U87,L62,D20,R33,U53,R51")
                .Follow("U98,R91,D20,R16,D67,R40,U7,R15,U6,R7")
                .AssertShortestLength(135);
        }

        static string[] Input => File.ReadAllLines("input.txt");

        static void Main(string[] args)
        {
            tests();
            var part1 = new WireGrid();
            foreach (var s in Input)
                part1.Follow(s);
            Console.WriteLine("Part 1: " + part1.GetShortest().Length);

            Console.WriteLine("done");
            Console.ReadKey(true);
        }
    }
}
