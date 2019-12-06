using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace day6
{
    class Universe
    {
        Dictionary<string, string> orbits;

        public Universe(string path) : this(File.ReadAllLines(path)) { }

        public Universe(string[] orbitDescr)
        {
            orbits = orbitDescr.Select(l =>
                {
                    var parts = l.Split(")");
                    return (parts[0], parts[1]);
                }).ToDictionary(p => p.Item2, p => p.Item1);
        }

        public int DirectOrbits => orbits.Keys.Count - 1;

        public IEnumerable<string> ParentsFor(string p)
        {
            while (p != "COM")
            {
                p = orbits[p];
                yield return p;
            }
        }

        public int IndirectOrbitsFor(string p) => ParentsFor(p).Count();

        public int IndirectOrbits => orbits
            .Keys
            .Sum(IndirectOrbitsFor);

        public IEnumerable<string> FromTo(string from, string to)
        {
            var fromParents = ParentsFor(from);
            var toParents = ParentsFor(to);
            string firstCommon = fromParents.First(p => toParents.Contains(p));
            return
                fromParents.TakeWhile(p => p != firstCommon)
                .Concat(toParents.Reverse().SkipWhile(p => p != firstCommon));
        }

        public int FromToLength(string from, string to) => FromTo(from, to).Count() - 1;

        public static void Assert<T>(T expected, T actual)
        {
            if (!expected.Equals(actual))
                throw new InvalidProgramException("Assertion Failed");
        }

        static void tests()
        {
            Assert(42, new Universe("test1.txt").IndirectOrbits);

            Assert(4, new Universe("test2.txt").FromToLength("YOU", "SAN"));
        }

        static void Main(string[] args)
        {
            tests();

            Console.WriteLine("Part 1: " + new Universe("input.txt").IndirectOrbits);
            Console.WriteLine("Part 2: " + new Universe("input.txt").FromToLength("YOU", "SAN"));

            Console.WriteLine("done");
            Console.ReadKey(true);
        }
    }
}
