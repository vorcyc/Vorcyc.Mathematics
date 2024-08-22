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
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Vorcyc.Mathematics.Statistics;

//https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags#list

/// <summary>
/// Represents a memory-fixed and size-fixed , one-dim array.
/// <para>
/// Use this class like an <see cref="Array"/> , but it's pinnable and efficient  , just for <see cref="INumber{TSelf}"/> .
/// </para>
/// </summary>
/// <typeparam name="T">
/// <list type="bullet">
/// <item>
/// <term> unmanaged Type</term>
/// <description>struct</description>
/// </item>
/// <item>
/// <term>IComparable</term>
/// <description><see cref="IComparable"/> , <see cref="IComparable{T}"/></description>
/// </item>
/// </list>
/// </typeparam>
public sealed class PinnableArray<T> : IDisposable, IEnumerable<T>
    where T : unmanaged, INumber<T>
{

    private readonly T[] _array;

    private readonly int _length;

    private bool _isPinned;

    private Memory<T> _memory;

    private MemoryHandle _memoryHandle;

    private ArrayPool<T> _pool = ArrayPool<T>.Create();

    #region constructor

    public PinnableArray(ArraySegment<T> segment, bool pin = true)
    {
        _length = segment.Count;
        _array = _pool.Rent(_length);
        //_array = new T[_length];

        Buffer.BlockCopy(segment.Array, segment.Offset, _array, 0, _length);

        if (pin) Pin();
    }


    public PinnableArray(Span<T> span, bool pin = true)
    {
        _length = span.Length;

        _array = _pool.Rent(_length);

        Buffer.BlockCopy(span.ToArray(), 0, _array, 0, span.Length);

        if (pin) Pin();
    }


    public PinnableArray(T[] array, bool pin = true)
    {
        _length = array.Length;


        //至于是用 Buffer还是用 Array 来 Copy，以下是 BingChat 给出的对比 ：
        /*
            Buffer.BlockCopy 更高效，但是仅限于原始类型（primitive types），即字节、整数、浮点数等12。
            Array.Copy 用于引用类型（reference types），复杂值类型（complex value types），或者相同的原始类型3。
            Array.Copy 会执行类型转换，如果源数组和目标数组的元素类型不匹配，会抛出异常45。
            Buffer.BlockCopy 不会执行类型转换，而是直接复制字节，因此可以在不同的原始类型数组之间进行“转换”3。
            Buffer.BlockCopy 的参数是基于字节的，而Array.Copy的参数是基于索引的，因此Buffer.BlockCopy更容易出错4。
            */

        //_array = new T[_length];
        _array = _pool.Rent(_length);

        Buffer.BlockCopy(array, 0, _array, 0, _length * Unsafe.SizeOf<T>());
        //_array = new TScalar[_count];
        //Array.Copy(array, _array, array.Length);


        if (pin) Pin();
    }


    public PinnableArray(T[] array, int offset, int count, bool pin = true)
    {
        if (offset < 0 || offset > array.Length)
            throw new IndexOutOfRangeException(nameof(offset));

        if (count < 0 || count > array.Length)
            throw new IndexOutOfRangeException(nameof(count));

        _length = count;

        //_array = new T[_length];
        _array = _pool.Rent(_length);

        Buffer.BlockCopy(array, offset, _array, 0, count * Unsafe.SizeOf<T>());

        if (pin) Pin();
    }

    public PinnableArray(int count, bool pin = true)
    {
        //_array = new T[count];
        _length = count;
        _array = _pool.Rent(_length);

        if (pin) Pin();
    }

    #endregion

    /// <summary>
    /// Pin this <see cref="PinnableArray{T}"/> , so you can access it's address and interactive with unmanaged code.
    /// </summary>
    public void Pin()
    {
        if (_isPinned) return;

        _memory = new Memory<T>(_array);
        _memoryHandle = _memory.Pin();
        _isPinned = true;
    }

    /// <summary>
    /// Unpin ,  so this <see cref="PinnableArray{T}"/> can be collected by GC.
    /// </summary>
    public void Unpin()
    {
        if (_isPinned)
        {
            _memoryHandle.Dispose();
            _isPinned = false;
        }
    }


    #region Properites

    /// <summary>
    /// Gets if this <see cref="PinnableArray{T}"/> is pinned.
    /// </summary>
    public bool IsPinned => _isPinned;

    /// <summary>
    /// Gets the element count of current <see cref="PinnableArray{T}"/>.
    /// </summary>
    public int Length => _length;

    /// <summary>
    /// Gets the normal array form of current <see cref="PinnableArray{T}"/>.
    /// </summary>
    public T[] Values => _array;

    #endregion


    #region AsSpan

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan() => new(_array);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan(int start, int length) => new(_array, start, length);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan(int start) => _array.AsSpan(start);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan(System.Index startIndex) => _array.AsSpan(startIndex);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan(Range range) => _array.AsSpan(range);

    #endregion


    #region Pointers

    /// <summary>
    /// 
    /// </summary>
    public unsafe T* Pointer => _isPinned ? (T*)_memoryHandle.Pointer : null;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TElement"><peparam>
    /// <returns></returns>
    public unsafe TElement* ToPointerOf<TElement>() where TElement : unmanaged
        => _isPinned ? (TElement*)_memoryHandle.Pointer : null;


    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
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
    /// Gets or sets the element by specified index.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public T this[int index]
    {
        get => _array[index];
        set => _array[index] = value;
    }

    //public T[] this[int startPos, int endPos]
    //{
    //    get
    //    {
    //        Guard.AgainstInvalidRange(startPos, endPos, "Left index", "Right index");

    //        // Implementaion is LINQ-less, since Skip() would be less efficient:
    //        //     return new DiscreteSignal(SamplingRate, Samples.Skip(startPos).Take(endPos - startPos));

    //        return _array[startPos..endPos];//用集合表达式语法会创建新的数组，所以不行！
    //    }
    //}

    /// <summary>
    /// Gets the segment as <see cref="Span{T}"/>.
    /// </summary>
    /// <param name="startPos"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public Span<T> this[int startPos, int length]
        => _array.AsSpan(startPos, length);


    //public T[] this[Range range]
    //{
    //    get
    //    {
    //        return _array[range];
    //    }
    //}

    /// <summary>
    /// Gets the segment as <see cref="Span{T}"/> by collection expression.
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public Span<T> this[Range range]
        => _array.AsSpan(range);



    #endregion


    #region Element-wise operations

    /// <summary>
    /// Fill up all data with specified value with same type.
    /// </summary>
    /// <param name="value"></param>
    public void Fill(T value)
    {
        for (int i = 0; i < _length; i++)
        {
            _array[i] = value;
        }
    }

    public void Fill(T startValue, T step)
    {
        for (int i = 0; i < _length; i++)
        {
            _array[i] = startValue + T.CreateChecked(i) * step;
        }
    }

    /// <summary>
    /// Fill up with another unmanaged type.
    /// <para>
    /// If the size of <typeparamref name="TNumber"/> is bigger than <typeparamref name="T"/> 
    /// , then do byte-expland operation.
    /// </para>
    /// </summary>
    /// <typeparam name="TNumber"></typeparam>
    /// <param name="number"></param>
    public void FillWith<TNumber>(TNumber number)
        where TNumber : unmanaged
    {
        /*
         * ! 这里注意：
         * Marshal.SizeOf<bool>().PrintLine(); 为 4
         * Unsafe.SizeOf<bool>().PrintLine(); 为 1
         */

        var size = Unsafe.SizeOf<T>();
        var numberSize = Unsafe.SizeOf<TNumber>();

        if (numberSize > size)//! 填充的是更大的类型 ，把大的类型的按位展开
        {
            // size : 若 T 是 byte ，为 1
            // numberSize : 若 TNumber 是 short ，为 2
            var count = size * _length / numberSize;
            // count : 假如长度为 10， 为 5

#if use_unsafe_code
            unsafe
            {
                fixed (T* pArr = _array)
                {
                    TNumber* pDest = (TNumber*)pArr;
                    for (int i = 0; i < count; i++)// 按大的来，跑 5 次
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
        else if (numberSize == size)//! 尺寸相同，这里还不能说明是同样类型
        {
            //x if (typeof(TNumber) == typeof(T))
            //直接模式匹配即可
            if (number is T n)//! 类型相同
                Fill(n);
            else//! 类型不同
            {
#if use_unsafe_code
                unsafe
                {
                    fixed (T* pArr = _array)
                    {
                        TNumber* pDest = (TNumber*)pArr;// 因为尺寸相同，所以就强制转了
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
        else if (numberSize < size)//! 填充小的类型，填就是了
        {
            //for (int i = 0; i < _length; i++)
            //{
            //    _array[i] = (T)number; //无法类型转换
            //}

            //! 还是得靠 指针
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
    /// &gt;= 0.0 and &lt; 1
    /// </summary>
    public void FillWithRandomNumber()
    {
        for (int i = 0; i < _length; i++)
        {
            _array[i] = T.CreateTruncating(Random.Shared.NextSingle());
        }
    }

    /// <summary>
    /// Iterate each element in current <see cref="PinnableArray{T}"/> with <see cref="Func{T1, T2, TResult}"/>.
    /// The first argument is <see cref="int"/> , which represents index.
    /// </summary>
    /// <param name="func"></param>
    /// <exception cref="ArgumentNullException"></exception>
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
    /// Iterate each element in current <see cref="PinnableArray{T}"/> with <see cref="Func{T1, T2, T3, T4, T5, TResult}"/>.
    /// </summary>
    /// <param name="func">
    /// <list type="number">
    ///     <item>
    ///         <term><see cref="int"/></term>
    ///         <description>index</description>
    ///     </item>
    ///     <item>
    ///         <term><typeparamref name="T"/>? </term>
    ///         <description>The previous element in array , like array[i-1] , maybe is null.</description>
    ///     </item>
    ///     <item>
    ///         <term><typeparamref name="T"/></term>
    ///         <description>The current element in array , like array[i] .</description>
    ///     </item>
    ///     <item>
    ///         <term><typeparamref name="T"/>?</term>
    ///         <description>The next element in array , like array[i+1]  , maybe is null.</description>
    ///     </item>   
    ///     <item>
    ///         <term><typeparamref name="T"/></term>
    ///         <description>The return value.</description>
    ///     </item>
    /// </list>
    /// </param>
    /// <param name="direction">Forward or backward.</param>
    public void Each(Func<int, T?, T, T?, T> func, Direction direction = Direction.Forward)
    {
        ArgumentNullException.ThrowIfNull(func);

        if (direction == Direction.Forward)
        {

            for (int i = 0; i < _length; i++)
            {
                if (i == 0)//第一个样本
                {
                    _array[i] = func(i, null, _array[i], _array[i + 1]);
                }
                else if (i == _length - 1)//最后一个样本
                {
                    _array[i] = func(i, _array[i - 1], _array[i], null);
                }
                else//其它中间的
                {
                    _array[i] = func(i, _array[i - 1], _array[i], _array[i + 1]);
                }
            }

        }
        else if (direction == Direction.Inverse)
        {

            for (int i = _length - 1; i > 0; i--)
            {
                if (i == 0)//第一个样本
                {
                    _array[i] = func(i, null, _array[i], _array[i + 1]);
                }
                else if (i == _length - 1)//最后一个样本
                {
                    _array[i] = func(i, _array[i - 1], _array[i], null);
                }
                else//其它中间的
                {
                    _array[i] = func(i, _array[i - 1], _array[i], _array[i + 1]);
                }
            }
        }
    }



    #endregion


    #region IEnumerable, IEnumerable<T> members

    IEnumerator IEnumerable.GetEnumerator() => _array.GetEnumerator();

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)_array).GetEnumerator();

    #endregion


    #region type conversion


    public static implicit operator T[](PinnableArray<T> pinableArray)
        => pinableArray._array;


    public static unsafe implicit operator T*(PinnableArray<T> pinableArray)
        => pinableArray._isPinned ? pinableArray.Pointer : null;

    /// <summary>
    /// Converts to a <see cref="Span{T}"/>
    /// </summary>
    /// <param name="pinableArray"></param>
    public static implicit operator Span<T>(PinnableArray<T> pinableArray) => pinableArray._array.AsSpan();


    public static unsafe implicit operator nint(PinnableArray<T> pinableArray)
        => pinableArray._isPinned ? (nint)pinableArray._memoryHandle.Pointer : nint.Zero;


    public static implicit operator PinnableArray<T>(T[] array)
        => new(array);


    public ref T GetReference(int index)
        => ref _array[index]; //or  MemoryMarshal.GetArrayDataReference(_array);

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


    #region Max

    /// <summary>
    /// Returns the maximum of current <see cref="PinnableArray{TScalar}"/>.
    /// </summary>
    /// <returns></returns>
    public T Max() => MaxHelper.Max(_array);// _scalars.Max();


    /// <summary>
    /// Returns the maximum within a sub-range of current <see cref="PinnableArray{T}"/>.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public T Max(int start, int length) => _array.Max(start, length);


    /// <summary>
    /// Gets the maximum of current instance of <see cref="PinnableArray{T}"/> in async mode.
    /// </summary>
    /// <param name="numberOfWorkers">This paramater specify the number of threads to compute, 
    /// this value is less than or equal to <strong>Environment.ProcessorCount</strong>.</param>
    /// <param name="useTPL">If ture, use TPL and ingore the paramater <strong>numberOfWorkers</strong>,otherwise use self-defined methods instead.</param>
    /// <returns></returns>
    public async Task<T> MaxAsync(int? numberOfWorkers = null, bool useTPL = false)
        => await _array.MaxAsync(numberOfWorkers, useTPL);



    /// <summary>
    /// Returns the maximum within sub-range of current instance of <see cref="PinnableArray{T}"/> in a parallel and async mode.
    /// </summary>
    /// <param name="start">The start index of sub-range.</param>
    /// <param name="length">The length of sub-range.</param>
    /// <param name="numberOfWorkers">his paramater specify the number of cores to compute,this value is less than or equal to <strong>Environment.ProcessorCount</strong>.</param>
    /// <param name="useTPL">If ture, use TPL and ingore the paramater <strong>numberOfWorkers</strong>,otherwise use self-defined methods instead.</param>
    /// <returns></returns>
    public async Task<T> MaxAsync(int start, int length, int? numberOfWorkers = null, bool useTPL = false)
        => await _array.MaxAsync(start, length, numberOfWorkers, useTPL);



    #endregion


    #region Min

    public T Min() => MinHelper.Min(_array);

    public T Min(int start, int length) => MinHelper.Min(_array, start, length);

    public async Task<T> MinAsync(int? numberOfWorkers = null, bool useTPL = false)
        => await _array.MinAsync(numberOfWorkers, useTPL);


    public async Task<T> MinAsync(int start, int length, int? numberOfWorkers = null, bool useTPL = false)
        => await _array.MinAsync(start, length, numberOfWorkers, useTPL);



    #endregion


    #region Map

    private static TNumber Map<TNumber>
        (TNumber number, TNumber fromMin, TNumber fromMax, TNumber toMin, TNumber toMax)
        where TNumber : INumber<TNumber>
    {
        var fromRange = fromMax - fromMin;
        var toRange = toMax - toMin;
        return toMin + (toRange / fromRange) * (number - fromMin);
    }



    public void MapIn(T fromMin, T fromMax, T toMin, T toMax)
    {
        for (int i = 0; i < _length; i++)
        {
            _array[i] = Map(_array[i], fromMin, fromMax, toMin, toMax);
        }
    }


    public void MapIn(T toMin, T toMax)
    {
        var fromMin = this.Min();
        var fromMax = this.Max();

        for (int i = 0; i < _length; i++)
        {
            _array[i] = Map(_array[i], fromMin, fromMax, toMin, toMax);
        }
    }

    public PinnableArray<T> Map(T fromMin, T fromMax, T toMin, T toMax)
    {
        var result = new PinnableArray<T>(this._length);

        for (int i = 0; i < _length; i++)
        {
            result[i] = Map(_array[i], fromMin, fromMax, toMin, toMax);
        }

        return result;
    }


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
    /// Dot product (inner product) with another <see cref="PinnableArray{T}"/>.
    /// </summary>
    /// <param name="another"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    /// <remarks>
    /// https://www.tutorialspoint.com/cplusplus-program-for-dot-product-and-cross-product-of-two-vectors#
    /// </remarks>
    public T Dot(PinnableArray<T> another)
    {
        if (another is null) throw new ArgumentNullException();

        if (this._length != another._length) throw new InvalidOperationException("Length of two PinnableArray mismatch.");

        T result = default;
        for (int i = 0; i < _length; i++)
            result += _array[i] * another._array[i];
        return result;
    }



    //public T[] Cross(FixedArray<T> another)
    //{
    //    if (another is null) throw new ArgumentNullException();

    //    if (this._length != another._length) throw new InvalidOperationException("Length of two FixedArray mismatch");

    //    T[] result = new T[_length];
    //    for (int i = 0; i < _length; i++)
    //        result[i] = _data[i];
    //    return result;
    //}


    #endregion


    #region Operators


    // 用户定义类型不能重载赋值运算符。
    // https://learn.microsoft.com/zh-cn/dotnet/csharp/language-reference/operators/assignment-operator
    //public static FixedArray<T> operator =(float value)



    #region op +

#if NET7_0_OR_GREATER

    public static PinnableArray<T> operator +(PinnableArray<T> left, PinnableArray<T> right)
    {
        //if (left is null)
        //    throw new NullReferenceException($"{nameof(left)} is null");

        //if (right is null)
        //    throw new NullReferenceException($"{nameof(right)} is null");

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
    /// Element-wise add. 
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static PinnableArray<T> operator +(PinnableArray<T> left, T right)
    {
        //if (left is null)
        //    throw new NullReferenceException($"{nameof(left)} is null");

        //if (right is null)
        //    throw new NullReferenceException($"{nameof(right)} is null");

        var result = new PinnableArray<T>(left._length);
        for (int i = 0; i < left._length; i++)
        {
            result[i] = left[i] + right;
        }
        return result;
    }

    public static PinnableArray<T> operator +(T left, PinnableArray<T> right)
    {
        //if (left is null)
        //    throw new NullReferenceException($"{nameof(left)} is null");

        //if (right is null)
        //    throw new NullReferenceException($"{nameof(right)} is null");

        var result = new PinnableArray<T>(right._length);
        for (int i = 0; i < right._length; i++)
        {
            result[i] = left + right[i];
        }
        return result;
    }

#endif

    /// <summary>
    /// Element-wise add with another array. in-place
    /// </summary>
    /// <param name="right"></param>
    /// <exception cref="InvalidOperationException"></exception>
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
    /// Element-wise add with another array. in-place
    /// </summary>
    /// <param name="right"></param>
    /// <exception cref="InvalidOperationException"></exception>
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

    public static PinnableArray<T> operator -(PinnableArray<T> left, PinnableArray<T> right)
    {
        //if (left is null)
        //    throw new NullReferenceException($"{nameof(left)} is null");

        //if (right is null)
        //    throw new NullReferenceException($"{nameof(right)} is null");

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
    /// Element-wise sub. 
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public static PinnableArray<T> operator -(PinnableArray<T> left, T right)
    {
        //if (left is null)
        //    throw new NullReferenceException($"{nameof(left)} is null");

        //if (right is null)
        //    throw new NullReferenceException($"{nameof(right)} is null");

        var result = new PinnableArray<T>(left._length);
        for (int i = 0; i < left._length; i++)
        {
            result[i] = left[i] - right;
        }
        return result;
    }

    public static PinnableArray<T> operator -(T left, PinnableArray<T> right)
    {
        //if (left is null)
        //    throw new NullReferenceException($"{nameof(left)} is null");

        //if (right is null)
        //    throw new NullReferenceException($"{nameof(right)} is null");

        var result = new PinnableArray<T>(right._length);
        for (int i = 0; i < right._length; i++)
        {
            result[i] = left - right[i];
        }
        return result;
    }

#endif


    /// <summary>
    /// Element-wise subtract with another array.
    /// </summary>
    /// <param name="right"></param>
    /// <exception cref="InvalidOperationException"></exception>
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
    /// Element-wise subtract with another array.
    /// </summary>
    /// <param name="right"></param>
    /// <exception cref="InvalidOperationException"></exception>
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

    public static PinnableArray<T> operator *(PinnableArray<T> left, PinnableArray<T> right)
    {
        //if (left is null)
        //    throw new ArgumentNullException($"{nameof(left)} is null");

        //if (right is null)
        //    throw new ArgumentNullException($"{nameof(right)} is null");

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
    /// Element-wise sub. 
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static PinnableArray<T> operator *(PinnableArray<T> left, T right)
    {
        //if (left is null)
        //    throw new ArgumentNullException($"{nameof(left)} is null");

        //if (right is null)
        //    throw new ArgumentNullException($"{nameof(right)} is null");

        var result = new PinnableArray<T>(left._length);
        for (int i = 0; i < left._length; i++)
        {
            result[i] = left[i] * right;
        }
        return result;
    }

    public static PinnableArray<T> operator *(T left, PinnableArray<T> right)
    {
        //if (left is null)
        //    throw new ArgumentNullException($"{nameof(left)} is null");

        //if (right is null)
        //    throw new ArgumentNullException($"{nameof(right)} is null");

        var result = new PinnableArray<T>(right._length);
        for (int i = 0; i < right._length; i++)
        {
            result[i] = left * right[i];
        }
        return result;
    }

#endif


    /// <summary>
    /// Element-wise multiply with another array.
    /// </summary>
    /// <param name="right"></param>
    /// <exception cref="InvalidOperationException"></exception>
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
    /// Element-wise multiply with another array.
    /// </summary>
    /// <param name="right"></param>
    /// <exception cref="InvalidOperationException"></exception>
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

    public static PinnableArray<T> operator /(PinnableArray<T> left, PinnableArray<T> right)
    {
        //if (left is null)
        //    throw new ArgumentNullException($"{nameof(left)} is null");

        //if (right is null)
        //    throw new ArgumentNullException($"{nameof(right)} is null");

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
    /// Element-wise divide. 
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static PinnableArray<T> operator /(PinnableArray<T> left, T right)
    {
        //if (left is null)
        //    throw new ArgumentNullException($"{nameof(left)} is null");

        //if (right is null)
        //    throw new ArgumentNullException($"{nameof(right)} is null");

        var result = new PinnableArray<T>(left._length);
        for (int i = 0; i < left._length; i++)
        {
            result[i] = left[i] - right;
        }
        return result;
    }

    public static PinnableArray<T> operator /(T left, PinnableArray<T> right)
    {
        //if (left is null)
        //    throw new ArgumentNullException($"{nameof(left)} is null");

        //if (right is null)
        //    throw new ArgumentNullException($"{nameof(right)} is null");

        var result = new PinnableArray<T>(right._length);
        for (int i = 0; i < right._length; i++)
        {
            result[i] = left / right[i];
        }
        return result;
    }

#endif


    /// <summary>
    /// Element-wise divide with another array.
    /// </summary>
    /// <param name="right"></param>
    /// <exception cref="InvalidOperationException"></exception>
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
    /// Element-wise divide with another array.
    /// </summary>
    /// <param name="right"></param>
    /// <exception cref="InvalidOperationException"></exception>
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

    private bool _disposedValue;

    protected void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // TODO: 释放托管状态(托管对象)
                if (_isPinned) _memoryHandle.Dispose();

                _pool.Return(_array, false);
            }

            // TODO: 释放未托管的资源(未托管的对象)并重写终结器
            // TODO: 将大型字段设置为 null
            _disposedValue = true;
        }
    }

    // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
    // ~PinnedArray()
    // {
    //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }


    #endregion




    /// <summary>
    /// overrided
    /// </summary>
    /// <returns></returns>
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