using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using System.Drawing;
using System.Drawing.Imaging;

namespace day15
{
    enum Tile
    {
        Unexplored = 0,
        Empty,
        Wall,
        OxygenSystem
    }

    enum Direction
    {
        North = 1,
        South = 2,
        West = 3,
        East = 4,
        
        SPECIAL
    }

    class RepairDroid
    {
        static readonly IReadOnlyDictionary<Direction, Vector2Int> DirectionVectors = new Dictionary<Direction, Vector2Int>()
        {
            { Direction.North, new Vector2Int(0, -1) },
            { Direction.South, new Vector2Int(0, 1) },
            { Direction.West, new Vector2Int(-1, 0) },
            { Direction.East, new Vector2Int(1, 0) }
        };

        static readonly IReadOnlyDictionary<Direction, Direction> OppositeDirections = new Dictionary<Direction, Direction>()
        {
            { Direction.North, Direction.South },
            { Direction.South, Direction.North },
            { Direction.West, Direction.East },
            { Direction.East, Direction.West }
        };

        IntCode2 code;
        Dictionary<Vector2Int, Tile> tiles = new Dictionary<Vector2Int, Tile>();
        Dictionary<Vector2Int, int> distance = new Dictionary<Vector2Int, int>();
        Dictionary<Vector2Int, Vector2Int> previous = new Dictionary<Vector2Int, Vector2Int>();
        Vector2Int current;

        public RepairDroid(params long[] input)
        {
            code = new IntCode2(input);
        }

        public int ExploreDirection(Direction dir)
        {
            code.Run((long)dir);
            if (code.HasHalted || !code.NeedsInput || code.Output.Count() != 1)
                throw new InvalidProgramException("This should not happen");
            if (code.Output.First() != 0)
                current += DirectionVectors[dir];
            return (int)code.Output.First();
        }

        public void GoKnownDirection(Direction dir)
        {
            int result = ExploreDirection(dir);
            if (result == 0)
                throw new InvalidProgramException("Expected not a wall");
            if (result == 1 && tiles[current] != Tile.Empty)
                throw new InvalidProgramException("Expected an empty tile");
            if (result == 2 && tiles[current] != Tile.OxygenSystem)
                throw new InvalidProgramException("Expected an oxygen system");
        }

        public bool IsKnown(Direction dir)
        {
            return tiles.ContainsKey(current + DirectionVectors[dir]);
        }

        public void ExploreMap()
        {
            current = new Vector2Int(0, 0);
            tiles.Clear();
            tiles.Add(Vector2Int.zero, Tile.Empty);

            Stack<Direction> stepsWent = new Stack<Direction>();
            stepsWent.Push(Direction.SPECIAL);
            while(stepsWent.Count > 0)
            {
                int i;
                for (i = (int)Direction.North; i <= (int)Direction.East; i++)
                {
                    if (IsKnown((Direction)i))
                        continue;
                    int result = ExploreDirection((Direction)i);
                    if (result == 0)
                        tiles[current + DirectionVectors[(Direction)i]] = Tile.Wall;
                    else
                    {
                        stepsWent.Push((Direction)i);
                        tiles[current] = result == 1 ? Tile.Empty : Tile.OxygenSystem;
                    }
                    break;
                }
                if (i > (int)Direction.East)
                {
                    var lastDir = stepsWent.Pop();
                    if (lastDir != Direction.SPECIAL)
                        GoKnownDirection(OppositeDirections[lastDir]);
                }
            }

            if (current != Vector2Int.zero)
                throw new InvalidProgramException("ExploreMap should not move after exploring");
        }

        public void DijkstraTo(Vector2Int target)
        {
            distance[target] = 0;
            PrioQueue<Vector2Int> q = new PrioQueue<Vector2Int>();
            var validTiles = tiles.Where(t => t.Value == Tile.OxygenSystem || t.Value == Tile.Empty).ToDictionary(p => p.Key, p => p.Value);

            int DistanceTo(Vector2Int tile)
            {
                return distance.TryGetValue(tile, out int actual)
                    ? actual
                    : int.MaxValue - 10;
            }

            IEnumerable<Vector2Int> NeighborsTo(Vector2Int tile)
            {
                for (int i = (int)Direction.North; i <= (int)Direction.East; i++)
                {
                    Vector2Int test = tile + DirectionVectors[(Direction)i];
                    if (validTiles.ContainsKey(test))
                        yield return test;
                }
            }
            
            foreach (var pos in validTiles.Keys)
                q.Enqueue(pos, DistanceTo(pos));

            while(!q.IsEmpty())
            {
                var u = q.Dequeue();
                var uDist = DistanceTo(u);
                foreach (var neighbor in NeighborsTo(u))
                {
                    int testDistance = uDist + 1;
                    if (testDistance < DistanceTo(neighbor))
                    {
                        q.ChangePrio(neighbor, DistanceTo(neighbor), testDistance);
                        distance[neighbor] = testDistance;
                        previous[neighbor] = u;
                    }
                }
            }
        }

        void SaveMapAsImage()
        {
            int minX = tiles.Keys.Min(v => v.x);
            int maxX = tiles.Keys.Max(v => v.x);
            int minY = tiles.Keys.Min(v => v.y);
            int maxY = tiles.Keys.Max(v => v.y);
            int width = maxX - minX + 1;
            int height = maxY - minY + 1;

            Bitmap bitmap = new Bitmap(width, height);
            foreach (var p in tiles)
            {
                System.Drawing.Color c;
                switch(p.Value)
                {
                    default: c = System.Drawing.Color.FromArgb(255, 255, 0, 255); break;
                    case Tile.Empty: c = System.Drawing.Color.Black; break;
                    case Tile.Wall: c = System.Drawing.Color.White; break;
                    case Tile.OxygenSystem: c = System.Drawing.Color.Blue; break;
                }
                bitmap.SetPixel(p.Key.x - minX, p.Key.y - minY, c);
            }
            bitmap.Save("map.png", ImageFormat.Png);
        }

        static void Main(string[] args)
        {
            IntCode2.tests(args);
            var droid = new RepairDroid(IntCode2.Input);
            droid.ExploreMap();
            droid.SaveMapAsImage();

            droid.DijkstraTo(droid.tiles.First(p => p.Value == Tile.OxygenSystem).Key);
            Console.WriteLine("Part 1: " + droid.distance[Vector2Int.zero]);
            Console.WriteLine("Part 2: " + droid.distance.Max(p => p.Value));

            Console.WriteLine("done");
            Console.ReadKey(true);
        }
    }
}
