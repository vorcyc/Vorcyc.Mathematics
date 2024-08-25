using System.ComponentModel;
using System.Drawing;
using System.Globalization;

namespace Vorcyc.Mathematics.Numerics;

public struct RectangleFP32
{
    public readonly static RectangleFP32 Empty;

    private float x;

    private float y;

    private float width;

    private float height;

    [Browsable(false)]
    public float Bottom
    {
        get
        {
            return this.Y + this.Height;
        }
    }

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
            if (this.Width <= 0f)
            {
                return true;
            }
            return this.Height <= 0f;
        }
    }

    [Browsable(false)]
    public float Left
    {
        get
        {
            return this.X;
        }
    }

    [Browsable(false)]
    public PointF Location
    {
        get
        {
            return new PointF(this.X, this.Y);
        }
        set
        {
            this.X = value.X;
            this.Y = value.Y;
        }
    }

    [Browsable(false)]
    public float Right
    {
        get
        {
            return this.X + this.Width;
        }
    }

    [Browsable(false)]
    public SizeF Size
    {
        get
        {
            return new SizeF(this.Width, this.Height);
        }
        set
        {
            this.Width = value.Width;
            this.Height = value.Height;
        }
    }

    [Browsable(false)]
    public float Top
    {
        get
        {
            return this.Y;
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

    static RectangleFP32()
    {
    }

    public RectangleFP32(float x, float y, float width, float height)
    {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
    }

    public RectangleFP32(PointF location, SizeF size)
    {
        this.x = location.X;
        this.y = location.Y;
        this.width = size.Width;
        this.height = size.Height;
    }

    public bool Contains(float x, float y)
    {
        if (this.X > x || x >= this.X + this.Width || this.Y > y)
        {
            return false;
        }
        return y < this.Y + this.Height;
    }

    public bool Contains(PointF pt)
    {
        return this.Contains(pt.X, pt.Y);
    }

    public bool Contains(RectangleFP32 rect)
    {
        if (this.X > rect.X || rect.X + rect.Width > this.X + this.Width || this.Y > rect.Y)
        {
            return false;
        }
        return rect.Y + rect.Height <= this.Y + this.Height;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is RectangleFP32))
        {
            return false;
        }
        RectangleFP32 rectangleF = (RectangleFP32)obj;
        if (rectangleF.X != this.X || rectangleF.Y != this.Y || rectangleF.Width != this.Width)
        {
            return false;
        }
        return rectangleF.Height == this.Height;
    }

    public static RectangleFP32 FromLTRB(float left, float top, float right, float bottom)
    {
        return new RectangleFP32(left, top, right - left, bottom - top);
    }

    public override int GetHashCode()
    {
        return (int)((uint)this.X ^ ((uint)this.Y << 13 | (uint)this.Y >> 19) ^ ((uint)this.Width << 26 | (uint)this.Width >> 6) ^ ((uint)this.Height << 7 | (uint)this.Height >> 25));
    }

    public void Inflate(float x, float y)
    {
        this.X = this.X - x;
        this.Y = this.Y - y;
        this.Width = this.Width + 2f * x;
        this.Height = this.Height + 2f * y;
    }

    public void Inflate(SizeF size)
    {
        this.Inflate(size.Width, size.Height);
    }

    public static RectangleFP32 Inflate(RectangleFP32 rect, float x, float y)
    {
        RectangleFP32 rectangleF = rect;
        rectangleF.Inflate(x, y);
        return rectangleF;
    }

    public void Intersect(RectangleFP32 rect)
    {
        RectangleFP32 rectangleF = RectangleFP32.Intersect(rect, this);
        this.X = rectangleF.X;
        this.Y = rectangleF.Y;
        this.Width = rectangleF.Width;
        this.Height = rectangleF.Height;
    }

    public static RectangleFP32 Intersect(RectangleFP32 a, RectangleFP32 b)
    {
        float single = Math.Max(a.X, b.X);
        float single1 = Math.Min(a.X + a.Width, b.X + b.Width);
        float single2 = Math.Max(a.Y, b.Y);
        float single3 = Math.Min(a.Y + a.Height, b.Y + b.Height);
        if (single1 < single || single3 < single2)
        {
            return RectangleFP32.Empty;
        }
        return new RectangleFP32(single, single2, single1 - single, single3 - single2);
    }

    public bool IntersectsWith(RectangleFP32 rect)
    {
        if (rect.X >= this.X + this.Width || this.X >= rect.X + rect.Width || rect.Y >= this.Y + this.Height)
        {
            return false;
        }
        return this.Y < rect.Y + rect.Height;
    }

    public void Offset(PointF pos)
    {
        this.Offset(pos.X, pos.Y);
    }

    public void Offset(float x, float y)
    {
        this.X = this.X + x;
        this.Y = this.Y + y;
    }

    public static bool operator ==(RectangleFP32 left, RectangleFP32 right)
    {
        if (left.X != right.X || left.Y != right.Y || left.Width != right.Width)
        {
            return false;
        }
        return left.Height == right.Height;
    }

    public static implicit operator RectangleFP32(Rectangle r)
    {
        return new RectangleFP32((float)r.X, (float)r.Y, (float)r.Width, (float)r.Height);
    }

    public static bool operator !=(RectangleFP32 left, RectangleFP32 right)
    {
        return !(left == right);
    }



    public override string ToString()
    {
        string[] str = new string[] { "{X=", null, null, null, null, null, null, null, null };
        float x = this.X;
        str[1] = x.ToString(CultureInfo.CurrentCulture);
        str[2] = ",Y=";
        x = this.Y;
        str[3] = x.ToString(CultureInfo.CurrentCulture);
        str[4] = ",Width=";
        x = this.Width;
        str[5] = x.ToString(CultureInfo.CurrentCulture);
        str[6] = ",Height=";
        x = this.Height;
        str[7] = x.ToString(CultureInfo.CurrentCulture);
        str[8] = "}";
        return string.Concat(str);
    }

    public static RectangleFP32 Union(RectangleFP32 a, RectangleFP32 b)
    {
        float single = Math.Min(a.X, b.X);
        float single1 = Math.Max(a.X + a.Width, b.X + b.Width);
        float single2 = Math.Min(a.Y, b.Y);
        float single3 = Math.Max(a.Y + a.Height, b.Y + b.Height);
        return new RectangleFP32(single, single2, single1 - single, single3 - single2);
    }
}