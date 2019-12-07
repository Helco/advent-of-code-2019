using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace day7
{
    class ThrusterSequence
    {
        int[] program;

        public ThrusterSequence(params int[] program)
        {
            this.program = program;
        }

        public int Run(params int[] sequence)
        {
            int input = 0;
            foreach (var phase in sequence)
            {
                var code = new IntCode2(program.ToArray());
                code.Run(phase, input);
                input = code.Output.First();
            }
            return input;
        }

        public int RunFL(params int[] sequence)
        {
            var codes = Enumerable.Range(0, sequence.Length).Select(_ => new IntCode2(program.ToArray())).ToArray();
            int signal = 0;
            bool isFirst = true;
            do
            {
                for (int i = 0; i < sequence.Length; i++)
                {
                    try
                    {
                        var input = Enumerable.Empty<int>();
                        if (isFirst)
                            input = input.Append(sequence[i]);
                        input = input.Append(signal);
                        codes[i].Run(input.ToArray());
                    }
                    catch (IntCode2.NoInputException)
                    { }
                    signal = codes[i].Output.First();
                }
                isFirst = false;
            } while (codes.All(c => !c.HasHalted));
            return codes.Last().Output.First();
        }

        public void AssertRun(int expected, params int[] sequence)
        {
            int actual = Run(sequence);
            if (actual != expected)
                throw new InvalidProgramException("Assertion failed");
        }

        public void AssertRunFL(int expected, params int[] sequence)
        {
            int actual = RunFL(sequence);
            if (actual != expected)
                throw new InvalidProgramException("Assertion failed");
        }

        public int FindHighestAllowDoubles(int length, int phaseCount)
        {
            return _FindHighest(new int[0], length, phaseCount);

            int _FindHighest(IEnumerable<int> prefix, int length, int phaseCount)
            {
                if (length == 0)
                    return Run(prefix.ToArray());
                return Enumerable
                    .Range(0, phaseCount)
                    .Max(i => _FindHighest(prefix.Append(i), length - 1, phaseCount));
            }
        }

        public int FindHighest(int length, int phaseCount)
        {
            return _FindHighest(new int[0], length, phaseCount);

            int _FindHighest(IEnumerable<int> prefix, int length, int phaseCount)
            {
                if (length == 0)
                {
                    if (prefix.ToHashSet().Count() != phaseCount)
                        return 0;
                    return Run(prefix.ToArray());
                }
                return Enumerable
                    .Range(0, phaseCount)
                    .Max(i => _FindHighest(prefix.Append(i), length - 1, phaseCount));
            }
        }

        public int FindHighestFL(int length, int phaseCount, int offset)
        {
            return _FindHighest(new int[0], length, phaseCount, offset);

            int _FindHighest(IEnumerable<int> prefix, int length, int phaseCount, int offset)
            {
                if (length == 0)
                {
                    if (prefix.ToHashSet().Count() != phaseCount)
                        return 0;
                    return RunFL(prefix.ToArray());
                }
                return Enumerable
                    .Range(offset, phaseCount)
                    .Max(i => _FindHighest(prefix.Append(i), length - 1, phaseCount, offset));
            }
        }

        static void tests()
        {
            new ThrusterSequence(3, 15, 3, 16, 1002, 16, 10, 16, 1, 16, 15, 15, 4, 15, 99, 0, 0)
                .AssertRun(43210, 4, 3, 2, 1, 0);

            new ThrusterSequence(3, 23, 3, 24, 1002, 24, 10, 24, 1002, 23, -1, 23, 101, 5, 23, 23, 1, 24, 23, 23, 4, 23, 99, 0, 0)
                .AssertRun(54321, 0, 1, 2, 3, 4);

            new ThrusterSequence(3, 31, 3, 32, 1002, 32, 10, 32, 1001, 31, -2, 31, 1007, 31, 0, 33, 1002, 33, 7, 33, 1, 33, 31, 31, 1, 32, 31, 31, 4, 31, 99, 0, 0, 0)
                .AssertRun(65210, 1, 0, 4, 3, 2);

            new ThrusterSequence(3, 26, 1001, 26, -4, 26, 3, 27, 1002, 27, 2, 27, 1, 27, 26, 27, 4, 27, 1001, 28, -1, 28, 1005, 28, 6, 99, 0, 0, 5)
                .AssertRunFL(139629729, 9, 8, 7, 6, 5);

            new ThrusterSequence(
                3, 52, 1001, 52, -5, 52, 3, 53, 1, 52, 56, 54, 1007, 54, 5, 55, 1005, 55, 26, 1001, 54,
                -5, 54, 1105, 1, 12, 1, 53, 54, 53, 1008, 54, 0, 55, 1001, 55, 1, 55, 2, 53, 55, 53, 4,
                53, 1001, 56, -1, 56, 1005, 56, 6, 99, 0, 0, 0, 0, 10)
                .AssertRunFL(18216, 9, 7, 8, 5, 6);
        }

        public static int[] Input => File.ReadAllText("input.txt").Split(",").Select(t => Convert.ToInt32(t)).ToArray();

        static void part1()
        {
            var t = new ThrusterSequence(Input);
            Console.WriteLine("Part 1: " + t.FindHighest(5, 5));
        }

        static void part2()
        {
            var t = new ThrusterSequence(Input);
            Console.WriteLine("Part 2: " + t.FindHighestFL(5, 5, 5));
        }

        static void Main(string[] args)
        {
            IntCode2.tests();
            tests();
            part2();

            Console.WriteLine("done");
            Console.ReadKey(true);
        }
    }
}
