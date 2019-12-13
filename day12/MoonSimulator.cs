using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;

namespace day12
{
    class MoonSimulator
    {
        public class Moon
        {
            public Vector3Int position, velocity;

            public int Potential => Math.Abs(position.x) + Math.Abs(position.y) + Math.Abs(position.z);
            public int Kinectic => Math.Abs(velocity.x) + Math.Abs(velocity.y) + Math.Abs(velocity.z);
            public int TotalEnergy => Potential * Kinectic;

            public override int GetHashCode()
            {
                int prime = 31;
                int hashCode = 0;
                hashCode ^= prime * position.GetHashCode();
                hashCode ^= velocity.GetHashCode();
                return hashCode;
            }
        }

        Moon[] moons = new Moon[0];

        public int Potential => moons.Sum(m => m.Potential);
        public int Kinectic => moons.Sum(m => m.Kinectic);
        public int TotalEnergy => moons.Sum(m => m.TotalEnergy);

        public MoonSimulator(params Vector3Int[] p)
        {
            moons = p.Select(p => new Moon() { position = p, velocity = Vector3Int.zero }).ToArray();
        }

        public MoonSimulator(MoonSimulator m)
        {
            moons = m.moons.Select(m => new Moon() { position = m.position, velocity = m.velocity }).ToArray();
        }

        public void ApplyGravity(Moon a, Moon b)
        {
            void FromTo(Moon a, Moon b)
            {
                a.velocity.x += Math.Sign(b.position.x - a.position.x);
                a.velocity.y += Math.Sign(b.position.y - a.position.y);
                a.velocity.z += Math.Sign(b.position.z - a.position.z);
            }

            FromTo(a, b);
            FromTo(b, a);
        }

        public void ApplyGravity()
        {
            var pairs = moons.SelectMany(
                (a, i) => moons.Where((_, j) => j > i).Select(b => (a, b))
            ).ToArray();
            foreach (var pair in pairs)
                ApplyGravity(pair.a, pair.b);
        }

        public void ApplyVelocity()
        {
            foreach (var moon in moons)
                moon.position += moon.velocity;
        }

        public MoonSimulator FusedStep()
        {
            for (int i = 0; i < moons.Length; i++)
            {
                for (int j = i + 1; j < moons.Length; j++)
                    ApplyGravity(moons[i], moons[j]);
                moons[i].position += moons[i].velocity;
            }
            return this;
        }

        public MoonSimulator ParttedStep()
        {
            ApplyGravity();
            ApplyVelocity();
            return this;
        }

        public MoonSimulator Step()
        {
            return FusedStep();
        }

        public MoonSimulator Steps(int count)
        {
            for (int i = 0; i < count; i++)
                Step();
            return this;
        }

        public MoonSimulator StepToCopy()
        {
            return new MoonSimulator(this).FusedStep();
        }

        public bool Equals(MoonSimulator o)
        {
            if (o.moons.Length != moons.Length)
                return false;
            for (int i = 0; i < moons.Length; i++)
            {
                if (moons[i].position != o.moons[i].position || moons[i].velocity != o.moons[i].velocity)
                    return false;
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is MoonSimulator))
                return false;
            return Equals(obj as MoonSimulator);
        }

        public static long StepsToCycle(MoonSimulator first)
        {
            var steps = new HashSet<MoonSimulator>();
            steps.Add(first);
            var last = first;
            while (true)
            {
                var next = last.StepToCopy();
                if (steps.Contains(next))
                    return steps.LongCount();
                steps.Add(next);
                last = next;
            }
        }

        public static long StepsToFirstCycle(MoonSimulator first)
        {
            var cur = first.StepToCopy();
            long steps = 1;
            long lastMessage = 0;
            while (true)
            {
                steps++;
                cur.FusedStep();
                if (cur.Equals(first))
                    return steps;
                if (steps - lastMessage > 1000000 )
                {
                    Console.WriteLine(steps);
                    lastMessage = steps;
                }
            }
        }

        public MoonSimulator AssertMoons(params Vector3Int[] expected)
        {
            if (expected.Length % 2 != 0)
                throw new ArgumentException();

            for (int i = 0; i < moons.Length; i++)
            {
                if (moons[i].position != expected[i * 2 + 0])
                    throw new InvalidProgramException("Assertion failed");

                if (moons[i].velocity != expected[i * 2 + 1])
                    throw new InvalidProgramException("Assertion failed");
            }

            return this;
        }

        public MoonSimulator AssertTotalEnergy(int expected)
        {
            if (expected != TotalEnergy)
                throw new InvalidProgramException("Assertion failed");
            return this;
        }

        public static void Assert<T>(T expected, T actual)
        {
            if (!expected.Equals(actual))
                throw new InvalidProgramException("Assertion failed");
        }

        public override int GetHashCode()
        {
            int prime = 31;
            int hashCode = 0;
            foreach (var moon in moons)
                hashCode = (hashCode * prime) ^ moon.GetHashCode();
            return hashCode;
        }

        static void tests()
        {
            new MoonSimulator(
                new Vector3Int(-1, 0, 2),
                new Vector3Int(2, -10, -7),
                new Vector3Int(4, -8, 8),
                new Vector3Int(3, 5, -1))
                .Step().AssertMoons(
                new Vector3Int(2, -1, 1), new Vector3Int(3, -1, -1),
                new Vector3Int(3, -7, -4), new Vector3Int(1, 3, 3),
                new Vector3Int(1, -7, 5), new Vector3Int(-3, 1, -3),
                new Vector3Int(2, 2, 0), new Vector3Int(-1, -3, 1))
                .Step().AssertMoons(
                new Vector3Int(5, -3, -1), new Vector3Int(3, -2, -2),
                new Vector3Int(1, -2, 2), new Vector3Int(-2, 5, 6),
                new Vector3Int(1, -4, -1), new Vector3Int(0, 3, -6),
                new Vector3Int(1, -4, 2), new Vector3Int(-1, -6, 2))
                .Steps(8).AssertMoons(
                new Vector3Int(2, 1, -3), new Vector3Int(-3, -2, 1),
                new Vector3Int(1, -8, 0), new Vector3Int(-1, 1, 3),
                new Vector3Int(3, -6, 1), new Vector3Int(3, 2, -3),
                new Vector3Int(2, 0, 4), new Vector3Int(1, -1, -1))
                .AssertTotalEnergy(179);

            Assert(2772, StepsToFirstCycle(new MoonSimulator(
                new Vector3Int(-1, 0, 2),
                new Vector3Int(2, -10, -7),
                new Vector3Int(4, -8, 8),
                new Vector3Int(3, 5, -1))));
        }

        static MoonSimulator Input => new MoonSimulator(
            new Vector3Int(-5, 6, -11),
            new Vector3Int(-8, -4, -2),
            new Vector3Int(1, 16, 4),
            new Vector3Int(11, 11, -4));

        static void part1()
        {
            Console.WriteLine("Part 1:" + Input.Steps(1000).TotalEnergy);
        }

        static void part2()
        {
            Console.WriteLine("Part 2: " + StepsToFirstCycle(Input));
        }

        static void Main(string[] args)
        {
            tests();
            part1();
            part2();

            Console.WriteLine("done");
            Console.ReadKey(true);
        }
    }
}
