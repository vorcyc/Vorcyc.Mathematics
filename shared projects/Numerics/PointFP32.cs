using System.ComponentModel;
using System.Drawing;
using System.Globalization;

namespace Vorcyc.Mathematics.Numerics;

public struct PointFP32
{
    public readonly static PointFP32 Empty;

    private float x;

    private float y;

    [Browsable(false)]
    public bool IsEmpty
    {
        get
        {
            if (this.x != 0f)
            {
                return false;
            }
            return this.y == 0f;
        }
    }

    public float X
    {
        get
        {
            return this.x;
        }
        set
        {
            this.x = value;
        }
    }

    public float Y
    {
        get
        {
            return this.y;
        }
        set
        {
            this.y = value;
        }
    }

    static PointFP32()
    {
    }

    public PointFP32(float x, float y)
    {
        this.x = x;
        this.y = y;
    }

    public static PointFP32 Add(PointFP32 pt, Size sz)
    {
        return new PointFP32(pt.X + (float)sz.Width, pt.Y + (float)sz.Height);
    }

    public static PointFP32 Add(PointFP32 pt, SizeFP32 sz)
    {
        return new PointFP32(pt.X + sz.Width, pt.Y + sz.Height);
    }

    public override bool Equals(object obj)
    {
        if (!(obj is PointFP32))
        {
            return false;
        }
        PointFP32 pointF = (PointFP32)obj;
        if (pointF.X != this.X || pointF.Y != this.Y)
        {
            return false;
        }
        return pointF.GetType().Equals(this.GetType());
    }

    public override int GetHashCode()
    {
        return this.GetHashCode();
    }

    public static PointFP32 operator +(PointFP32 pt, Size sz)
    {
        return PointFP32.Add(pt, sz);
    }

    public static PointFP32 operator +(PointFP32 pt, SizeFP32 sz)
    {
        return PointFP32.Add(pt, sz);
    }

    public static bool operator ==(PointFP32 left, PointFP32 right)
    {
        if (left.X != right.X)
        {
            return false;
        }
        return left.Y == right.Y;
    }

    public static bool operator !=(PointFP32 left, PointFP32 right)
    {
        return !(left == right);
    }

    public static PointFP32 operator -(PointFP32 pt, Size sz)
    {
        return PointFP32.Subtract(pt, sz);
    }

    public static PointFP32 operator -(PointFP32 pt, SizeFP32 sz)
    {
        return PointFP32.Subtract(pt, sz);
    }

    public static PointFP32 Subtract(PointFP32 pt, Size sz)
    {
        return new PointFP32(pt.X - (float)sz.Width, pt.Y - (float)sz.Height);
    }

    public static PointFP32 Subtract(PointFP32 pt, SizeFP32 sz)
    {
        return new PointFP32(pt.X - sz.Width, pt.Y - sz.Height);
    }

    public override string ToString()
    {
        return string.Format(CultureInfo.CurrentCulture, "{{X={0}, Y={1}}}", new object[] { this.x, this.y });
    }
}
