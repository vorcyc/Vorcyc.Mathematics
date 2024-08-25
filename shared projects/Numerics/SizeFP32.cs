using System.ComponentModel;
using System.Drawing;
using System.Globalization;

namespace Vorcyc.Mathematics.Numerics;

public struct SizeFP32
{
    public readonly static SizeFP32 Empty;

    private float width;

    private float height;

    public float Height
    {
        get
        {
            return this.height;
        }
        set
        {
            this.height = value;
        }
    }

    [Browsable(false)]
    public bool IsEmpty
    {
        get
        {
            if (this.width != 0f)
            {
                return false;
            }
            return this.height == 0f;
        }
    }

    public float Width
    {
        get
        {
            return this.width;
        }
        set
        {
            this.width = value;
        }
    }

    static SizeFP32()
    {
    }

    public SizeFP32(SizeFP32 size)
    {
        this.width = size.width;
        this.height = size.height;
    }

    public SizeFP32(PointF pt)
    {
        this.width = pt.X;
        this.height = pt.Y;
    }

    public SizeFP32(float width, float height)
    {
        this.width = width;
        this.height = height;
    }

    public static SizeFP32 Add(SizeFP32 sz1, SizeFP32 sz2)
    {
        return new SizeFP32(sz1.Width + sz2.Width, sz1.Height + sz2.Height);
    }

    public override bool Equals(object obj)
    {
        if (!(obj is SizeFP32))
        {
            return false;
        }
        SizeFP32 sizeF = (SizeFP32)obj;
        if (sizeF.Width != this.Width || sizeF.Height != this.Height)
        {
            return false;
        }
        return sizeF.GetType().Equals(this.GetType());
    }

    public override int GetHashCode()
    {
        return this.GetHashCode();
    }

    public static SizeFP32 operator +(SizeFP32 sz1, SizeFP32 sz2)
    {
        return SizeFP32.Add(sz1, sz2);
    }

    public static bool operator ==(SizeFP32 sz1, SizeFP32 sz2)
    {
        if (sz1.Width != sz2.Width)
        {
            return false;
        }
        return sz1.Height == sz2.Height;
    }

    public static explicit operator PointF(SizeFP32 size)
    {
        return new PointF(size.Width, size.Height);
    }

    public static bool operator !=(SizeFP32 sz1, SizeFP32 sz2)
    {
        return !(sz1 == sz2);
    }

    public static SizeFP32 operator -(SizeFP32 sz1, SizeFP32 sz2)
    {
        return SizeFP32.Subtract(sz1, sz2);
    }

    public static SizeFP32 Subtract(SizeFP32 sz1, SizeFP32 sz2)
    {
        return new SizeFP32(sz1.Width - sz2.Width, sz1.Height - sz2.Height);
    }

    public PointF ToPointF()
    {
        return (PointF)this;
    }

    //public Size ToSize()
    //{
    //    return Size.Truncate(this);
    //}

    public override string ToString()
    {
        return string.Concat(new string[] { "{Width=", this.width.ToString(CultureInfo.CurrentCulture), ", Height=", this.height.ToString(CultureInfo.CurrentCulture), "}" });
    }
}