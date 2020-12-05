using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace day3
{
    class Program
    {
        public class Segment
        {
            public readonly IVec2 Dir;
            public readonly int Length;

            private static readonly Regex SegmentRegex = new Regex(@"^[DULR]\d+$", RegexOptions.Compiled);
            public Segment(string seg)
            {
                if (!SegmentRegex.IsMatch(seg))
                    throw new ArgumentException();
                Dir = seg[0] switch
                {
                    'U' => IVec2.Up,
                    'D' => IVec2.Down,
                    'L' => IVec2.Left,
                    'R' => IVec2.Right,
                    _ => throw new InvalidProgramException()
                };
                Length = int.Parse(seg[1..]);
            }

            public IEnumerable<IVec2> Points(IVec2 start) => Enumerable
                .Range(1, Length)
                .Select(i => start + Dir * i);
        }

        public class Wire
        {
            public readonly Segment[] Segments;

            public Wire(string path)
            {
                Segments = path.Trim().Split(",").Select(p => new Segment(p)).ToArray();
            }

            public IEnumerable<IVec2> Points()
            {
                var cur = IVec2.Zero;
                foreach (var segment in Segments)
                {
                    foreach (var point in segment.Points(cur))
                        yield return (cur = point);
                }
            }

            public IReadOnlyDictionary<IVec2, int> DistanceMap => Points()
                .Select((p, i) => (p, i))
                .GroupBy(t => t.p)
                .ToDictionary(t => t.Key, t => t.Min(t => t.i) + 1);
        }

        public static IEnumerable<IVec2> Intersections(Wire a, Wire b)
        {
            var aPoints = a.Points().ToHashSet();
            var bPoints = b.Points().ToHashSet();
            return aPoints.Intersect(bPoints);
        }

        public static IVec2 NearestIntersection(Wire a, Wire b) =>
            Intersections(a, b)
            .OrderBy(i => i.LengthManhattan)
            .First();

        public static int ShortestDistance(Wire a, Wire b)
        {
            var distMapA = a.DistanceMap;
            var distMapB = b.DistanceMap;
            return Intersections(a, b)
                .Select(i => distMapA[i] + distMapB[i])
                .Min();
        }

        public static void Assert<T>(T a, T b, string id)
        {
            if (!EqualityComparer<T>.Default.Equals(a, b))
                Console.WriteLine($"Assertion {id} failed: {a} != {b}");
        }

        public static void tests()
        {
            var w1a = new Wire("R8,U5,L5,D3");
            var w1b = new Wire("U7,R6,D4,L4");
            Assert(6, NearestIntersection(w1a, w1b).LengthManhattan, "w11");
            Assert(30, ShortestDistance(w1a, w1b), "w12");

            var w2a = new Wire("R75,D30,R83,U83,L12,D49,R71,U7,L72");
            var w2b = new Wire("U62,R66,U55,R34,D71,R55,D58,R83");
            Assert(159, NearestIntersection(w2a, w2b).LengthManhattan, "W21");
            Assert(610, ShortestDistance(w2a, w2b), "w22");

            var w3a = new Wire("R98,U47,R26,D63,R33,U87,L62,D20,R33,U53,R51");
            var w3b = new Wire("U98,R91,D20,R16,D67,R40,U7,R15,U6,R7");
            Assert(135, NearestIntersection(w3a, w3b).LengthManhattan, "w31");
            Assert(410, ShortestDistance(w3a, w3b), "w32");
        }

        static string[] Input => File.ReadAllLines("input.txt");
        public static void Main(string[] args)
        {
            tests();

            var inputWireA = new Wire(Input[0]);
            var inputWireB = new Wire(Input[1]);
            Console.WriteLine(NearestIntersection(inputWireA, inputWireB).LengthManhattan);
            Console.WriteLine(ShortestDistance(inputWireA, inputWireB));
        }
    }
}
