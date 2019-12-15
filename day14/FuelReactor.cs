using INT = System.Int64;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace day14
{
    class FuelReactor
    {
        public struct Probe
        {
            public INT amount;
            public string type;

            public Probe(INT amount, string type)
            {
                this.amount = amount;
                this.type = type;
            }

            public Probe(string r)
            {
                int s = r.IndexOf(' ');
                amount = int.Parse(r.Substring(0, s));
                type = r.Substring(s + 1).Trim();
            }
        }

        public class Reaction
        {
            public Probe[] inputs;
            public Probe output;

            public Reaction(Probe[] i, Probe o)
            {
                inputs = i;
                output = o;
            }

            public Reaction(string r)
            {
                var parts = r.Trim().Split(new string[] { ", ", " => " }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                inputs = parts.Reverse().Skip(1).Select(r => new Probe(r)).ToArray();
                output = new Probe(parts.Last());
            }
        }

        Reaction[] reactions;
        Dictionary<string, Reaction> reactionByOutput = new Dictionary<string, Reaction>();
        Dictionary<string, INT> oreRequirements = new Dictionary<string, INT>();
        Dictionary<string, INT> oreOrder = new Dictionary<string, INT>();

        public FuelReactor(string input)
        {
            reactions = input
                .Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Replace("\r", ""))
                .Where(l => l.Length > 0)
                .Select(l => new Reaction(l))
                .ToArray();

            foreach (var r in reactions)
            {
                if (reactionByOutput.ContainsKey(r.output.type))
                    throw new Exception("This does not work");
                reactionByOutput[r.output.type] = r;
            }

            oreRequirements.Add("ORE", 1);
            oreOrder.Add("ORE", 0);
            FindForward();
        }

        public class Step
        {
            public Reaction lastReaction;
            public Dictionary<string, INT> requirements;
        }

        public Step ApplyReaction(Step prev, Reaction nextReaction)
        {
            Step step = new Step();
            step.lastReaction = nextReaction;
            step.requirements = new Dictionary<string, INT>(prev.requirements);

            var req = step.requirements[nextReaction.output.type];
            step.requirements.Remove(nextReaction.output.type);
            INT multiplier = (req + nextReaction.output.amount - 1) / nextReaction.output.amount;
            foreach (var input in nextReaction.inputs)
            {
                if (!step.requirements.ContainsKey(input.type))
                    step.requirements[input.type] = 0;
                step.requirements[input.type] += multiplier * input.amount;
            }

            return step;
        }

        public INT FindBackwardFor(string type)
        {
            INT curMinimum = Int32.MaxValue;

            INT _doit(Step prev, Reaction nextReaction)
            {
                Step cur = ApplyReaction(prev, nextReaction);
                INT curOreReq = cur.requirements.GetValueOrDefault("ORE", 0);
                if (curOreReq >= curMinimum)
                    return INT.MaxValue;
                if (cur.requirements.Count == 1 && cur.requirements.First().Key == "ORE")
                    return (curMinimum = Math.Min(curOreReq, curMinimum));

                var candidates = cur.requirements
                    .Where(req => req.Key != "ORE")
                    .Where(req => curOreReq + oreRequirements[req.Key] * req.Value < curMinimum)
                    .ToArray();
                if (candidates.Length == 0)
                    return INT.MaxValue;
                return candidates.Min(req => _doit(cur, reactionByOutput[req.Key]));
            }

            Step empty = new Step();
            empty.lastReaction = null;
            empty.requirements = new Dictionary<string, INT>();
            empty.requirements.Add(type, 1);
            return _doit(empty, reactions.Single(r => r.output.type == type));
        }

        public INT FindBackwardItFor(string type)
        {
            INT curMinimum = Int32.MaxValue;
            Step empty = new Step();
            empty.lastReaction = null;
            empty.requirements = new Dictionary<string, INT>();
            empty.requirements.Add(type, 1);
            Stack<Step> stack = new Stack<Step>();
            stack.Push(empty);

            while(stack.Any())
            {
                Step cur = stack.Pop();
                INT curOreReq = cur.requirements.GetValueOrDefault("ORE", 0);
                if (curOreReq >= curMinimum)
                    continue;
                if (cur.requirements.Count == 1 && cur.requirements.First().Key == "ORE")
                {
                    curMinimum = Math.Min(curOreReq, curMinimum);
                    Console.WriteLine(curMinimum);
                    continue;
                }

                var candidates = cur.requirements
                    .Where(req => req.Key != "ORE")
                    .Where(req => curOreReq + oreRequirements[req.Key] * req.Value < curMinimum)
                    .OrderBy(req => oreOrder[req.Key])
                    .ToArray();
                foreach (var c in candidates)
                    stack.Push(ApplyReaction(cur, reactionByOutput[c.Key]));
            }
            return curMinimum;
        }

        public INT FindUsageBackwardIt()
        {
            INT curMaximum = Int32.MaxValue;
            Step empty = new Step();
            empty.lastReaction = null;
            empty.requirements = new Dictionary<string, INT>();
            empty.requirements.Add(type, 1);
            Stack<Step> stack = new Stack<Step>();
            stack.Push(empty);

            while(stack.Any())
            {
                Step cur = stack.Pop();
                INT curOreReq = cur.requirements.GetValueOrDefault("ORE", 0);
                if (curOreReq >= curMinimum)
                    continue;
                if (cur.requirements.Count == 1 && cur.requirements.First().Key == "ORE")
                {
                    curMinimum = Math.Min(curOreReq, curMinimum);
                    Console.WriteLine(curMinimum);
                    continue;
                }

                var candidates = cur.requirements
                    .Where(req => req.Key != "ORE")
                    .Where(req => curOreReq + oreRequirements[req.Key] * req.Value < curMinimum)
                    .OrderBy(req => oreOrder[req.Key])
                    .ToArray();
                foreach (var c in candidates)
                    stack.Push(ApplyReaction(cur, reactionByOutput[c.Key]));
            }
            return curMinimum;
        }

        public INT FindOreCountFor(string type)
        {
            return FindBackwardItFor(type);
        }

        public void FindForward()
        {
            INT order = 1;
            while(oreRequirements.Count - 1 != reactionByOutput.Count)
            {
                var nextPossible = reactions
                    .Where(r => !oreRequirements.ContainsKey(r.output.type))
                    .Where(r => r.inputs.All(i => oreRequirements.ContainsKey(i.type)));
                foreach (var p in nextPossible)
                {
                    INT amount = 0;
                    foreach (var i in p.inputs)
                        amount += oreRequirements[i.type] * i.amount;
                    oreRequirements[p.output.type] = amount / p.output.amount;
                    oreOrder[p.output.type] = order;
                }
                order++;
            }
        }

        static void Assert<T>(T expected, T actual)
        {
            Console.WriteLine("{0} == {1}", expected, actual);
            if (!expected.Equals(actual))
                throw new InvalidProgramException("assertion failed");
        }

        static void tests()
        {
            Assert(31, new FuelReactor(@"
                10 ORE => 10 A
                1 ORE => 1 B
                7 A, 1 B => 1 C
                7 A, 1 C => 1 D
                7 A, 1 D => 1 E
                7 A, 1 E => 1 FUEL").FindOreCountFor("FUEL"));

            Assert(165, new FuelReactor(@"
                9 ORE => 2 A
                8 ORE => 3 B
                7 ORE => 5 C
                3 A, 4 B => 1 AB
                5 B, 7 C => 1 BC
                4 C, 1 A => 1 CA
                2 AB, 3 BC, 4 CA => 1 FUEL").FindOreCountFor("FUEL"));

            Assert(13312, new FuelReactor(@"
                157 ORE => 5 NZVS
                165 ORE => 6 DCFZ
                44 XJWVT, 5 KHKGT, 1 QDVJ, 29 NZVS, 9 GPVTF, 48 HKGWZ => 1 FUEL
                12 HKGWZ, 1 GPVTF, 8 PSHF => 9 QDVJ
                179 ORE => 7 PSHF
                177 ORE => 5 HKGWZ
                7 DCFZ, 7 PSHF => 2 XJWVT
                165 ORE => 2 GPVTF
                3 DCFZ, 7 NZVS, 5 HKGWZ, 10 PSHF => 8 KHKGT").FindOreCountFor("FUEL"));

            Assert(180697, new FuelReactor(@"
                2 VPVL, 7 FWMGM, 2 CXFTF, 11 MNCFX => 1 STKFG
                17 NVRVD, 3 JNWZP => 8 VPVL
                53 STKFG, 6 MNCFX, 46 VJHF, 81 HVMC, 68 CXFTF, 25 GNMV => 1 FUEL
                22 VJHF, 37 MNCFX => 5 FWMGM
                139 ORE => 4 NVRVD
                144 ORE => 7 JNWZP
                5 MNCFX, 7 RFSQX, 2 FWMGM, 2 VPVL, 19 CXFTF => 3 HVMC
                5 VJHF, 7 MNCFX, 9 VPVL, 37 CXFTF => 6 GNMV
                145 ORE => 6 MNCFX
                1 NVRVD => 8 CXFTF
                1 VJHF, 6 MNCFX => 4 RFSQX
                176 ORE => 6 VJHF").FindOreCountFor("FUEL"));

            Assert(2210736, new FuelReactor(@"
                171 ORE => 8 CNZTR
                7 ZLQW, 3 BMBT, 9 XCVML, 26 XMNCP, 1 WPTQ, 2 MZWV, 1 RJRHP => 4 PLWSL
                114 ORE => 4 BHXH
                14 VRPVC => 6 BMBT
                6 BHXH, 18 KTJDG, 12 WPTQ, 7 PLWSL, 31 FHTLT, 37 ZDVW => 1 FUEL
                6 WPTQ, 2 BMBT, 8 ZLQW, 18 KTJDG, 1 XMNCP, 6 MZWV, 1 RJRHP => 6 FHTLT
                15 XDBXC, 2 LTCX, 1 VRPVC => 6 ZLQW
                13 WPTQ, 10 LTCX, 3 RJRHP, 14 XMNCP, 2 MZWV, 1 ZLQW => 1 ZDVW
                5 BMBT => 4 WPTQ
                189 ORE => 9 KTJDG
                1 MZWV, 17 XDBXC, 3 XCVML => 2 XMNCP
                12 VRPVC, 27 CNZTR => 2 XDBXC
                15 KTJDG, 12 BHXH => 5 XCVML
                3 BHXH, 2 VRPVC => 7 MZWV
                121 ORE => 7 VRPVC
                7 XCVML => 6 RJRHP
                5 BHXH, 4 VRPVC => 5 LTCX").FindOreCountFor("FUEL"));
        }

        static void Main(string[] args)
        {
            //tests();
            Console.WriteLine("Part 1: " + new FuelReactor(File.ReadAllText("input.txt")).FindOreCountFor("FUEL"));

            Console.WriteLine("done");
            Console.ReadKey(true);
        }
    }
}
