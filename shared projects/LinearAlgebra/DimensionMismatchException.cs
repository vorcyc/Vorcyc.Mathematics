﻿/*
 *  Based on the Accord.NET Framework project.
 */

namespace Vorcyc.Mathematics.LinearAlgebra;

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;


/// <summary>
///   Dimension Mismatch Exception.
/// </summary>
///
/// <remarks><para>The dimension mismatch exception is thrown in cases where a method expects 
/// a matrix or array object having specific or compatible dimensions, such as the inner matrix
/// dimensions in matrix multiplication.</para>
/// </remarks>
///
[Serializable]
public class DimensionMismatchException : ArgumentException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DimensionMismatchException"/> class.
    /// </summary>
    public DimensionMismatchException() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DimensionMismatchException"/> class.
    /// </summary>
    /// 
    /// <param name="paramName">The name of the parameter that caused the current exception.</param>
    /// 
    public DimensionMismatchException(string paramName) :
        base(paramName, "Array dimensions must match.")
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DimensionMismatchException"/> class.
    /// </summary>
    /// 
    /// <param name="paramName">The name of the parameter that caused the current exception.</param>
    /// <param name="message">Message providing some additional information.</param>
    /// 
    public DimensionMismatchException(string paramName, string message) :
        base(message, paramName)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DimensionMismatchException"/> class.
    /// </summary>
    /// 
    /// <param name="message">Message providing some additional information.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    /// 
    public DimensionMismatchException(string message, Exception innerException) :
        base(message, innerException)
    { }


    /// <summary>
    /// Initializes a new instance of the <see cref="DimensionMismatchException"/> class.
    /// </summary>
    /// 
    /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
    /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// The <paramref name="info"/> parameter is null.
    /// </exception>
    /// <exception cref="T:System.Runtime.Serialization.SerializationException">
    /// The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0).
    /// </exception>
    /// 
    protected DimensionMismatchException(SerializationInfo info, StreamingContext context) :
        base(info, context)
    { }

}