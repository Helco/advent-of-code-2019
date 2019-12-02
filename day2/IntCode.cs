using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace day2
{
    class IntCode
    {
        Dictionary<int, int> map = new Dictionary<int, int>();
        int pc = 0;
        bool halted = false;

        public IntCode(params int[] initial)
        {
            for (int i = 0; i < initial.Length; i++)
                this[i] = initial[i];
        }

        public int[] ToArray()
        {
            int max = map.Max(p => p.Key);
            int[] result = new int[max + 1];
            foreach (var p in map)
                result[p.Key] = p.Value;
            return result;
        }

        public IntCode AssertAgainst(params int[] against)
        {
            int[] check = ToArray();
            int i = 0;
            for (; i < Math.Min(check.Length, against.Length); i++)
                if (check[i] != against[i])
                    throw new InvalidProgramException("Assertion failed");
            for (; i < Math.Max(check.Length, against.Length); i++)
            {
                if ((i < check.Length && check[i] != 0) || (i < against.Length && against[i] != 0))
                    throw new InvalidProgramException("Assertion failed");
            }
            return this;
        }

        public IntCode AssertHalted()
        {
            if (!halted)
                throw new InvalidProgramException("Assertion failed");
            return this;
        }

        public int this[int index]
        {
            get => map.TryGetValue(index, out int result) ? result : 0;
            set => map[index] = value;
        }

        public IntCode Step()
        {
            if (this[pc] == 1)
            {
                this[this[pc + 3]] = this[this[pc + 1]] + this[this[pc + 2]];
                pc += 4;
            }
            else if (this[pc] == 2)
            {
                this[this[pc + 3]] = this[this[pc + 1]] * this[this[pc + 2]];
                pc += 4;
            }
            else if (this[pc] == 99)
                halted = true;
            else
                throw new InvalidOperationException("Invalid opcode: " + this[pc]);
            return this;
        }

        public IntCode Run()
        {
            while (!halted) Step();
            return this;
        }

        static void tests(string[] args)
        {
            new IntCode(1, 9, 10, 3, 2, 3, 11, 0, 99, 30, 40, 50)
                .Step().AssertAgainst(1, 9, 10, 70, 2, 3, 11, 0, 99, 30, 40, 50)
                .Step().AssertAgainst(3500, 9, 10, 70, 2, 3, 11, 0, 99, 30, 40, 50)
                .Step().AssertHalted();

            new IntCode(1, 0, 0, 0, 99)
                .Step().AssertAgainst(2, 0, 0, 0, 99);

            new IntCode(2, 4, 4, 5, 99, 0)
                .Step().AssertAgainst(2, 4, 4, 5, 99, 9801);

            new IntCode(1, 1, 1, 4, 99, 5, 6, 0, 99)
                .Run().AssertAgainst(30, 1, 1, 4, 2, 5, 6, 0, 99);
        }

        public static int[] Input => File.ReadAllText("input.txt").Split(",").Select(t => Convert.ToInt32(t)).ToArray();

        static void part1(string[] args)
        {
            int[] code = Input;
            code[1] = 12;
            code[2] = 2;
            var p = new IntCode(code);
            p.Run();
            Console.WriteLine("Part 1: " + p[0]);
        }

        static void part2(string[] args)
        {
            for (int noun = 0; noun < 100; noun++)
            {
                for (int verb = 0; verb < 100; verb++)
                {
                    try
                    {
                        var code = Input;
                        code[1] = noun;
                        code[2] = verb;
                        var p = new IntCode(code);
                        p.Run();
                        if (p[0] == 19690720)
                        {
                            Console.WriteLine("Part2 Noun: {0} Verb {1}", noun, verb);
                            return;
                        }
                    }
                    catch (InvalidOperationException)
                    {}
                }
            }
            Console.WriteLine("Part 2 Did not found anything");
        }

        static void Main(string[] args)
        {
            tests(args);
            part1(args);
            part2(args);
            Console.WriteLine("done");
            Console.ReadKey(true);
        }
    }
}
