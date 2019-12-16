using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace day16
{
    class FFT
    {
        static readonly int[] BasePattern = new int[] { 0, 1, 0, -1 };

        static IEnumerable<int> PatternFor(int digit)
        {
            var tmp = BasePattern.SelectMany(d => Enumerable.Repeat(d, digit + 1));
            return tmp.Skip(1).Append(tmp.First());
        }

        static int PatternDigitAt(int i, int j)
        {
            i++;
            j++;
            int jModulated = j % (BasePattern.Length * i);
            int jDigit = jModulated / i;
            return BasePattern[jDigit];
        }

        static List<int[]> Patterns = new List<int[]>();

        static int ActualPhaseDigit(int[] digits, int i, int digitCount = -1)
        {
            if (digitCount < 0)
                digitCount = digits.Length;
            return Enumerable.Range(0, digitCount)
                .Select(j => digits[j % digits.Length] * PatternDigitAt(i, j))
                .Sum();
        }

        static int euklid(int a, int b)
        {
            if (a == 0)
                return b;
            while (b != 0)
            {
                if (a > b)
                    a = a - b;
                else
                    b = b - a;
            }
            return a;
        }

        static int smallestMultiple(int a, int b)
        {
            return (a * b) / euklid(a, b);
        }

        static int PhaseDigit(int[] digits, int i, int digitCount = -1)
        {
            if (digitCount < 0)
                digitCount = digits.Length;
            int patternLength = BasePattern.Length * (i + 1);
            int calculateLength = Math.Min(digitCount, smallestMultiple(digitCount, patternLength));
            return Math.Abs(ActualPhaseDigit(digits, i, digitCount) * (digitCount / calculateLength)) % 10;
        }

        static string Phase(string input, int repeatBy = 1)
        {
            var digits = input
                .ToCharArray()
                .Select(c => int.Parse("" + c))
                .ToArray();
            return string.Join<string>("", digits.Select((_, i) => PhaseDigit(digits, i, input.Length * repeatBy).ToString()));
        }

        static string Phases(string input, int count, int repeatBy = 1)
        {
            for (int i = 0; i < count; i++)
            {
                input = Phase(input, repeatBy);
                if (input.Length > 100)
                    Console.WriteLine("Phase " + i);
            }
            return input;
        }

        static void Assert<T>(T expected, T actual)
        {
            if (!expected.Equals(actual))
                throw new InvalidProgramException("Assertion failed");
        }

        static void AssertStart(string expected, string full)
        {
            if (!full.StartsWith(expected))
                throw new InvalidProgramException("Assertion failed");
        }

        static void tests()
        {
            Assert("48226158", Phase("12345678"));
            Assert("34040438", Phase("48226158"));
            Assert("03415518", Phase("34040438"));
            Assert("01029498", Phase("03415518"));

            AssertStart("24176176", Phases("80871224585914546619083218645595", 100));
            AssertStart("73745418", Phases("19617804207202209144916044189917", 100));
            AssertStart("52432133", Phases("69317163492948606335995924319873", 100));

            AssertStart("84462026", Phases("03036732577212944063491565474664", 100, 10000));
            AssertStart("78725270", Phases("02935109699940807407585447034323", 100, 10000));
            AssertStart("53553731", Phases("03081770884921959731165446850517", 100, 10000));
        }

        static string Input = File.ReadAllText("input.txt");

        static void part2()
        {
            int offset = int.Parse(Input.Substring(0, 7));
            string output = Phases(Input, 100, 10000);
            Console.WriteLine("Part 2: " + output.Substring(offset, 8));
        }

        static void Main(string[] args)
        {
            tests();

            Console.WriteLine("Part 1: " + Phases(Input, 100).Substring(0, 8));
            part2();

            Console.WriteLine("done");
            Console.ReadKey(true);
        }
    }
}
