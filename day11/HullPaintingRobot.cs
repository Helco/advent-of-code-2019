using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;

namespace day11
{
    class HullPaintingRobot
    {
        static readonly Vector2Int[] directions = new Vector2Int[]
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left
        };

        HashSet<Vector2Int> whitePanels = new HashSet<Vector2Int>();
        HashSet<Vector2Int> allPanels = new HashSet<Vector2Int>();
        long[] initialProgram;
        IntCode2 code;
        Vector2Int currentPos = Vector2Int.zero;
        int currentDirection = 0;

        public HullPaintingRobot(params long[] program)
        {
            this.initialProgram = program;
            Reset();
        }

        public void Reset()
        {
            code = new IntCode2(initialProgram);
        }

        public bool Step()
        {
            code.Run(whitePanels.Contains(currentPos) ? 1 : 0);
            if (code.Output.Count() == 2)
            {
                if (code.Output.ElementAt(0) == 1)
                    whitePanels.Add(currentPos);
                else
                    whitePanels.Remove(currentPos);
                allPanels.Add(currentPos);

                currentDirection += 4; //muhaha
                currentDirection += -1 + 2 * (int)code.Output.ElementAt(1);
                currentDirection = currentDirection % 4;
                currentPos += directions[currentDirection];
            }
            else
                Assert(false, code.HasHalted);
            return code.HasHalted;
        }

        static void Assert<T>(T expected, T actual)
        {
            if (!expected.Equals(actual))
                throw new InvalidProgramException("Assertion failed");
        }

        static void part1()
        {
            var robot = new HullPaintingRobot(IntCode2.Input);
            while (!robot.Step()) ;
            Console.WriteLine("Part 1:" + robot.allPanels.Count);
        }

        static void part2()
        {
            var robot = new HullPaintingRobot(IntCode2.Input);
            robot.whitePanels.Add(robot.currentPos);
            while (!robot.Step()) ;

            int minX = robot.whitePanels.Min(v => v.x);
            int maxX = robot.whitePanels.Max(v => v.x);
            int minY = robot.whitePanels.Min(v => v.y);
            int maxY = robot.whitePanels.Max(v => v.y);
            int width = maxX - minX + 1;
            int height = maxY - minY + 1;
            Bitmap bitmap = new Bitmap(width, height);
            foreach (var p in robot.whitePanels)
                bitmap.SetPixel(p.x - minX, p.y - minY, System.Drawing.Color.White);
            bitmap.Save("part2.png", ImageFormat.Png);
        }

        static void Main(string[] args)
        {
            IntCode2.tests(args);
            part1();
            part2();

            Console.WriteLine("done");
            Console.ReadKey(true);
        }
    }
}
