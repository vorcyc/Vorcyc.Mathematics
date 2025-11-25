namespace Vorcyc.Mathematics;

/* duan linli aka cyclone_dll
 * 
 * ‎2022‎年‎1‎月‎10‎日 
 * 于昆明初次完成修订
 * 最早叫 PinnedNumbers 
 * 现在改进了。还加入了租赁模式
 */

//#define use_unsafe_code

using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Vorcyc.Mathematics.Framework;
using Vorcyc.Mathematics.Statistics;

//https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags#list

/// <summary>
/// Represents a memory-fixed and size-fixed, one-dimensional array.
/// <para>
/// Use this class like an <see cref="Array"/>, but it's pinnable and efficient, intended for <see cref="INumberBase{TSelf}"/>.
/// </para>
/// </summary>
/// <typeparam name="T">
/// <list type="bullet">
/// <item>
/// <term>unmanaged</term>
/// <description>Value type suitable for fixed memory operations.</description>
/// </item>
/// <item>
/// <term><see cref="INumberBase{TSelf}"/></term>
/// <description>Provides generic numeric construction and classification APIs.</description>
/// </item>
/// </list>
/// </typeparam>
public sealed class PinnableArray<T> : IDisposable, IEnumerable<T>
    where T : unmanaged, INumberBase<T>
{

    private readonly T[] _array;

    private readonly int _length;

    private bool _isPinned;

    private Memory<T> _memory;

    private MemoryHandle _memoryHandle;

    private ArrayPool<T> _pool = ArrayPool<T>.Create();


    #region constructor

    /// <summary>
    /// Initializes a new instance from an <see cref="ArraySegment{T}"/> by copying its contents.
    /// </summary>
    /// <param name="segment">The source segment to copy from.</param>
    /// <param name="pin">If true, pins the underlying buffer immediately.</param>
    public PinnableArray(ArraySegment<T> segment, bool pin = false)
    {
        _length = segment.Count;

        if (Options.UseLeasingMode)
            _array = _pool.Rent(_length);
        else
            _array = new T[_length];

        Buffer.BlockCopy(segment.Array, segment.Offset, _array, 0, _length * Unsafe.SizeOf<T>());

        if (pin) Pin();
    }

    /// <summary>
    /// Initializes a new instance from a <see cref="Span{T}"/> by copying its contents.
    /// </summary>
    /// <param name="span">The source span to copy from.</param>
    /// <param name="pin">If true, pins the underlying buffer immediately.</param>
    public PinnableArray(Span<T> span, bool pin = false)
    {
        _length = span.Length;

        if (Options.UseLeasingMode)
            _array = _pool.Rent(_length);
        else
            _array = new T[_length];

        Buffer.BlockCopy(span.ToArray(), 0, _array, 0, span.Length * Unsafe.SizeOf<T>());

        if (pin) Pin();
    }

    /// <summary>
    /// Initializes a new instance from an array by copying its contents.
    /// </summary>
    /// <param name="array">The source array to copy from.</param>
    /// <param name="pin">If true, pins the underlying buffer immediately.</param>
    public PinnableArray(T[] array, bool pin = false)
    {
        _length = array.Length;

        if (Options.UseLeasingMode)
            _array = _pool.Rent(_length);
        else
            _array = new T[_length];

        Buffer.BlockCopy(array, 0, _array, 0, _length * Unsafe.SizeOf<T>());

        if (pin) Pin();
    }

    /// <summary>
    /// Initializes a new instance from a sub-range of an array by copying its contents.
    /// </summary>
    /// <param name="array">The source array.</param>
    /// <param name="offset">The starting index within the source array.</param>
    /// <param name="count">The number of elements to copy.</param>
    /// <param name="pin">If true, pins the underlying buffer immediately.</param>
    /// <exception cref="IndexOutOfRangeException">Thrown if <paramref name="offset"/> or <paramref name="count"/> are invalid.</exception>
    public PinnableArray(T[] array, int offset, int count, bool pin = false)
    {
        if (offset < 0 || offset > array.Length)
            throw new IndexOutOfRangeException(nameof(offset));

        if (count < 0 || count > array.Length)
            throw new IndexOutOfRangeException(nameof(count));

        _length = count;

        if (Options.UseLeasingMode)
            _array = _pool.Rent(_length);
        else
            _array = new T[_length];

        Buffer.BlockCopy(array, offset, _array, 0, count * Unsafe.SizeOf<T>());

        if (pin) Pin();
    }

    /// <summary>
    /// Initializes a new instance with the specified length. Elements are default-initialized.
    /// </summary>
    /// <param name="count">Logical element count.</param>
    /// <param name="pin">If true, pins the underlying buffer immediately.</param>
    public PinnableArray(int count, bool pin = false)
    {
        _length = count;

        if (Options.UseLeasingMode)
            _array = _pool.Rent(_length);
        else
            _array = new T[_length];

        if (pin) Pin();
    }

    #endregion


    #region pin and unpin

    /// <summary>
    /// Pins this <see cref="PinnableArray{T}"/> so you can get its stable memory address for interop.
    /// </summary>
    public void Pin()
    {
        if (_isPinned) return;

        _memory = new Memory<T>(_array);
        _memoryHandle = _memory.Pin();
        _isPinned = true;
    }

    /// <summary>
    /// Unpins this <see cref="PinnableArray{T}"/> so it can be moved or collected by the GC.
    /// </summary>
    public void Unpin()
    {
        if (_isPinned)
        {
            _memoryHandle.Dispose();
            _isPinned = false;
        }
    }

    #endregion


    #region Properites

    /// <summary>
    /// Gets whether this <see cref="PinnableArray{T}"/> is currently pinned.
    /// </summary>
    public bool IsPinned => _isPinned;

    /// <summary>
    /// Gets the logical element count.
    /// </summary>
    public int Length => _length;

    /// <summary>
    /// Gets the underlying array. In leasing mode, the returned array may be larger than <see cref="Length"/>.
    /// </summary>
    public T[] Values => _array;

    #endregion


    #region AsSpan

    /// <summary>
    /// Returns a <see cref="Span{T}"/> covering all logical elements.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan() => new(_array, 0, _length);

    /// <summary>
    /// Returns a <see cref="Span{T}"/> over a sub-range.
    /// </summary>
    /// <param name="start">Start index.</param>
    /// <param name="length">Number of elements.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan(int start, int length) => new(_array, start, length);

    /// <summary>
    /// Returns a <see cref="Span{T}"/> starting at <paramref name="start"/> to the end.
    /// </summary>
    /// <param name="start">Start index.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan(int start) => _array.AsSpan(start);

    /// <summary>
    /// Returns a <see cref="Span{T}"/> starting at <paramref name="startIndex"/> to the end.
    /// </summary>
    /// <param name="startIndex">Start index.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan(System.Index startIndex) => _array.AsSpan(startIndex);

    /// <summary>
    /// Returns a <see cref="Span{T}"/> represented by <paramref name="range"/>.
    /// </summary>
    /// <param name="range">The range to slice.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan(Range range) => _array.AsSpan(range);

    #endregion


    #region Pointers

    /// <summary>
    /// Gets a pointer to the first element if pinned; otherwise <c>null</c>.
    /// </summary>
    public unsafe T* Pointer => _isPinned ? (T*)_memoryHandle.Pointer : null;

    /// <summary>
    /// Reinterprets the pinned memory as a pointer of the specified unmanaged type.
    /// </summary>
    /// <typeparam name="TElement">Target element type for reinterpretation.</typeparam>
    /// <returns>A pointer to the buffer if pinned; otherwise <c>null</c>.</returns>
    public unsafe TElement* ToPointerOf<TElement>() where TElement : unmanaged
        => _isPinned ? (TElement*)_memoryHandle.Pointer : null;

    /// <summary>
    /// Returns a tuple containing the raw address and the element count when reinterpreted as <typeparamref name="TElement"/>.
    /// </summary>
    /// <typeparam name="TElement">Target element type for reinterpretation.</typeparam>
    /// <returns>(address, count). If not pinned, returns (Zero, 0).</returns>
    public (nint address, int count) ToSecurePointerOf<TElement>()
        where TElement : unmanaged
    {
        if (!_isPinned) return (nint.Zero, 0);

        unsafe
        {
            return ((nint)_memoryHandle.Pointer, Marshal.SizeOf<T>() * _length / Marshal.SizeOf<TElement>());
        }
    }

    #endregion


    #region Indexer

    /// <summary>
    /// Gets a by-ref access to the element at the specified index.
    /// </summary>
    /// <param name="index">The element index.</param>
    public ref T this[int index] => ref _array[index];

    /// <summary>
    /// Returns a <see cref="Span{T}"/> representing the specified segment.
    /// </summary>
    /// <param name="startPos">Start index of the slice.</param>
    /// <param name="length">Number of elements in the slice.</param>
    public Span<T> this[int startPos, int length]
        => _array.AsSpan(startPos, length);

    /// <summary>
    /// Returns a <see cref="Span{T}"/> representing the specified range.
    /// </summary>
    /// <param name="range">The range to slice.</param>
    public Span<T> this[Range range]
        => _array.AsSpan(range);

    #endregion


    #region Element-wise operations

    /// <summary>
    /// Fills all elements with the specified value.
    /// </summary>
    /// <param name="value">The value to assign.</param>
    public void Fill(T value)
    {
        for (int i = 0; i < _length; i++)
        {
            _array[i] = value;
        }
    }

    /// <summary>
    /// Fills the array with an arithmetic progression: startValue + i * step.
    /// </summary>
    /// <param name="startValue">First element value.</param>
    /// <param name="step">Increment per element.</param>
    public void Fill(T startValue, T step)
    {
        for (int i = 0; i < _length; i++)
        {
            _array[i] = startValue + T.CreateChecked(i) * step;
        }
    }

    /// <summary>
    /// Fills the underlying storage using another unmanaged value by expansion or reinterpretation.
    /// </summary>
    /// <typeparam name="TNumber">The source unmanaged type.</typeparam>
    /// <param name="number">The value to replicate.</param>
    /// <remarks>
    /// Behavior depends on size relation between <typeparamref name="TNumber"/> and <typeparamref name="T"/>:
    /// larger source => expanded across buffer; equal => direct write; smaller => reinterpret and assign.
    /// </remarks>
    public void FillWith<TNumber>(TNumber number)
        where TNumber : unmanaged
    {
        /*
         * Marshal.SizeOf<bool>() == 4, Unsafe.SizeOf<bool>() == 1
         */

        var size = Unsafe.SizeOf<T>();
        var numberSize = Unsafe.SizeOf<TNumber>();

        if (numberSize > size)
        {
            var count = size * _length / numberSize;

#if use_unsafe_code
            unsafe
            {
                fixed (T* pArr = _array)
                {
                    TNumber* pDest = (TNumber*)pArr;
                    for (int i = 0; i < count; i++)
                    {
                        *(pDest + i) = number;
                    }
                }
            }
#else
            ref var addr = ref Unsafe.As<T, TNumber>(ref _array[0]);
            for (int i = 0; i < count; i++)
            {
                addr = number;
                addr = ref Unsafe.Add(ref addr, 1);
            }
#endif

        }
        else if (numberSize == size)
        {
            if (number is T n)
                Fill(n);
            else
            {
#if use_unsafe_code
                unsafe
                {
                    fixed (T* pArr = _array)
                    {
                        TNumber* pDest = (TNumber*)pArr;
                        for (int i = 0; i < _length; i++)
                        {
                            *(pDest + i) = number;
                        }
                    }
                }
#else
                ref var addr = ref Unsafe.As<T, TNumber>(ref _array[0]);
                for (int i = 0; i < _length; i++)
                {
                    addr = number;
                    addr = ref Unsafe.Add(ref addr, 1);
                }
#endif
            }

        }
        else // numberSize < size
        {
#if use_unsafe_code
            unsafe
            {
                fixed (T* pArr = _array)
                {
                    TNumber* pNumber = &number;
                    for (int i = 0; i < _length; i++)
                    {
                        *(pArr + i) = *(T*)pNumber;
                    }
                }
            }
#else
            var num = Unsafe.As<TNumber, T>(ref number);
            for (int i = 0; i < _length; i++)
            {
                _array[i] = num;
            }
#endif
        }
    }

    /// <summary>
    /// Fills each element with a random floating-point number in [0,1).
    /// For non-floating <typeparamref name="T"/>, the value is truncated.
    /// </summary>
    public void FillWithRandomNumber()
    {
        for (int i = 0; i < _length; i++)
        {
            _array[i] = T.CreateTruncating(Random.Shared.NextSingle());
        }
    }

    /// <summary>
    /// Applies a transformation to each element in-place.
    /// </summary>
    /// <param name="func">Delegate that receives (index, currentValue) and returns the new value.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="func"/> is null.</exception>
    public void Each(Func<int, T, T> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        for (int i = 0; i < _length; i++)
        {
            _array[i] = func(i, _array[i]);
        }
    }

    //! 要约束为 struct 才能把 null转为 T?

    /// <summary>
    /// Applies a neighborhood-aware transformation to each element in-place.
    /// </summary>
    /// <param name="func">
    /// Delegate receiving (index, previous?, current, next?) and returning the new value.
    /// </param>
    /// <param name="direction">Iteration direction (forward or inverse).</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="func"/> is null.</exception>
    public void Each(Func<int, T?, T, T?, T> func, Direction direction = Direction.Forward)
    {
        ArgumentNullException.ThrowIfNull(func);

        if (direction == Direction.Forward)
        {

            for (int i = 0; i < _length; i++)
            {
                if (i == 0)
                {
                    _array[i] = func(i, null, _array[i], _array[i + 1]);
                }
                else if (i == _length - 1)
                {
                    _array[i] = func(i, _array[i - 1], _array[i], null);
                }
                else
                {
                    _array[i] = func(i, _array[i - 1], _array[i], _array[i + 1]);
                }
            }

        }
        else if (direction == Direction.Inverse)
        {

            for (int i = _length - 1; i > 0; i--)
            {
                if (i == 0)
                {
                    _array[i] = func(i, null, _array[i], _array[i + 1]);
                }
                else if (i == _length - 1)
                {
                    _array[i] = func(i, _array[i - 1], _array[i], null);
                }
                else
                {
                    _array[i] = func(i, _array[i - 1], _array[i], _array[i + 1]);
                }
            }
        }
    }

    /// <summary>
    /// Delegate representing an element-wise operation that can mutate the current element by reference.
    /// </summary>
    /// <typeparam name="TElement">Element type.</typeparam>
    /// <param name="index">Current element index.</param>
    /// <param name="current">Reference to current element.</param>
    public delegate void ElementWiseActionDelegate<TElement>(int index, ref TElement current)
        where TElement : unmanaged, INumberBase<TElement>;

    /// <summary>
    /// Iterates elements and invokes the ref delegate for in-place mutation.
    /// </summary>
    /// <param name="action">Delegate invoked per element with index and ref to element.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is null.</exception>
    public void Each(ElementWiseActionDelegate<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (_length == 0)
            return;

        for (int i = 0; i < _length; i++)
        {
            action(i, ref _array[i]);
        }
    }

    #endregion


    #region IEnumerable, IEnumerable<T> members

    /// <summary>
    /// Returns a non-generic enumerator for the underlying array.
    /// </summary>
    IEnumerator IEnumerable.GetEnumerator() => _array.GetEnumerator();

    /// <summary>
    /// Returns a generic enumerator over the elements.
    /// </summary>
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)_array).GetEnumerator();

    #endregion


    #region type conversion

    /// <summary>
    /// Implicit conversion to the underlying array.
    /// The array may be larger than <see cref="Length"/> in leasing mode.
    /// </summary>
    /// <param name="pinableArray">The source pinnable array.</param>
    public static implicit operator T[](PinnableArray<T> pinableArray)
        => pinableArray._array;

    /// <summary>
    /// Implicit conversion to an unmanaged pointer. Returns null if not pinned.
    /// </summary>
    /// <param name="pinableArray">The source pinnable array.</param>
    public static unsafe implicit operator T*(PinnableArray<T> pinableArray)
        => pinableArray._isPinned ? pinableArray.Pointer : null;

    /// <summary>
    /// Implicit conversion to a <see cref="Span{T}"/> over the logical range.
    /// </summary>
    /// <param name="pinableArray">The source pinnable array.</param>
    public static implicit operator Span<T>(PinnableArray<T> pinableArray) => pinableArray._array.AsSpan(0, pinableArray._length);

    /// <summary>
    /// Implicit conversion to a native integer address. Returns <see cref="nint.Zero"/> if not pinned.
    /// </summary>
    /// <param name="pinableArray">The source pinnable array.</param>
    public static unsafe implicit operator nint(PinnableArray<T> pinableArray)
        => pinableArray._isPinned ? (nint)pinableArray._memoryHandle.Pointer : nint.Zero;

    /// <summary>
    /// Implicit conversion from array; creates a new pinnable array by copying the contents.
    /// </summary>
    /// <param name="array">The source array.</param>
    public static implicit operator PinnableArray<T>(T[] array)
        => new(array);

    /// <summary>
    /// Gets a by-ref reference to the element at <paramref name="index"/>.
    /// </summary>
    /// <param name="index">The element index.</param>
    /// <returns>Reference to the element.</returns>
    public ref T GetReference(int index)
        => ref _array[index]; //or  MemoryMarshal.GetArrayDataReference(_array);

    /// <summary>
    /// Copies the specified array into this instance. The length must match.
    /// </summary>
    /// <param name="array">The source array.</param>
    /// <exception cref="Exception">Thrown if lengths differ.</exception>
    public void From(T[] array)
    {

        if (array.Length != this._length)
            throw new Exception("array length mismatch");

        for (int i = 0; i < this._length; i++)
        {
            _array[i] = array[i];
        }
    }

    //language BUG:

    //public static implicit operator FixedArray<float>(float[] array)
    //    => new(array);

    #endregion


    #region Map

    /// <summary>
    /// Performs a linear mapping of <paramref name="number"/> from the source interval
    /// [<paramref name="fromMin"/>, <paramref name="fromMax"/>] to the target interval
    /// [<paramref name="toMin"/>, <paramref name="toMax"/>].
    /// </summary>
    /// <typeparam name="TNumber">Numeric type implementing <see cref="INumberBase{TNumber}"/>.</typeparam>
    /// <param name="number">Value in the source interval to be mapped.</param>
    /// <param name="fromMin">Lower bound of the source interval.</param>
    /// <param name="fromMax">Upper bound of the source interval.</param>
    /// <param name="toMin">Lower bound of the target interval.</param>
    /// <param name="toMax">Upper bound of the target interval.</param>
    /// <returns>
    /// The mapped value in the target interval, computed as:
    /// <c>toMin + (toRange / fromRange) * (number - fromMin)</c>.
    /// </returns>
    /// <exception cref="DivideByZeroException">Thrown if <paramref name="fromMin"/> equals <paramref name="fromMax"/>.</exception>
    private static TNumber Map<TNumber>
        (TNumber number, TNumber fromMin, TNumber fromMax, TNumber toMin, TNumber toMax)
        where TNumber : INumberBase<TNumber>
    {
        var fromRange = fromMax - fromMin;
        if (fromRange == TNumber.Zero)
            throw new DivideByZeroException("Source interval length is zero (fromMin == fromMax).");

        var toRange = toMax - toMin;
        return toMin + (toRange / fromRange) * (number - fromMin);
    }

    /// <summary>
    /// In-place linear mapping of all elements from a specified source interval
    /// to a specified target interval.
    /// </summary>
    /// <param name="fromMin">Source interval lower bound.</param>
    /// <param name="fromMax">Source interval upper bound.</param>
    /// <param name="toMin">Target interval lower bound.</param>
    /// <param name="toMax">Target interval upper bound.</param>
    /// <exception cref="DivideByZeroException">Thrown if <paramref name="fromMin"/> equals <paramref name="fromMax"/>.</exception>
    public void MapIn(T fromMin, T fromMax, T toMin, T toMax)
    {
        for (int i = 0; i < _length; i++)
        {
            _array[i] = Map(_array[i], fromMin, fromMax, toMin, toMax);
        }
    }

    /// <summary>
    /// In-place remapping of all elements to a new interval
    /// using the current data's observed minimum and maximum as the source interval.
    /// </summary>
    /// <param name="toMin">Target interval lower bound.</param>
    /// <param name="toMax">Target interval upper bound.</param>
    /// <exception cref="DivideByZeroException">Thrown if all elements are equal.</exception>
    public void MapIn(T toMin, T toMax)
    {
        var fromMin = this.Min();
        var fromMax = this.Max();

        for (int i = 0; i < _length; i++)
        {
            _array[i] = Map(_array[i], fromMin, fromMax, toMin, toMax);
        }
    }

    /// <summary>
    /// Creates a new <see cref="PinnableArray{T}"/> with each element linearly mapped
    /// from a source interval to a target interval.
    /// </summary>
    /// <param name="fromMin">Source interval lower bound.</param>
    /// <param name="fromMax">Source interval upper bound.</param>
    /// <param name="toMin">Target interval lower bound.</param>
    /// <param name="toMax">Target interval upper bound.</param>
    /// <returns>A new array containing the mapped values.</returns>
    /// <exception cref="DivideByZeroException">Thrown if <paramref name="fromMin"/> equals <paramref name="fromMax"/>.</exception>
    public PinnableArray<T> Map(T fromMin, T fromMax, T toMin, T toMax)
    {
        var result = new PinnableArray<T>(this._length);

        for (int i = 0; i < _length; i++)
        {
            result[i] = Map(_array[i], fromMin, fromMax, toMin, toMax);
        }

        return result;
    }

    /// <summary>
    /// Creates a new <see cref="PinnableArray{T}"/> by remapping all elements to
    /// a target interval, using the current data's min and max as the source interval.
    /// </summary>
    /// <param name="toMin">Target interval lower bound.</param>
    /// <param name="toMax">Target interval upper bound.</param>
    /// <returns>A new array containing the remapped values.</returns>
    /// <exception cref="DivideByZeroException">Thrown if all elements are equal.</exception>
    public PinnableArray<T> Map(T toMin, T toMax)
    {
        var fromMin = this.Min();
        var fromMax = this.Max();

        var result = new PinnableArray<T>(this._length);

        for (int i = 0; i < _length; i++)
        {
            result[i] = Map(_array[i], fromMin, fromMax, toMin, toMax);
        }
        return result;
    }

    #endregion


    #region Dot Product

    /// <summary>
    /// Computes the dot (inner) product with another <see cref="PinnableArray{T}"/>.
    /// </summary>
    /// <param name="another">The other array.</param>
    /// <returns>The scalar dot product.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="another"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if lengths differ.</exception>
    public T Dot(PinnableArray<T> another)
    {
        if (another is null) throw new ArgumentNullException();

        if (this._length != another._length) throw new InvalidOperationException("Length of two PinnableArray mismatch.");

        T result = default;
        for (int i = 0; i < _length; i++)
            result += _array[i] * another._array[i];
        return result;
    }

    #endregion


    #region Operators


    // 用户定义类型不能重载赋值运算符。
    // https://learn.microsoft.com/zh-cn/dotnet/csharp/language-reference/operators/assignment-operator
    //public static FixedArray<T> operator =(float value)



    #region op +

#if NET7_0_OR_GREATER

    /// <summary>
    /// Element-wise addition of two arrays. Produces a new array.
    /// </summary>
    /// <param name="left">Left operand.</param>
    /// <param name="right">Right operand.</param>
    /// <returns>New array containing element-wise sums.</returns>
    /// <exception cref="InvalidOperationException">Thrown if lengths differ.</exception>
    public static PinnableArray<T> operator +(PinnableArray<T> left, PinnableArray<T> right)
    {
        if (left._length != right._length)
            throw new InvalidOperationException($"The length of {nameof(left)} not equals {nameof(right)}");

        var result = new PinnableArray<T>(left._length);
        for (int i = 0; i < left._length; i++)
        {
            result[i] = left[i] + right[i];
        }
        return result;
    }


    /// <summary>
    /// Adds a scalar to each element. Produces a new array.
    /// </summary>
    /// <param name="left">Array.</param>
    /// <param name="right">Scalar addend.</param>
    /// <returns>New array containing element-wise sums.</returns>
    public static PinnableArray<T> operator +(PinnableArray<T> left, T right)
    {
        var result = new PinnableArray<T>(left._length);
        for (int i = 0; i < left._length; i++)
        {
            result[i] = left[i] + right;
        }
        return result;
    }

    /// <summary>
    /// Adds a scalar to each element. Produces a new array.
    /// </summary>
    /// <param name="left">Scalar addend.</param>
    /// <param name="right">Array.</param>
    /// <returns>New array containing element-wise sums.</returns>
    public static PinnableArray<T> operator +(T left, PinnableArray<T> right)
    {
        var result = new PinnableArray<T>(right._length);
        for (int i = 0; i < right._length; i++)
        {
            result[i] = left + right[i];
        }
        return result;
    }

#endif

    /// <summary>
    /// In-place element-wise addition with another array.
    /// </summary>
    /// <param name="right">Right-hand array.</param>
    /// <exception cref="InvalidOperationException">Thrown if lengths differ.</exception>
    public void Add(T[] right)
    {
        if (this._length != right.Length)
            throw new InvalidOperationException($"The length of current PinnableArray not equals {nameof(right)}");

        for (int i = 0; i < this._length; i++)
        {
            this._array[i] = this._array[i] + right[i];
        }
    }


    /// <summary>
    /// In-place scalar addition.
    /// </summary>
    /// <param name="right">Scalar addend.</param>
    public void Add(T right)
    {
        for (int i = 0; i < this._length; i++)
        {
            this._array[i] = this._array[i] + right;
        }
    }

    #endregion


    #region op -

#if NET7_0_OR_GREATER

    /// <summary>
    /// Element-wise subtraction of two arrays. Produces a new array.
    /// </summary>
    /// <param name="left">Minuend array.</param>
    /// <param name="right">Subtrahend array.</param>
    /// <returns>New array of differences.</returns>
    /// <exception cref="InvalidOperationException">Thrown if lengths differ.</exception>
    public static PinnableArray<T> operator -(PinnableArray<T> left, PinnableArray<T> right)
    {
        if (left._length != right._length)
            throw new InvalidOperationException($"The length of {nameof(left)} not equals {nameof(right)}");

        var result = new PinnableArray<T>(left._length);
        for (int i = 0; i < left._length; i++)
        {
            result[i] = left[i] - right[i];
        }
        return result;
    }

    /// <summary>
    /// Scalar subtraction (array - scalar). Produces a new array.
    /// </summary>
    /// <param name="left">Array.</param>
    /// <param name="right">Scalar subtrahend.</param>
    /// <returns>New array of differences.</returns>
    public static PinnableArray<T> operator -(PinnableArray<T> left, T right)
    {
        var result = new PinnableArray<T>(left._length);
        for (int i = 0; i < left._length; i++)
        {
            result[i] = left[i] - right;
        }
        return result;
    }

    /// <summary>
    /// Scalar subtraction (scalar - array). Produces a new array.
    /// </summary>
    /// <param name="left">Scalar minuend.</param>
    /// <param name="right">Array.</param>
    /// <returns>New array of differences.</returns>
    public static PinnableArray<T> operator -(T left, PinnableArray<T> right)
    {
        var result = new PinnableArray<T>(right._length);
        for (int i = 0; i < right._length; i++)
        {
            result[i] = left - right[i];
        }
        return result;
    }

#endif


    /// <summary>
    /// In-place element-wise subtraction with another array.
    /// </summary>
    /// <param name="right">Right-hand array.</param>
    /// <exception cref="InvalidOperationException">Thrown if lengths differ.</exception>
    public void Subtract(T[] right)
    {
        if (this._length != right.Length)
            throw new InvalidOperationException($"The length of current FixedArray not equals {nameof(right)}");

        for (int i = 0; i < this._length; i++)
        {
            this._array[i] = this._array[i] - right[i];
        }
    }


    /// <summary>
    /// In-place scalar subtraction.
    /// </summary>
    /// <param name="right">Scalar to subtract.</param>
    public void Subtract(T right)
    {
        for (int i = 0; i < this._length; i++)
        {
            this._array[i] = this._array[i] - right;
        }
    }

    #endregion


    #region op *

#if NET7_0_OR_GREATER

    /// <summary>
    /// Element-wise multiplication of two arrays. Produces a new array.
    /// </summary>
    /// <param name="left">Left array.</param>
    /// <param name="right">Right array.</param>
    /// <returns>New array of products.</returns>
    /// <exception cref="InvalidOperationException">Thrown if lengths differ.</exception>
    public static PinnableArray<T> operator *(PinnableArray<T> left, PinnableArray<T> right)
    {
        if (left._length != right._length)
            throw new InvalidOperationException($"The length of {nameof(left)} not equals {nameof(right)}");

        var result = new PinnableArray<T>(left._length);
        for (int i = 0; i < left._length; i++)
        {
            result[i] = left[i] * right[i];
        }
        return result;
    }

    /// <summary>
    /// Scalar multiplication (array * scalar). Produces a new array.
    /// </summary>
    /// <param name="left">Array.</param>
    /// <param name="right">Scalar multiplier.</param>
    /// <returns>New array of products.</returns>
    public static PinnableArray<T> operator *(PinnableArray<T> left, T right)
    {
        var result = new PinnableArray<T>(left._length);
        for (int i = 0; i < left._length; i++)
        {
            result[i] = left[i] * right;
        }
        return result;
    }

    /// <summary>
    /// Scalar multiplication (scalar * array). Produces a new array.
    /// </summary>
    /// <param name="left">Scalar multiplier.</param>
    /// <param name="right">Array.</param>
    /// <returns>New array of products.</returns>
    public static PinnableArray<T> operator *(T left, PinnableArray<T> right)
    {
        var result = new PinnableArray<T>(right._length);
        for (int i = 0; i < right._length; i++)
        {
            result[i] = left * right[i];
        }
        return result;
    }

#endif


    /// <summary>
    /// In-place element-wise multiplication with another array.
    /// </summary>
    /// <param name="right">Right-hand array.</param>
    /// <exception cref="InvalidOperationException">Thrown if lengths differ.</exception>
    public void Multiply(T[] right)
    {
        if (this._length != right.Length)
            throw new InvalidOperationException($"The length of current FixedArray not equals {nameof(right)}");

        for (int i = 0; i < this._length; i++)
        {
            this._array[i] = this._array[i] * right[i];
        }
    }

    /// <summary>
    /// In-place scalar multiplication.
    /// </summary>
    /// <param name="right">Scalar multiplier.</param>
    public void Multiply(T right)
    {
        for (int i = 0; i < this._length; i++)
        {
            this._array[i] = this._array[i] * right;
        }
    }

    #endregion


    #region op /

#if NET7_0_OR_GREATER

    /// <summary>
    /// Element-wise division of two arrays. Produces a new array.
    /// </summary>
    /// <param name="left">Dividend array.</param>
    /// <param name="right">Divisor array.</param>
    /// <returns>New array of quotients.</returns>
    /// <exception cref="InvalidOperationException">Thrown if lengths differ.</exception>
    public static PinnableArray<T> operator /(PinnableArray<T> left, PinnableArray<T> right)
    {
        if (left._length != right._length)
            throw new InvalidOperationException($"The length of {nameof(left)} not equals {nameof(right)}");

        var result = new PinnableArray<T>(left._length);
        for (int i = 0; i < left._length; i++)
        {
            result[i] = left[i] / right[i];
        }
        return result;
    }

    /// <summary>
    /// Scalar division (array / scalar). Produces a new array.
    /// </summary>
    /// <param name="left">Dividend array.</param>
    /// <param name="right">Scalar divisor.</param>
    /// <returns>New array of quotients.</returns>
    public static PinnableArray<T> operator /(PinnableArray<T> left, T right)
    {
        var result = new PinnableArray<T>(left._length);
        for (int i = 0; i < left._length; i++)
        {
            result[i] = left[i] - right;
        }
        return result;
    }

    /// <summary>
    /// Scalar division (scalar / array). Produces a new array.
    /// </summary>
    /// <param name="left">Scalar dividend.</param>
    /// <param name="right">Divisor array.</param>
    /// <returns>New array of quotients.</returns>
    public static PinnableArray<T> operator /(T left, PinnableArray<T> right)
    {
        var result = new PinnableArray<T>(right._length);
        for (int i = 0; i < right._length; i++)
        {
            result[i] = left / right[i];
        }
        return result;
    }

#endif


    /// <summary>
    /// In-place element-wise division with another array.
    /// </summary>
    /// <param name="right">Divisor array.</param>
    /// <exception cref="InvalidOperationException">Thrown if lengths differ.</exception>
    public void Divide(T[] right)
    {
        if (this._length != right.Length)
            throw new InvalidOperationException($"The length of current PinnableArray not equals {nameof(right)}");

        for (int i = 0; i < this._length; i++)
        {
            this._array[i] = this._array[i] / right[i];
        }
    }

    /// <summary>
    /// In-place scalar division.
    /// </summary>
    /// <param name="right">Scalar divisor.</param>
    public void Divide(T right)
    {
        for (int i = 0; i < this._length; i++)
        {
            this._array[i] = this._array[i] / right;
        }
    }


    #endregion



    #endregion


    #region Dispose Pattern

    /// <summary>
    /// Tracks whether the instance has been disposed.
    /// </summary>
    private bool _disposedValue;

    /// <summary>
    /// Releases resources held by this instance.
    /// </summary>
    /// <param name="disposing">True when called from <see cref="Dispose()"/>.</param>
    protected void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // Release managed state
                if (_isPinned) _memoryHandle.Dispose();

                if (Options.UseLeasingMode) _pool.Return(_array, false);
            }

            _disposedValue = true;
        }
    }

    // // Only needed if you add a finalizer.
    // ~PinnedArray()
    // {
    //     Dispose(disposing: false);
    // }

    /// <summary>
    /// Disposes the instance and suppresses finalization.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }


    #endregion


    #region static prop

    private static PinnableArrayOption s_options = new();

    /// <summary>
    /// Global options for all <see cref="PinnableArray{T}"/> instances of this element type.
    /// </summary>
    /// <remarks>
    /// Configure before allocating instances to affect allocation/leasing behavior.
    /// </remarks>
    public static PinnableArrayOption Options => s_options;

    #endregion



    /// <summary>
    /// Returns a string representation of the contents.
    /// For large arrays, prints the first and last segments separated by ellipses.
    /// </summary>
    public override string ToString()
    {
        var sb = new StringBuilder();

        const int threshold = 200;
        const int printCount = 20;

        if (_length < threshold)
        {
            sb.Append('[');
            for (int i = 0; i < _length; i++)
            {
                sb.Append(_array[i]);
                sb.Append(',');
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append(']');
        }
        else //>200
        {
            sb.Append('[');
            for (int i = 0; i < printCount; i++)
            {
                sb.Append(_array[i]);
                sb.Append(',');
            }
            sb.Append("......");
            sb.Append(',');

            for (int i = _length - printCount; i < _length; i++)
            {
                sb.Append(_array[i]);
                sb.Append(',');
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append(']');
        }
        return sb.ToString();
    }



}