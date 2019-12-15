using INT = System.Int64;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;

namespace day15
{
    public class IntCode2
    {
        Dictionary<INT, INT> map = new Dictionary<INT, INT>();
        Queue<INT> input = new Queue<INT>();
        List<INT> output = new List<INT>();
        INT pc = 0, relativeBase = 0;
        bool halted = false;
        bool needsInput = false;

        Func<INT, INT>[] getters;
        Action<INT, INT>[] setters;

        public bool HasHalted => halted;
        public bool NeedsInput => needsInput;
        public IEnumerable<INT> Output => output;
        public IEnumerable<KeyValuePair<INT, INT>> Map => map;

        public IntCode2(params INT[] initial)
        {
            for (INT i = 0; i < initial.Length; i++)
                this[i] = initial[i];

            getters = new Func<INT, INT>[]
            {
                (pos) => this[pos],
                (val) => val,
                (pos) => this[pos + relativeBase]
            };

            setters = new Action<INT, INT>[]
            {
                (pos, val) => this[pos] = val,
                (pos, val) => throw new InvalidOperationException("Cannot store into immediate mode"),
                (pos, val) => this[pos + relativeBase] = val
            };
        }

        public INT[] ToArray()
        {
            INT max = map.Max(p => p.Key);
            INT[] result = new INT[max + 1];
            foreach (var p in map)
                result[p.Key] = p.Value;
            return result;
        }

        public IntCode2 AssertAgainst(params INT[] against)
        {
            INT[] check = ToArray();
            INT i = 0;
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

        public IntCode2 AssertHalted()
        {
            if (!halted)
                throw new InvalidProgramException("Assertion failed");
            return this;
        }

        public IntCode2 AssertOutput(params INT[] expectedOutput)
        {
            AssertHalted();
            if (output.Count != expectedOutput.Count())
                throw new InvalidProgramException("Assertion failed");
            for (INT i = 0; i < output.Count; i++)
            {
                if (output[(int)i] != expectedOutput[i])
                    throw new InvalidProgramException("Assertion failed");
            }
            return this;
        }

        public INT this[INT index]
        {
            get => map.TryGetValue(index, out INT result) ? result : 0;
            set => map[index] = value;
        }

        public IntCode2 Step()
        {
            INT command = this[pc];
            INT opcode = command % 100;
            var param0Mode = (command / 100) % 10;
            var param1Mode = (command / 1000) % 10;
            var param2Mode = (command / 10000) % 10;
            var param0 = (get: getters[param0Mode], set: setters[param0Mode]);
            var param1 = (get: getters[param1Mode], set: setters[param1Mode]);
            var param2 = (get: getters[param2Mode], set: setters[param2Mode]);

            INT condition;

            switch (opcode)
            {
                case 1:
                    param2.set(this[pc + 3], param0.get(this[pc + 1]) + param1.get(this[pc + 2]));
                    pc += 4;
                    break;
                case 2:
                    param2.set(this[pc + 3], param0.get(this[pc + 1]) * param1.get(this[pc + 2]));
                    pc += 4;
                    break;
                case 3:
                    if (!input.TryDequeue(out INT inputValue))
                    {
                        needsInput = true;
                        return this;
                    }
                    param0.set(this[pc + 1], inputValue);
                    pc += 2;
                    break;
                case 4:
                    output.Add(param0.get(this[pc + 1]));
                    pc += 2;
                    break;
                case 5:
                    condition = param0.get(this[pc + 1]);
                    if (condition != 0)
                        pc = param1.get(this[pc + 2]);
                    else
                        pc += 3;
                    break;
                case 6:
                    condition = param0.get(this[pc + 1]);
                    if (condition == 0)
                        pc = param1.get(this[pc + 2]);
                    else
                        pc += 3;
                    break;
                case 7:
                    param2.set(this[pc + 3], (param0.get(this[pc + 1]) < param1.get(this[pc + 2])) ? 1 : 0);
                    pc += 4;
                    break;
                case 8:
                    param2.set(this[pc + 3], (param0.get(this[pc + 1]) == param1.get(this[pc + 2])) ? 1 : 0);
                    pc += 4;
                    break;
                case 9:
                    relativeBase += param0.get(this[pc + 1]);
                    pc += 2;
                    break;
                case 99:
                    halted = true;
                    break;
                default:
                    throw new InvalidOperationException("Invalid opcode: " + this[pc]);
            }
            return this;
        }

        public IntCode2 Run(params INT[] inputArr)
        {
            needsInput = false;
            output.Clear();
            input = new Queue<INT>(inputArr);
            while (!halted && !needsInput) Step();
            return this;
        }

        public IntCode2 PrintOutput()
        {
            bool first = true;
            foreach (var o in output)
            {
                Console.WriteLine("{0}{1}", first ? "" : ", ", o);
                first = false;
            }
            return this;
        }

        public static void tests(string[] args)
        {
            new IntCode2(1, 9, 10, 3, 2, 3, 11, 0, 99, 30, 40, 50)
                .Step().AssertAgainst(1, 9, 10, 70, 2, 3, 11, 0, 99, 30, 40, 50)
                .Step().AssertAgainst(3500, 9, 10, 70, 2, 3, 11, 0, 99, 30, 40, 50)
                .Step().AssertHalted();

            new IntCode2(1, 0, 0, 0, 99)
                .Step().AssertAgainst(2, 0, 0, 0, 99);

            new IntCode2(2, 4, 4, 5, 99, 0)
                .Step().AssertAgainst(2, 4, 4, 5, 99, 9801);

            new IntCode2(1, 1, 1, 4, 99, 5, 6, 0, 99)
                .Run().AssertAgainst(30, 1, 1, 4, 2, 5, 6, 0, 99);

            new IntCode2(3, 0, 4, 0, 99)
                .Run(42).AssertOutput(42);

            new IntCode2(3, 0, 4, 0, 99)
                .Run(1337, 55).AssertOutput(1337);

            new IntCode2(1002, 4, 3, 4, 33)
                .Run().AssertAgainst(1002, 4, 3, 4, 99);

            new IntCode2(1101, 100, -1, 4, 0)
                .Run().AssertAgainst(1101, 100, -1, 4, 99);

            new IntCode2(3, 9, 8, 9, 10, 9, 4, 9, 99, -1, 8)
                .Run(8).AssertOutput(1);
            new IntCode2(3, 9, 8, 9, 10, 9, 4, 9, 99, -1, 8)
                .Run(9).AssertOutput(0);

            new IntCode2(3, 9, 7, 9, 10, 9, 4, 9, 99, -1, 8)
                .Run(7).AssertOutput(1);
            new IntCode2(3, 9, 7, 9, 10, 9, 4, 9, 99, -1, 8)
                .Run(9).AssertOutput(0);

            new IntCode2(3, 3, 1108, -1, 8, 3, 4, 3, 99)
                .Run(8).AssertOutput(1);
            new IntCode2(3, 3, 1108, -1, 8, 3, 4, 3, 99)
                .Run(9).AssertOutput(0);

            new IntCode2(3, 3, 1107, -1, 8, 3, 4, 3, 99)
                .Run(7).AssertOutput(1);
            new IntCode2(3, 3, 1107, -1, 8, 3, 4, 3, 99)
                .Run(8).AssertOutput(0);

            new IntCode2(3, 12, 6, 12, 15, 1, 13, 14, 13, 4, 13, 99, -1, 0, 1, 9)
                .Run(0).AssertOutput(0);
            new IntCode2(3, 12, 6, 12, 15, 1, 13, 14, 13, 4, 13, 99, -1, 0, 1, 9)
                .Run(1).AssertOutput(1);

            new IntCode2(3, 3, 1105, -1, 9, 1101, 0, 0, 12, 4, 12, 99, 1)
                .Run(0).AssertOutput(0);
            new IntCode2(3, 3, 1105, -1, 9, 1101, 0, 0, 12, 4, 12, 99, 1)
                .Run(1).AssertOutput(1);

            new IntCode2(
                3, 21, 1008, 21, 8, 20, 1005, 20, 22, 107, 8, 21, 20, 1006, 20, 31,
                1106, 0, 36, 98, 0, 0, 1002, 21, 125, 20, 4, 20, 1105, 1, 46, 104,
                999, 1105, 1, 46, 1101, 1000, 1, 20, 4, 20, 1105, 1, 46, 98, 99)
                .Run(7).AssertOutput(999);
            new IntCode2(
                3, 21, 1008, 21, 8, 20, 1005, 20, 22, 107, 8, 21, 20, 1006, 20, 31,
                1106, 0, 36, 98, 0, 0, 1002, 21, 125, 20, 4, 20, 1105, 1, 46, 104,
                999, 1105, 1, 46, 1101, 1000, 1, 20, 4, 20, 1105, 1, 46, 98, 99)
                .Run(8).AssertOutput(1000);
            new IntCode2(
                3, 21, 1008, 21, 8, 20, 1005, 20, 22, 107, 8, 21, 20, 1006, 20, 31,
                1106, 0, 36, 98, 0, 0, 1002, 21, 125, 20, 4, 20, 1105, 1, 46, 104,
                999, 1105, 1, 46, 1101, 1000, 1, 20, 4, 20, 1105, 1, 46, 98, 99)
                .Run(9).AssertOutput(1001);

            var quine = new INT[] { 109, 1, 204, -1, 1001, 100, 1, 100, 1008, 100, 16, 101, 1006, 101, 0, 99 };
            new IntCode2(quine)
                .Run()
                .AssertOutput(quine);

            new IntCode2(1102, 34915192, 34915192, 7, 4, 7, 99, 0)
                .Run()
                .AssertHalted();

            new IntCode2(104, 1125899906842624, 99)
                .Run()
                .AssertOutput(1125899906842624);
        }

        public static INT[] Input => File.ReadAllText("input.txt").Split(",").Select(t => INT.Parse(t)).ToArray();
    }
}
