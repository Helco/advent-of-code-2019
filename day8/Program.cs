using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace day8
{
    public class SpaceImage
    {
        int[] data = new int[0];
        int maxValue;

        public int Width { get; }
        public int Height { get; }
        public int PixelCount => Width * Height;
        public int LayerCount => data.Length / PixelCount;

        public SpaceImage(int width, int height, params int[] data)
        {
            Width = width;
            Height = height;
            if (data.Length % PixelCount != 0)
                throw new ArgumentException("Invalid pixel count");
            this.data = data.ToArray();
            maxValue = data.Max();
        }

        public int LayerOffset(int l)
        {
            return l * PixelCount;
        }

        public int PixelAt(int layer, int x, int y)
        {
            if (layer < 0 || layer >= LayerCount || x < 0 || x >= Width || y < 0 || y >= Height)
                throw new ArgumentOutOfRangeException();
            return data[LayerOffset(layer) + y * Width + x];
        }

        public Color ColorAt(int layer, int x, int y)
        {
            // grayscale color map
            //int p = 255 * PixelAt(layer, x, y) / maxValue;
            //return Color.FromArgb(255, p, p, p);

            // puzzle color map;
            switch(PixelAt(layer, x, y))
            {
                case 0: return Color.Black;
                case 1: return Color.White;
                case 2: return Color.FromArgb(0, 0, 0, 0);
                default: throw new InvalidProgramException("Invalid pixel value: " + PixelAt(layer, x, y));
            }
        }

        public SpaceImage MergeLayers()
        {
            var newData = Enumerable.Repeat(2, PixelCount).ToArray();
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    for (int l = 0; l < LayerCount; l++)
                    {
                        int p = PixelAt(l, x, y);
                        if (p != 2)
                        {
                            newData[y * Width + x] = p;
                            break;
                        }
                    }
                }
            }
            return new SpaceImage(Width, Height, newData);
        }

        public bool Equals(SpaceImage other)
        {
            if (Width != other.Width || Height != other.Height || LayerCount != other.LayerCount)
                return false;
            return data.Zip(other.data, (a, b) => a == b).All(b => b);
        }

        public void SaveLayer(int l, string name)
        {
            Bitmap bitmap = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    bitmap.SetPixel(x, y, ColorAt(l, x, y));
                }
            }
            bitmap.Save(name + ".png", ImageFormat.Png);
        }

        public void SaveAllLayers(string name)
        {
            for (int l = 0; l < LayerCount; l++)
                SaveLayer(l, name + "_" + l);
        }

        public IEnumerable<IEnumerable<int>> Layers => Enumerable
            .Range(0, LayerCount)
            .Select(l => data.Skip(LayerOffset(l)).Take(PixelCount));
    }

    class Program
    {
        static SpaceImage Input => new SpaceImage(25, 6,
            File.ReadAllText("input.txt").Trim().ToArray().Select(c => int.Parse("" + c)).ToArray());

        static int part1Checksum(SpaceImage img)
        {
            var layer = img.Layers
                .OrderBy(l => l.Count(p => p == 0))
                .First();
            return
                layer.Count(p => p == 1) *
                layer.Count(p => p == 2);
        }

        static void Assert<T>(T expected, T actual)
        {
            if (!expected.Equals(actual))
                throw new InvalidProgramException("Assertion failed");
        }

        static void tests()
        {
            Assert(6, part1Checksum(new SpaceImage(3, 2,
                0, 1, 2, 3, 4, 5,
                0, 0, 1, 1, 2, 2,
                1, 1, 1, 2, 2, 3)));

            Assert(true,
                new SpaceImage(2, 2, 0, 2, 2, 2, 1, 1, 2, 2, 2, 2, 1, 2, 0, 0, 0, 0)
                .MergeLayers().Equals(
                new SpaceImage(2, 2, 0, 1, 1, 0)));
        }

        static void Main(string[] args)
        {
            tests();
            Console.WriteLine("Part 1: " + part1Checksum(Input));

            Input.MergeLayers().SaveAllLayers("output");

            Console.WriteLine("done");
            Console.ReadKey(true);
        }
    }
}
