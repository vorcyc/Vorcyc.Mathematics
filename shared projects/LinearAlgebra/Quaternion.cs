using System.Numerics;

namespace Vorcyc.Mathematics.LinearAlgebra;

/// <summary>
/// Represents a quaternion with components of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The numeric type of the quaternion components.</typeparam>
public readonly struct Quaternion<T>
    where T : IBinaryFloatingPointIeee754<T>
{
    /// <summary>
    /// Gets the W component of the quaternion.
    /// </summary>
    public T W { get; }

    /// <summary>
    /// Gets the X component of the quaternion.
    /// </summary>
    public T X { get; }

    /// <summary>
    /// Gets the Y component of the quaternion.
    /// </summary>
    public T Y { get; }

    /// <summary>
    /// Gets the Z component of the quaternion.
    /// </summary>
    public T Z { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Quaternion{T}"/> struct with the specified components.
    /// </summary>
    /// <param name="w">The W component.</param>
    /// <param name="x">The X component.</param>
    /// <param name="y">The Y component.</param>
    /// <param name="z">The Z component.</param>
    public Quaternion(T w, T x, T y, T z)
    {
        W = w;
        X = x;
        Y = y;
        Z = z;
    }

    /// <summary>
    /// Returns the conjugate of the quaternion.
    /// </summary>
    /// <returns>The conjugate of the quaternion.</returns>
    public Quaternion<T> Conjugate() => new(W, -X, -Y, -Z);

    /// <summary>
    /// Returns the norm (magnitude) of the quaternion.
    /// </summary>
    /// <returns>The norm of the quaternion.</returns>
    public T Norm() => T.Sqrt(W * W + X * X + Y * Y + Z * Z);

    /// <summary>
    /// Returns the normalized quaternion.
    /// </summary>
    /// <returns>The normalized quaternion.</returns>
    public Quaternion<T> Normalize()
    {
        var norm = Norm();
        return new Quaternion<T>(W / norm, X / norm, Y / norm, Z / norm);
    }

    /// <summary>
    /// Returns the inverse of the quaternion.
    /// </summary>
    /// <returns>The inverse of the quaternion.</returns>
    public Quaternion<T> Inverse()
    {
        var normSquared = W * W + X * X + Y * Y + Z * Z;
        return new Quaternion<T>(W / normSquared, -X / normSquared, -Y / normSquared, -Z / normSquared);
    }

    /// <summary>
    /// Adds two quaternions.
    /// </summary>
    /// <param name="a">The first quaternion.</param>
    /// <param name="b">The second quaternion.</param>
    /// <returns>The sum of the two quaternions.</returns>
    public static Quaternion<T> operator +(Quaternion<T> a, Quaternion<T> b) =>
        new(a.W + b.W, a.X + b.X, a.Y + b.Y, a.Z + b.Z);

    /// <summary>
    /// Subtracts one quaternion from another.
    /// </summary>
    /// <param name="a">The first quaternion.</param>
    /// <param name="b">The second quaternion.</param>
    /// <returns>The difference of the two quaternions.</returns>
    public static Quaternion<T> operator -(Quaternion<T> a, Quaternion<T> b) =>
        new(a.W - b.W, a.X - b.X, a.Y - b.Y, a.Z - b.Z);

    /// <summary>
    /// Multiplies two quaternions.
    /// </summary>
    /// <param name="a">The first quaternion.</param>
    /// <param name="b">The second quaternion.</param>
    /// <returns>The product of the two quaternions.</returns>
    public static Quaternion<T> operator *(Quaternion<T> a, Quaternion<T> b) =>
        new(
            a.W * b.W - a.X * b.X - a.Y * b.Y - a.Z * b.Z,
            a.W * b.X + a.X * b.W + a.Y * b.Z - a.Z * b.Y,
            a.W * b.Y - a.X * b.Z + a.Y * b.W + a.Z * b.X,
            a.W * b.Z + a.X * b.Y - a.Y * b.X + a.Z * b.W
        );

    /// <summary>
    /// Divides one quaternion by another.
    /// </summary>
    /// <param name="a">The first quaternion.</param>
    /// <param name="b">The second quaternion.</param>
    /// <returns>The quotient of the two quaternions.</returns>
    public static Quaternion<T> operator /(Quaternion<T> a, Quaternion<T> b) =>
        a * b.Inverse();

    /// <summary>
    /// Returns a string representation of the quaternion.
    /// </summary>
    /// <returns>A string representation of the quaternion.</returns>
    public override string ToString() =>
        $"Quaternion({W}, {X}, {Y}, {Z})";

    /// <summary>
    /// Determines whether the specified object is equal to the current quaternion.
    /// </summary>
    /// <param name="obj">The object to compare with the current quaternion.</param>
    /// <returns>true if the specified object is equal to the current quaternion; otherwise, false.</returns>
    public override bool Equals(object? obj) =>
        obj is Quaternion<T> other && W.Equals(other.W) && X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);

    /// <summary>
    /// Returns the hash code for the current quaternion.
    /// </summary>
    /// <returns>A hash code for the current quaternion.</returns>
    public override int GetHashCode() =>
        HashCode.Combine(W, X, Y, Z);

    /// <summary>
    /// Determines whether two quaternions are equal.
    /// </summary>
    /// <param name="left">The first quaternion.</param>
    /// <param name="right">The second quaternion.</param>
    /// <returns>true if the quaternions are equal; otherwise, false.</returns>
    public static bool operator ==(Quaternion<T> left, Quaternion<T> right) =>
        left.Equals(right);

    /// <summary>
    /// Determines whether two quaternions are not equal.
    /// </summary>
    /// <param name="left">The first quaternion.</param>
    /// <param name="right">The second quaternion.</param>
    /// <returns>true if the quaternions are not equal; otherwise, false.</returns>
    public static bool operator !=(Quaternion<T> left, Quaternion<T> right) =>
        !(left == right);
}
