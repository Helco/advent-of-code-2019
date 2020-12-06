using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace day12p2
{
    class Program
    {
        public struct State : IEquatable<State>
        {
            public readonly int[] pos, vel;

            public State(int[] pos)
            {
                this.pos = pos.ToArray();
                vel = Enumerable.Repeat(0, pos.Length).ToArray();
            }

            public State(int[] pos, int[] vel)
            {
                this.pos = pos;
                this.vel = vel;
            }

            public State(State prev)
            {
                pos = prev.pos.ToArray();
                vel = prev.vel.ToArray();

                for (int i = 0; i < vel.Length; i++)
                {
                    for (int j = 0; j < vel.Length; j++)
                        vel[i] += Math.Sign(pos[j] - pos[i]);
                }
                for (int i = 0; i < vel.Length; i++)
                    pos[i] += vel[i];
            }

            public bool Equals([AllowNull] State other) => this == other;
            public override bool Equals([AllowNull] object other) => other is State ? this == (State)other : false;
            public static bool operator ==(State a, State b) => a.pos.SequenceEqual(b.pos) && a.vel.SequenceEqual(b.vel);
            public static bool operator !=(State a, State b) => !(a == b);
            public override int GetHashCode() => (int)pos.Concat(vel).Aggregate(2166136261, (acc, cur) => (acc * 16777619) ^ (uint)cur);
        }

        static int StepsUntilCycle(int[] pos)
        {
            var state = new State(pos);
            var history = new HashSet<State>() { state };
            while (true)
            {
                if (!history.Add(state = new State(state)))
                    return history.Count;
            }
        }

        static long LeastCommonMultiple(int[] init)
        {
            var set = init.Select(i => (long)i).ToArray();
            return init.Count() > 2 ? LeastCommonMultiple(init.First(), LeastCommonMultiple(init.Skip(1).ToArray())) : LeastCommonMultiple(init[0], init[1]);
        }
        static long LeastCommonMultiple(long a, long b)
        {
            return a * b / GreatestCommonDivisor(a, b);
        }

        private static long GreatestCommonDivisor(long a, long b)
        {
            long h;
            if (a == 0) return Math.Abs(b);
            if (b == 0) return Math.Abs(a);

            do
            {
                h = a % b;
                a = b;
                b = h;
            } while (b != 0);

            return Math.Abs(a);
        }

        static long RepeatingUniverse(int[][] moons)
        {
            int[] axisCycles = Enumerable
                .Range(0, moons.First().Length)
                .Select(i => moons.Select(m => m[i]).ToArray())
                .Select(axis => StepsUntilCycle(axis))
                .ToArray();

            return LeastCommonMultiple(axisCycles);
        }

        static void Assert<T>(T a, T b, string id)
        {
            if (EqualityComparer<T>.Default.Equals(a, b))
                return;
            Console.WriteLine($"Assertion failed: {a} != {b} @ {id}");
        }

        static void Main(string[] args)
        {
            var example1 = new[]
            {
                new[] { -1, 0, 2 },
                new[] { 2, -10, -7 },
                new[] { 4, -8, 8 },
                new[] { 3, 5, -1 }
            };
            Assert(2772L, RepeatingUniverse(example1), "ex1");

            var lcmTest1 = new[] { 3, 4, 6 };
            Assert(12, LeastCommonMultiple(lcmTest1), "lcm1");
            Assert(84, LeastCommonMultiple(new[] { 4, 7, 12, 21, 42 }), "lcm2");
            Assert(2520, LeastCommonMultiple(new[] { 1, 2, 3, 4, 5, 6, 7, 8 ,9, 10 }), "lcm3");

            TestExampleSim(ExampleSimText, new[]
            {
                new State(new[] { -8, 5, 2, 9 }),
                new State(new[] { -10, 5, -7, -8 }),
                new State(new[] { 0, 10, 3, -3 })
            });
            TestExampleSim(ExampleSimText2, new[]
            {
                new State(new[] { -1, 2, 4, 3 }),
                new State(new[] { 0, -10, -8, 5 }),
                new State(new[] { 2, -7, 8, -1 })
            });

            var input = new[]
            {
                new[] { -5, 6, -11 },
                new[] { -8, -4, -2 },
                new[] { 1, 16, 4 },
                new[] { 11, 11, -4 }
            };
            Console.WriteLine(RepeatingUniverse(input));
        }

        private static void TestExampleSim(string text, State[] initStates)
        {
            foreach (var pair in ParseExample(ExampleSimText))
            {
                var states = initStates.ToArray();
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < pair.Key; j++)
                        states[i] = new State(states[i]);
                    Assert(pair.Value[i], states[i], $"Sim{pair.Key}_{i}");
                }
            }
        }

        private static Dictionary<int, State[]> ParseExample(string simText)
        {
            var lines = simText.Split("\n");
            var curSteps = -1;
            var curMatches = new List<Match>();
            var result = new Dictionary<int, State[]>();
            foreach (var line in lines)
            {
                var headerMatch = HeaderRegex.Match(line);
                if (headerMatch.Success)
                {
                    if (curMatches.Count > 0)
                        ConvertCurrent();
                    curMatches = new List<Match>();
                    curSteps = int.Parse(headerMatch.Groups[1].Value);
                    continue;
                }
                else if (curSteps < 0)
                    continue;

                var moonMatch = MoonRegex.Match(line);
                if (moonMatch.Success)
                    curMatches.Add(moonMatch);
            }
            if (curMatches.Count > 0)
                ConvertCurrent();
            return result;

            void ConvertCurrent()
            {
                var moonCount = curMatches.Count;
                result.Add(curSteps, Enumerable
                    .Range(0, 3)
                    .Select(i => new State(
                        curMatches.Select(m => int.Parse(m.Groups[1 + i].Value)).ToArray(),
                        curMatches.Select(m => int.Parse(m.Groups[4 + i].Value)).ToArray()))
                    .ToArray());
            }
        }

        private const string ExampleSimText2 = @"
After 0 steps:
pos=<x= -1, y=  0, z=  2>, vel=<x=  0, y=  0, z=  0>
pos=<x=  2, y=-10, z= -7>, vel=<x=  0, y=  0, z=  0>
pos=<x=  4, y= -8, z=  8>, vel=<x=  0, y=  0, z=  0>
pos=<x=  3, y=  5, z= -1>, vel=<x=  0, y=  0, z=  0>

After 2770 steps:
pos=<x=  2, y= -1, z=  1>, vel=<x= -3, y=  2, z=  2>
pos=<x=  3, y= -7, z= -4>, vel=<x=  2, y= -5, z= -6>
pos=<x=  1, y= -7, z=  5>, vel=<x=  0, y= -3, z=  6>
pos=<x=  2, y=  2, z=  0>, vel=<x=  1, y=  6, z= -2>

After 2771 steps:
pos=<x= -1, y=  0, z=  2>, vel=<x= -3, y=  1, z=  1>
pos=<x=  2, y=-10, z= -7>, vel=<x= -1, y= -3, z= -3>
pos=<x=  4, y= -8, z=  8>, vel=<x=  3, y= -1, z=  3>
pos=<x=  3, y=  5, z= -1>, vel=<x=  1, y=  3, z= -1>

After 2772 steps:
pos=<x= -1, y=  0, z=  2>, vel=<x=  0, y=  0, z=  0>
pos=<x=  2, y=-10, z= -7>, vel=<x=  0, y=  0, z=  0>
pos=<x=  4, y= -8, z=  8>, vel=<x=  0, y=  0, z=  0>
pos=<x=  3, y=  5, z= -1>, vel=<x=  0, y=  0, z=  0>";

        private static readonly Regex HeaderRegex = new Regex(@" ^ After (\d+) steps", RegexOptions.Compiled);
        private static readonly Regex MoonRegex = new Regex(@"^pos=<x=\s*(-?\d+), y=\s*(-?\d+), z=\s*(-?\d+)>, vel=<x=\s*(-?\d+), y=\s*(-?\d+), z=\s*(-?\d+)>");
        private const string ExampleSimText = @"
After 0 steps:
pos=<x= -8, y=-10, z=  0>, vel=<x=  0, y=  0, z=  0>
pos=<x=  5, y=  5, z= 10>, vel=<x=  0, y=  0, z=  0>
pos=<x=  2, y= -7, z=  3>, vel=<x=  0, y=  0, z=  0>
pos=<x=  9, y= -8, z= -3>, vel=<x=  0, y=  0, z=  0>

After 10 steps:
pos=<x= -9, y=-10, z=  1>, vel=<x= -2, y= -2, z= -1>
pos=<x=  4, y= 10, z=  9>, vel=<x= -3, y=  7, z= -2>
pos=<x=  8, y=-10, z= -3>, vel=<x=  5, y= -1, z= -2>
pos=<x=  5, y=-10, z=  3>, vel=<x=  0, y= -4, z=  5>

After 20 steps:
pos=<x=-10, y=  3, z= -4>, vel=<x= -5, y=  2, z=  0>
pos=<x=  5, y=-25, z=  6>, vel=<x=  1, y=  1, z= -4>
pos=<x= 13, y=  1, z=  1>, vel=<x=  5, y= -2, z=  2>
pos=<x=  0, y=  1, z=  7>, vel=<x= -1, y= -1, z=  2>

After 30 steps:
pos=<x= 15, y= -6, z= -9>, vel=<x= -5, y=  4, z=  0>
pos=<x= -4, y=-11, z=  3>, vel=<x= -3, y=-10, z=  0>
pos=<x=  0, y= -1, z= 11>, vel=<x=  7, y=  4, z=  3>
pos=<x= -3, y= -2, z=  5>, vel=<x=  1, y=  2, z= -3>

After 40 steps:
pos=<x= 14, y=-12, z= -4>, vel=<x= 11, y=  3, z=  0>
pos=<x= -1, y= 18, z=  8>, vel=<x= -5, y=  2, z=  3>
pos=<x= -5, y=-14, z=  8>, vel=<x=  1, y= -2, z=  0>
pos=<x=  0, y=-12, z= -2>, vel=<x= -7, y= -3, z= -3>

After 50 steps:
pos=<x=-23, y=  4, z=  1>, vel=<x= -7, y= -1, z=  2>
pos=<x= 20, y=-31, z= 13>, vel=<x=  5, y=  3, z=  4>
pos=<x= -4, y=  6, z=  1>, vel=<x= -1, y=  1, z= -3>
pos=<x= 15, y=  1, z= -5>, vel=<x=  3, y= -3, z= -3>

After 60 steps:
pos=<x= 36, y=-10, z=  6>, vel=<x=  5, y=  0, z=  3>
pos=<x=-18, y= 10, z=  9>, vel=<x= -3, y= -7, z=  5>
pos=<x=  8, y=-12, z= -3>, vel=<x= -2, y=  1, z= -7>
pos=<x=-18, y= -8, z= -2>, vel=<x=  0, y=  6, z= -1>

After 70 steps:
pos=<x=-33, y= -6, z=  5>, vel=<x= -5, y= -4, z=  7>
pos=<x= 13, y= -9, z=  2>, vel=<x= -2, y= 11, z=  3>
pos=<x= 11, y= -8, z=  2>, vel=<x=  8, y= -6, z= -7>
pos=<x= 17, y=  3, z=  1>, vel=<x= -1, y= -1, z= -3>

After 80 steps:
pos=<x= 30, y= -8, z=  3>, vel=<x=  3, y=  3, z=  0>
pos=<x= -2, y= -4, z=  0>, vel=<x=  4, y=-13, z=  2>
pos=<x=-18, y= -7, z= 15>, vel=<x= -8, y=  2, z= -2>
pos=<x= -2, y= -1, z= -8>, vel=<x=  1, y=  8, z=  0>

After 90 steps:
pos=<x=-25, y= -1, z=  4>, vel=<x=  1, y= -3, z=  4>
pos=<x=  2, y= -9, z=  0>, vel=<x= -3, y= 13, z= -1>
pos=<x= 32, y= -8, z= 14>, vel=<x=  5, y= -4, z=  6>
pos=<x= -1, y= -2, z= -8>, vel=<x= -3, y= -6, z= -9>

After 100 steps:
pos=<x=  8, y=-12, z= -9>, vel=<x= -7, y=  3, z=  0>
pos=<x= 13, y= 16, z= -3>, vel=<x=  3, y=-11, z= -5>
pos=<x=-29, y=-11, z= -1>, vel=<x= -3, y=  7, z=  4>
pos=<x= 16, y=-13, z= 23>, vel=<x=  7, y=  1, z=  1>";
    }
}
