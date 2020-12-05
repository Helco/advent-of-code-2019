using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace day3
{
    public struct IVec2
    {
        public static readonly IVec2 Zero = new IVec2(0, 0);
        public static readonly IVec2 One = new IVec2(1, 1);
        public static readonly IVec2 Right = new IVec2(1, 0);
        public static readonly IVec2 Left = new IVec2(-1, 0);
        public static readonly IVec2 Up = new IVec2(0, -1);
        public static readonly IVec2 Down = new IVec2(0, 1);

        public readonly int x, y;

        public IVec2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public override bool Equals(object obj) => (obj is IVec2) ? ((IVec2)obj) == this : false;

        public override int GetHashCode() => (x * 45265497) ^ y;

        public override string ToString() => $"[{x},{y}]";

        public IVec2 WithX(int newX) => new IVec2(newX, y);
        public IVec2 WithY(int newY) => new IVec2(x, newY);

        public static IVec2 operator +(IVec2 a, IVec2 b) => new IVec2(a.x + b.x, a.y + b.y);
        public static IVec2 operator -(IVec2 a, IVec2 b) => new IVec2(a.x - b.x, a.y - b.y);
        public static IVec2 operator *(IVec2 a, int s) => new IVec2(a.x * s, a.y * s);
        public static IVec2 operator /(IVec2 a, int s) => new IVec2(a.x / s, a.y / s);
        public static bool operator ==(IVec2 a, IVec2 b) => a.x == b.x && a.y == b.y;
        public static bool operator !=(IVec2 a, IVec2 b) => a.x != b.x || a.y != b.y;

        public IVec2 Normalized => this / GreatestCommonDivisor(x, y);
        public int dot(IVec2 a) => x * a.x + y * a.y;
        public int LengthSqr => dot(this);
        public int Length => (int)Math.Sqrt(LengthSqr);
        public int LengthManhattan => Math.Abs(x) + Math.Abs(y);

        // starting up and going clockwise
        public double Angle => Math.Atan2(x, -y);
        public double AngleTo(IVec2 a) => (a - this).Angle;

        // ripped from wikipedia
        private int GreatestCommonDivisor(int a, int b)
        {
            int h;
            if (a == 0) return Math.Abs(b);
            if (b == 0) return Math.Abs(a);

            do
            {
                h = a % b;
                a = b;
                b = h;
            } while (b != 0);

            return Math.Abs(a);
        }
    }
}
