using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace day1
{
    class Program
    {

        static int FuelFor(int mass)
        {
            int totalFuel = 0;
            int addFuel = (mass / 3) - 2;
            do
            {
                totalFuel += addFuel;
                addFuel = (addFuel / 3) - 2;
            } while (addFuel > 0);
            return totalFuel;
        } 

        static int ShipFuel() => File
                .ReadAllLines("input.txt")
                .Select(l => Convert.ToInt32(l))
                .Select(FuelFor)
                .Sum();
        static void part2(string[] args)
        {
            Console.WriteLine("Test 3: " + FuelFor(100756) + " == 50346");

            Console.WriteLine(ShipFuel());
            Console.ReadKey(true);
        }

        static void Main(string[] args)
        {
            part2(args);
        }
    }
}
