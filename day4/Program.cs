using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace day4
{
    static class Program
    {
        static IEnumerable<T> From<T>(params T[] elements) => elements;

        static IEnumerable<int> FromTo(int min, int max) => Enumerable.Range(min, max - min + 1);

        static IEnumerable<int> WithMinAdjacentDigits(this IEnumerable<int> source, int minCount) => source.Where(password =>
            Enumerable
                .Range(0, 10)
                .Select(digit => string.Join((char)('0' + digit), Enumerable.Repeat("", minCount + 1)))
                .Any(digits => password.ToString().Contains(digits)));
        static IEnumerable<int> WithAdjacentDigits(this IEnumerable<int> source, int minCount) => source.Where(password =>
            Enumerable
                .Range(0, 10)
                .Select(digit => string.Join((char)('0' + digit), Enumerable.Repeat("", minCount + 1)))
                .Any(digits =>
                    password.ToString().Contains(digits) &&
                    !password.ToString().Contains(digits + digits.Substring(0, 1))
                )
        );

        static IEnumerable<int> WithIncreasingDigits(this IEnumerable<int> source) => source.Where(password => password.ToString() == new string(password
            .ToString()
            .ToArray()
            .OrderBy(c => c)
            .ToArray()));

        static void AssertContains(this IEnumerable<int> source, int element)
        {
            if (!source.Contains(element))
                throw new InvalidProgramException("Assertion failed");
        }

        static void AssertDoesNotContain(this IEnumerable<int> source, int element)
        {
            if (source.Contains(element))
                throw new InvalidProgramException("Assertion failed");
        }

        static void tests()
        {
            From(111111)
                .WithMinAdjacentDigits(2)
                .WithIncreasingDigits()
                .AssertContains(111111);

            From(223450)
                .WithMinAdjacentDigits(2)
                .WithIncreasingDigits()
                .AssertDoesNotContain(223450);

            From(123789)
                .WithMinAdjacentDigits(2)
                .WithIncreasingDigits()
                .AssertDoesNotContain(123789);

            From(112233)
                .WithAdjacentDigits(2)
                .WithIncreasingDigits()
                .AssertContains(112233);

            From(123444)
                .WithAdjacentDigits(2)
                .WithIncreasingDigits()
                .AssertDoesNotContain(123444);

            From(111122)
                .WithAdjacentDigits(2)
                .WithIncreasingDigits()
                .AssertContains(111122);
        }

        static void part1()
        {
            Console.WriteLine("Part 1: {0}",
                FromTo(382345, 843167)
                .WithMinAdjacentDigits(2)
                .WithIncreasingDigits()
                .Count());
        }

        static void part2()
        {
            Console.WriteLine("Part 2: {0}",
                FromTo(382345, 843167)
                .WithAdjacentDigits(2)
                .WithIncreasingDigits()
                .Count());
        }

        static void Main(string[] args)
        {
            tests();

            part2();

            Console.WriteLine("done");
            Console.ReadKey(true);
        }
    }
}
