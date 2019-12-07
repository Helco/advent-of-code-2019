using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;

namespace day7
{
    public class IntCode2
    {
        Dictionary<int, int> map = new Dictionary<int, int>();
        Queue<int> input = new Queue<int>();
        List<int> output = new List<int>();
        int pc = 0;
        bool halted = false;

        Func<int, int>[] getters;
        Action<int, int>[] setters;

        public IntCode2(params int[] initial)
        {
            for (int i = 0; i < initial.Length; i++)
                this[i] = initial[i];

            getters = new Func<int, int>[]
            {
                (pos) => this[pos],
                (val) => val
            };

            setters = new Action<int, int>[]
            {
                (pos, val) => this[pos] = val,
                (pos, val) => throw new InvalidOperationException("Cannot store into immediate mode")
            };
        }

        public IEnumerable<int> Output => output;
        public bool HasHalted => halted;

        public int[] ToArray()
        {
            int max = map.Max(p => p.Key);
            int[] result = new int[max + 1];
            foreach (var p in map)
                result[p.Key] = p.Value;
            return result;
        }

        public IntCode2 AssertAgainst(params int[] against)
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

        public IntCode2 AssertHalted()
        {
            if (!halted)
                throw new InvalidProgramException("Assertion failed");
            return this;
        }

        public IntCode2 AssertOutput(params int[] expectedOutput)
        {
            AssertHalted();
            if (output.Count != expectedOutput.Count())
                throw new InvalidProgramException("Assertion failed");
            for (int i = 0; i < output.Count; i++)
            {
                if (output[i] != expectedOutput[i])
                    throw new InvalidProgramException("Assertion failed");
            }
            return this;
        }

        public int this[int index]
        {
            get => map.TryGetValue(index, out int result) ? result : 0;
            set => map[index] = value;
        }

        public IntCode2 Step()
        {
            int command = this[pc];
            int opcode = command % 100;
            var param0Mode = (command / 100) % 10;
            var param1Mode = (command / 1000) % 10;
            var param2Mode = (command / 10000) % 10;
            var param0 = (get: getters[param0Mode], set: setters[param0Mode]);
            var param1 = (get: getters[param1Mode], set: setters[param1Mode]);
            var param2 = (get: getters[param2Mode], set: setters[param2Mode]);

            int condition;

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
                    if (!input.TryDequeue(out int inputValue))
                        throw new NoInputException("Input is empty");
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
                case 99:
                    halted = true;
                    break;
                default:
                    throw new InvalidOperationException("Invalid opcode: " + this[pc]);
            }
            return this;
        }

        public IntCode2 Run(params int[] inputArr)
        {
            output.Clear();
            input = new Queue<int>(inputArr);
            while (!halted) Step();
            return this;
        }

        public static void tests()
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
        }

        [Serializable]
        public class NoInputException : Exception
        {
            public NoInputException(string message) : base(message)
            {
            }
        }
    }
}
