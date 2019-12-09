using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace day9
{
    class Program
    {
        static void Main(string[] args)
        {
            IntCode2.tests(args);

            Console.WriteLine("Part 1: ");
            new IntCode2(IntCode2.Input).Run(1).PrintOutput();
            new IntCode2(IntCode2.Input).Run(2).PrintOutput();

            Console.WriteLine("done");
            Console.ReadKey(true);
        }
    }
}
