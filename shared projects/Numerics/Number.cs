using System.Text;

namespace Vorcyc.Mathematics.Numerics;

/// <summary>
/// A class that represents a number as a list of digits
/// </summary>
/// <remarks>
/// 该类型支持所有数值类型的操作 + - * / 移位
/// </remarks>
/// <example>
/// 以下代码演示如何使用理论上无限精度的两个数值并对它们做乘法。
/// <code>
/// Number n1 = new Number("9999999999999999999999999999999999999999999999999999999999999999.99999999999999999");
/// Number n2 = new Number("1111111111111111111111111111111111111111111111.11111111111111111");
/// Console.WriteLine(n1*n2);
/// => 11111111111111111111111111111111111111111111111111111111111111099999999999
/// 99999999999999999999999999999999999999999999999999999999999999999999999999999
/// 99999999999999999999999999999999999999999999999999999999999999999999999999999
/// 999999999999999999988888888888888888888888888888.8888888888888888888888888888888889
/// </code>
/// 可以从系统内建的数值类型进行转换
/// <code>
/// Number number = 123456; // from int32
/// Number number2 = 123123L; // from int64
/// Number number3 = 123.0f; // from float
/// Number number4 = 123.33333m; // from decimal
/// Number number5 = 3.14 // form double
/// </code>
/// </example>
public class Number : IComparable<Number>
{
    #region Constants
    /// <summary>
    /// The maximum number of floating point digits.
    /// </summary>
    private const int MAX_FLOATING_DIGITS = 100;

    #endregion

    #region Fields

    /// <summary>
    /// The list that contains the digits.
    /// </summary>
    private LinkedList<int> digits = new LinkedList<int>();
    /// <summary>
    /// The number of floating digits.
    /// </summary>
    private int floatingDigitsCount = 0;
    /// <summary>
    /// True if the number is negative, false otherwise.
    /// </summary>
    private bool isNegative;

    #endregion

    #region Constructors
    /// <summary>
    /// The default constructor. Sets the number to zero.
    /// </summary>
    public Number()
        : this(0)
    { }
    /// <summary>
    /// Sets the number to a decimal value.
    /// </summary>
    /// <param name="value">The initial value of the number.</param>
    public Number(decimal value)
    {
        SetValue(value);
    }
    /// <summary>
    /// Sets the number to the value of another Number instance.
    /// </summary>
    /// <param name="value">The initial value of the number.</param>
    public Number(Number value)
    {
        if (Number.Equals(value, null))
        {
            SetValue(0);
        }
        else
        {
            SetValue(value);
        }
    }
    /// <summary>
    /// Sets the number to a string-represented value.
    /// </summary>
    /// <param name="value">The initial value of the number. The accepted format is: [-]x[.y]</param>
    /// <exception cref="ArgumentException"></exception>
    public Number(string value)
    {
        if (String.IsNullOrEmpty(value))
        {
            SetValue(0);
        }
        else
        {
            SetValue(value);
        }
    }

    // Used internally
    private Number(LinkedList<int> value, int floatingDigitsCount, bool isNegative)
    {
        if (value == null)
        {
            SetValue(0);
        }
        else
        {
            SetValue(value, floatingDigitsCount, isNegative);
        }
    }

    #endregion

    #region Public Properties
    /// <summary>
    /// The number of digits.
    /// </summary>
    public int DigitsCount
    {
        get { return digits.Count; }
    }
    /// <summary>
    /// The number of integer digits.
    /// </summary>
    public int IntegerDigitsCount
    {
        get { return digits.Count - floatingDigitsCount; }
    }
    /// <summary>
    /// The number of floating digits.
    /// </summary>
    public int FloatingDigitsCount
    {
        get { return floatingDigitsCount; }
    }
    /// <summary>
    /// The first digit of the number.
    /// </summary>
    public int FirstDigit
    {
        get { return digits.First.Value; }
        private set { digits.First.Value = value; }
    }
    /// <summary>
    /// The last digit of the number.
    /// </summary>
    public int LastDigit
    {
        get { return digits.Last.Value; }
    }

    #endregion

    #region Overriden Methods
    /// <summary>
    /// Converts the number to its string representation.
    /// </summary>
    /// <returns>The string representation of the number.</returns>
    public override string ToString()
    {
        StringBuilder stringBuilder = new StringBuilder(DigitsCount);

        if (this < 0)
        {
            stringBuilder.Append('-');
        }

        LinkedListNode<int> currentNode = digits.First;
        while (currentNode != null)
        {
            stringBuilder.Append(currentNode.Value);
            currentNode = currentNode.Next;
        }

        if (floatingDigitsCount != 0)
        {
            stringBuilder.Insert(stringBuilder.Length - floatingDigitsCount, ".");
        }

        return stringBuilder.ToString();
    }
    /// <summary>
    /// Determins if the value of the number is equal to another number.
    /// </summary>
    /// <param name="obj">The number to check for equality. Must be an instance of the Number class.</param>
    /// <returns>True if the two numbers are equal, false otherwise.</returns>
    public override bool Equals(object obj)
    {
        Number otherNumber = obj as Number;
        if (otherNumber != null)
        {
            return (this.CompareTo(otherNumber) == 0);
        }
        else
        {
            return false;
        }
    }
    /// <summary>
    /// Calculates a hash code for the current instance.
    /// </summary>
    /// <returns>The hash code of the current instance.</returns>
    public override int GetHashCode()
    {
        return this.ToString().GetHashCode();
    }

    #endregion

    #region Public Static Methods
    /// <summary>
    /// Calculates the absolute value of a number.
    /// </summary>
    /// <param name="numberToConvert">The number to convert.</param>
    /// <returns>The absolute value of the input parameter.</returns>
    public static Number Abs(Number numberToConvert)
    {
        Number resultNumber = new Number(numberToConvert);
        resultNumber.isNegative = false;
        return resultNumber;
    }
    /// <summary>
    /// Rounds a number to a specified number of floating digits.
    /// </summary>
    /// <param name="numberToConvert">The number to convert.</param>
    /// <param name="newFloatingDigitsCount">The new number of floating digits.</param>
    /// <returns>The rounded input parameter.</returns>
    public static Number Round(Number numberToConvert, int newFloatingDigitsCount)
    {
        Number resultNumber = new Number(numberToConvert);

        while (resultNumber.floatingDigitsCount > newFloatingDigitsCount + 1)
        {
            resultNumber.RemoveLastDigit();
        }
        if (resultNumber.floatingDigitsCount == newFloatingDigitsCount + 1)
        {
            resultNumber.RoundLastDigit();
        }

        return resultNumber;
    }
    /// <summary>
    /// Calculates the biggest integer number that is smaller than a specified floating point number. If the specified number is integer, returns the same number.
    /// </summary>
    /// <param name="numberToConvert">The number to convert.</param>
    /// <returns>The floor value of the input parameter.</returns>
    public static Number Floor(Number numberToConvert)
    {
        Number resultNumber = new Number(numberToConvert);

        while (resultNumber.floatingDigitsCount > 0)
        {
            resultNumber.RemoveLastDigit();
        }

        return resultNumber;
    }
    /// <summary>
    /// Calculates the smallest integer number that is bigger than a specified floating point number. If the specified number is integer, returns the same number.
    /// </summary>
    /// <param name="numberToConvert">The number to convert.</param>
    /// <returns>The ceiling value of the input parameter.</returns>
    public static Number Ceiling(Number numberToConvert)
    {
        Number resultNumber = null;

        if (numberToConvert.floatingDigitsCount != 0)
        {
            resultNumber = Number.Floor(numberToConvert);
            resultNumber++;
        }
        else
        {
            resultNumber = new Number(numberToConvert);
        }

        return resultNumber;
    }
    /// <summary>
    /// Calculates the non-integer part of a number. If the number is integer, returns 0.
    /// </summary>
    /// <param name="numberToConvert">The number to convert.</param>
    /// <returns>The non-integer part of a number.</returns>
    public static Number GetFloatingPart(Number numberToConvert)
    {
        Number resultNumber = new Number(numberToConvert);

        while (resultNumber.floatingDigitsCount < resultNumber.DigitsCount - 1)
        {
            resultNumber.RemoveFirstDigit();
        }
        resultNumber.FirstDigit = 0;

        return resultNumber;
    }

    /// <summary>
    /// Appends a new first digit to a number.
    /// </summary>
    /// <param name="numberToConvert">The number to convert.</param>
    /// <param name="newFirstDigit">The new first digit. Must be between 0 and 9.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <returns>The converted input parameter.</returns>
    public static Number AppendFirstDigit(Number numberToConvert, int newFirstDigit)
    {
        if (newFirstDigit >= 0 && newFirstDigit <= 9)
        {
            Number resultNumber = new Number(numberToConvert);
            resultNumber.digits.AddFirst(newFirstDigit);
            return resultNumber;
        }
        else
        {
            throw new ArgumentOutOfRangeException("newDigitValue", "The digit value must be between 0 and 9.");
        }
    }
    /// <summary>
    /// Removes the first digit of a number. The number must have at least one digit.
    /// </summary>
    /// <param name="numberToConvert">The number to convert.</param>
    /// <exception cref="InvalidOperationException"></exception>
    /// <returns>The converted input parameter.</returns>
    public static Number RemoveFirstDigit(Number numberToConvert)
    {
        if (numberToConvert.DigitsCount > 0)
        {
            Number resultNumber = new Number(numberToConvert);
            resultNumber.digits.RemoveFirst();
            return resultNumber;
        }
        else
        {
            throw new InvalidOperationException("The number has no digits to remove.");
        }
    }
    /// <summary>
    /// Appends a digit to the end of a number.
    /// </summary>
    /// <param name="numberToConvert">The number to convert.</param>
    /// <param name="newDigitValue">The digit to append. Must be between 0 and 9.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <returns>The converted input parameter.</returns>
    public static Number AppendLastDigit(Number numberToConvert, int newDigitValue)
    {
        return AppendLastDigit(numberToConvert, newDigitValue, false);
    }
    /// <summary>
    /// Appends a digit to the end of a number.
    /// </summary>
    /// <param name="numberToConvert">The number to convert.</param>
    /// <param name="newDigitValue">The digit to append. Must be between 0 and 9.</param>
    /// <param name="setFloat">True if the new digit should appear after a floating point, false otherwise.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <returns>The converted input parameter.</returns>
    public static Number AppendLastDigit(Number numberToConvert, int newDigitValue, bool setFloat)
    {
        if (newDigitValue >= 0 && newDigitValue <= 9)
        {
            Number resultNumber = new Number(numberToConvert);
            resultNumber.digits.AddLast(newDigitValue);

            if (resultNumber.floatingDigitsCount > 0 || setFloat == true)
            {
                resultNumber.floatingDigitsCount++;
            }
            return resultNumber;
        }
        else
        {
            throw new ArgumentOutOfRangeException("newDigitValue", "The digit value must be between 0 and 9.");
        }
    }
    /// <summary>
    /// Removes the last digit of a number. The number must have at least one digit.
    /// </summary>
    /// <param name="numberToConvert">The number to convert.</param>
    /// <exception cref="InvalidOperationException"></exception>
    /// <returns>The converted input parameter.</returns>
    public static Number RemoveLastDigit(Number numberToConvert)
    {
        if (numberToConvert.DigitsCount > 0)
        {
            Number resultNumber = new Number(numberToConvert);
            resultNumber.digits.RemoveLast();
            if (resultNumber.floatingDigitsCount > 0)
            {
                resultNumber.floatingDigitsCount--;
            }
            return resultNumber;
        }
        else
        {
            throw new InvalidOperationException("The number has no digits to remove.");
        }
    }

    #endregion

    #region Overloaded Operators

    #region Comparison And Equality Operators
    /// <summary>
    /// Compares two numbers.
    /// </summary>
    /// <param name="leftNumber">The number to the left of the operator.</param>
    /// <param name="rightNumber">The number to the right of the operator.</param>
    /// <returns>True if the left number is less than the right number, false otherwise.</returns>
    public static bool operator <(Number leftNumber, Number rightNumber)
    {
        return (leftNumber.CompareTo(rightNumber) < 0);
    }
    /// <summary>
    /// Compares two numbers.
    /// </summary>
    /// <param name="leftNumber">The number to the left of the operator.</param>
    /// <param name="rightNumber">The number to the right of the operator.</param>
    /// <returns>True if the left number is greater than the right number, false otherwise.</returns>
    public static bool operator >(Number leftNumber, Number rightNumber)
    {
        return (leftNumber.CompareTo(rightNumber) > 0);
    }
    /// <summary>
    /// Compares two numbers.
    /// </summary>
    /// <param name="leftNumber">The number to the left of the operator.</param>
    /// <param name="rightNumber">The number to the right of the operator.</param>
    /// <returns>True if the left number is less than or equal to the right number, false otherwise.</returns>
    public static bool operator <=(Number leftNumber, Number rightNumber)
    {
        return (leftNumber.CompareTo(rightNumber) <= 0);
    }
    /// <summary>
    /// Compares two numbers.
    /// </summary>
    /// <param name="leftNumber">The number to the left of the operator.</param>
    /// <param name="rightNumber">The number to the right of the operator.</param>
    /// <returns>True if the left number is greater than or equal to the right number, false otherwise.</returns>
    public static bool operator >=(Number leftNumber, Number rightNumber)
    {
        return (leftNumber.CompareTo(rightNumber) >= 0);
    }
    /// <summary>
    /// Compares two numbers.
    /// </summary>
    /// <param name="leftNumber">The number to the left of the operator.</param>
    /// <param name="rightNumber">The number to the right of the operator.</param>
    /// <returns>True if the left number is equal to the right number, false otherwise.</returns>
    public static bool operator ==(Number leftNumber, Number rightNumber)
    {
        return (leftNumber.CompareTo(rightNumber) == 0);
    }
    /// <summary>
    /// Compares two numbers.
    /// </summary>
    /// <param name="leftNumber">The number to the left of the operator.</param>
    /// <param name="rightNumber">The number to the right of the operator.</param>
    /// <returns>True if the left number is not equal to the right number, false otherwise.</returns>
    public static bool operator !=(Number leftNumber, Number rightNumber)
    {
        return (leftNumber.CompareTo(rightNumber) != 0);
    }

    #endregion

    #region Arithmetic Operators

    #region Operator +
    /// <summary>
    /// Adds up two numbers.
    /// </summary>
    /// <param name="leftNumberParam">The number to the left of the operator.</param>
    /// <param name="rightNumberParam">The number to the right of the operator.</param>
    /// <returns>The sum of the two parameters.</returns>
    public static Number operator +(Number leftNumberParam, Number rightNumberParam)
    {
        Number leftNumber = new Number(leftNumberParam);
        Number rightNumber = new Number(rightNumberParam);
        leftNumber.Normalize();
        rightNumber.Normalize();

        if (leftNumber < 0 && rightNumber >= 0)
        {
            return -(Number.Abs(leftNumber) - rightNumber);
        }
        else if (leftNumber >= 0 && rightNumber < 0)
        {
            return (leftNumber - Number.Abs(rightNumber));
        }

        LinkedList<int> resultDigits = new LinkedList<int>();
        int resultFloatingDigitsCount;

        LinkedListNode<int> leftCurrentNode = leftNumber.digits.Last;
        LinkedListNode<int> rightCurrentNode = rightNumber.digits.Last;

        if (leftNumber.floatingDigitsCount != rightNumber.floatingDigitsCount)
        {
            int floatPositionDifference = leftNumber.floatingDigitsCount - rightNumber.floatingDigitsCount;
            while (floatPositionDifference < 0)
            {
                resultDigits.AddFirst(rightCurrentNode.Value);
                rightCurrentNode = rightCurrentNode.Previous;
                floatPositionDifference++;
            }
            while (floatPositionDifference > 0)
            {
                resultDigits.AddFirst(leftCurrentNode.Value);
                leftCurrentNode = leftCurrentNode.Previous;
                floatPositionDifference--;
            }
            resultFloatingDigitsCount = System.Math.Max(leftNumber.floatingDigitsCount, rightNumber.floatingDigitsCount);
        }
        else
        {
            resultFloatingDigitsCount = leftNumber.floatingDigitsCount;
        }

        int carry = 0;
        int digitsSum = 0;

        while (rightCurrentNode != null)
        {
            digitsSum = rightCurrentNode.Value + carry;
            if (leftCurrentNode != null)
            {
                digitsSum += leftCurrentNode.Value;
                leftCurrentNode = leftCurrentNode.Previous;
            }
            carry = digitsSum / 10;

            resultDigits.AddFirst(digitsSum % 10);
            rightCurrentNode = rightCurrentNode.Previous;
        }

        while (carry > 0)
        {
            digitsSum = carry;
            if (leftCurrentNode != null)
            {
                digitsSum += leftCurrentNode.Value;
                leftCurrentNode = leftCurrentNode.Previous;
            }

            resultDigits.AddFirst(digitsSum % 10);
            carry = digitsSum / 10;
        }

        while (leftCurrentNode != null)
        {
            resultDigits.AddFirst(leftCurrentNode.Value);
            leftCurrentNode = leftCurrentNode.Previous;
        }

        return (new Number(resultDigits, resultFloatingDigitsCount, leftNumber.isNegative));
    }

    #endregion

    #region Operator -
    /// <summary>
    /// Subtracts one number from another.
    /// </summary>
    /// <param name="leftNumberParam">The number to the left of the operator.</param>
    /// <param name="rightNumberParam">The number to the right of the operator.</param>
    /// <returns>The difference of the two parameters.</returns>
    public static Number operator -(Number leftNumberParam, Number rightNumberParam)
    {
        Number leftNumber = new Number(leftNumberParam);
        Number rightNumber = new Number(rightNumberParam);
        leftNumber.Normalize();
        rightNumber.Normalize();

        if (leftNumber >= 0 && rightNumber < 0)
        {
            return leftNumber + Number.Abs(rightNumber);
        }
        else if (leftNumber < 0 && rightNumber >= 0)
        {
            return leftNumber + (-rightNumber);
        }
        else if (leftNumber < 0 && rightNumber < 0)
        {
            return -(Number.Abs(leftNumber) - Number.Abs(rightNumber));
        }
        else if (leftNumber < rightNumber)
        {
            return -(rightNumber - leftNumber);
        }

        LinkedList<int> resultDigits = new LinkedList<int>();
        int resultFloatingDigitsCount = 0;

        LinkedListNode<int> leftCurrentNode = leftNumber.digits.Last;
        LinkedListNode<int> rightCurrentNode = rightNumber.digits.Last;

        int floatPositionDifference = 0;
        if (leftNumber.floatingDigitsCount != rightNumber.floatingDigitsCount)
        {
            floatPositionDifference = leftNumber.floatingDigitsCount - rightNumber.floatingDigitsCount;
            while (floatPositionDifference > 0)
            {
                resultDigits.AddFirst(leftCurrentNode.Value);
                leftCurrentNode = leftCurrentNode.Previous;
                floatPositionDifference--;
            }
            resultFloatingDigitsCount = System.Math.Max(leftNumber.floatingDigitsCount, rightNumber.floatingDigitsCount);
        }
        else
        {
            resultFloatingDigitsCount = leftNumber.floatingDigitsCount;
        }

        bool borrow = false;
        int digitsDiff = 0;

        while (rightCurrentNode != null)
        {
            if (floatPositionDifference < 0)
            {
                digitsDiff = 0 - rightCurrentNode.Value;
                floatPositionDifference++;
            }
            else
            {
                digitsDiff = leftCurrentNode.Value - rightCurrentNode.Value;
                leftCurrentNode = leftCurrentNode.Previous;
            }

            if (borrow == true)
            {
                digitsDiff--;
            }

            if (digitsDiff < 0)
            {
                resultDigits.AddFirst(digitsDiff + 10);
                borrow = true;
            }
            else
            {
                resultDigits.AddFirst(digitsDiff);
                borrow = false;
            }

            rightCurrentNode = rightCurrentNode.Previous;
        }

        while (borrow == true)
        {
            if (leftCurrentNode.Value == 0)
            {
                resultDigits.AddFirst(9);
            }
            else
            {
                resultDigits.AddFirst(leftCurrentNode.Value - 1);
                borrow = false;
            }
            leftCurrentNode = leftCurrentNode.Previous;
        }

        while (leftCurrentNode != null)
        {
            resultDigits.AddFirst(leftCurrentNode.Value);
            leftCurrentNode = leftCurrentNode.Previous;
        }

        return (new Number(resultDigits, resultFloatingDigitsCount, false));
    }

    #endregion

    #region Operator *
    /// <summary>
    /// Multiplies two numbers.
    /// </summary>
    /// <param name="leftNumberParam">The number to the left of the operator.</param>
    /// <param name="rightNumberParam">The number to the right of the operator.</param>
    /// <returns>The product of the two parameters.</returns>
    public static Number operator *(Number leftNumberParam, Number rightNumberParam)
    {
        Number leftNumber = new Number(leftNumberParam);
        Number rightNumber = new Number(rightNumberParam);
        leftNumber.Normalize();
        rightNumber.Normalize();

        if (leftNumber == 0 || rightNumber == 0)
        {
            return 0;
        }

        if (leftNumber.DigitsCount > rightNumber.DigitsCount)
        {
            return rightNumber * leftNumber;
        }

        LinkedList<int> resultDigits = new LinkedList<int>();

        LinkedListNode<int> leftCurrentNode = leftNumber.digits.Last;
        LinkedListNode<int> rightCurrentNode;

        LinkedList<int> tempDigits = new LinkedList<int>();
        Number bigCarry = 0;

        while (leftCurrentNode != null)
        {
            tempDigits.Clear();
            rightCurrentNode = rightNumber.digits.Last;
            int digitsProduct = 0;
            int carry = 0;

            while (rightCurrentNode != null)
            {
                digitsProduct = leftCurrentNode.Value * rightCurrentNode.Value + carry;
                tempDigits.AddFirst(digitsProduct % 10);
                carry = digitsProduct / 10;

                rightCurrentNode = rightCurrentNode.Previous;
            }

            if (carry > 0)
            {
                tempDigits.AddFirst(carry);
            }

            Number tempResult = new Number(tempDigits, 0, false);
            tempResult += bigCarry;

            if (tempResult.DigitsCount > 1)
            {
                bigCarry.SetValue(tempResult);
                bigCarry.RemoveLastDigit();
            }
            else
            {
                bigCarry.SetValue(0);
            }

            resultDigits.AddFirst(tempResult.LastDigit);
            leftCurrentNode = leftCurrentNode.Previous;
        }

        if (bigCarry != 0)
        {
            LinkedListNode<int> bigCarryCurrentNode = bigCarry.digits.Last;
            while (bigCarryCurrentNode != null)
            {
                resultDigits.AddFirst(bigCarryCurrentNode.Value);
                bigCarryCurrentNode = bigCarryCurrentNode.Previous;
            }
        }

        int resultFloatingDigitsCount = leftNumber.floatingDigitsCount + rightNumber.floatingDigitsCount;

        while (resultDigits.Count < resultFloatingDigitsCount + 1)
        {
            resultDigits.AddFirst(0);
        }

        return (new Number(resultDigits,
                           resultFloatingDigitsCount,
                           (leftNumber < 0) != (rightNumber < 0)));
    }

    #endregion

    #region Operator /
    /// <summary>
    /// Divides two numbers.
    /// </summary>
    /// <param name="leftNumberParam">The number to the left of the operator.</param>
    /// <param name="rightNumberParam">The number to the right of the operator.</param>
    /// <exception cref="DivideByZeroException"></exception>
    /// <returns>The quotient of the two parameters.</returns>
    public static Number operator /(Number leftNumberParam, Number rightNumberParam)
    {
        Number leftNumber = new Number(leftNumberParam);
        Number rightNumber = new Number(rightNumberParam);
        leftNumber.Normalize();
        rightNumber.Normalize();

        if (rightNumber == 0)
        {
            throw new DivideByZeroException("Can't divide by zero.");
        }
        else if (leftNumber == 0)
        {
            return 0;
        }

        bool leftIsNegative = (leftNumber < 0);
        bool rightIsNegative = (rightNumber < 0);

        leftNumber = Number.Abs(leftNumber);
        rightNumber = Number.Abs(rightNumber);

        Number resultNumber = CalculateIntegerQuotient(leftNumber, rightNumber);
        Number remainder = null;

        if (resultNumber == 0)
        {
            remainder = new Number(leftNumber);
        }
        else
        {
            remainder = leftNumber - resultNumber * rightNumber;
        }

        if (remainder != 0)
        {
            LinkedList<int> floatingPart = CalculateFloatingQuotient(remainder, rightNumber);
            foreach (int digit in floatingPart)
            {
                resultNumber.AppendLastDigit(digit, true);
            }
        }

        resultNumber.isNegative = (leftIsNegative != rightIsNegative);
        resultNumber.Normalize();
        return resultNumber;
    }

    #endregion

    #region Operator ++
    /// <summary>
    /// Adds 1 to a number.
    /// </summary>
    /// <param name="number">The addend.</param>
    /// <returns>The input parameter plus 1.</returns>
    public static Number operator ++(Number number)
    {
        return (number + 1);
    }

    #endregion

    #region Operator --
    /// <summary>
    /// Subtracts 1 from a number.
    /// </summary>
    /// <param name="number">The minuend.</param>
    /// <returns>The input parameter minus 1.</returns>
    public static Number operator --(Number number)
    {
        return (number - 1);
    }

    #endregion

    #region Operator unary -
    /// <summary>
    /// Negates a number.
    /// </summary>
    /// <param name="numberToNegate">The number to negate.</param>
    /// <returns>The negative value of the number.</returns>
    public static Number operator -(Number numberToNegate)
    {
        Number resultNumber = new Number(numberToNegate);
        if (resultNumber != 0)
        {
            resultNumber.isNegative = !resultNumber.isNegative;
        }
        return resultNumber;
    }

    #endregion

    #region Operator <<
    /// <summary>
    /// Performs a decimal shift to the left.
    /// </summary>
    /// <param name="number">The number to shift.</param>
    /// <param name="value">The number of shifts to perform.</param>
    /// <returns>The left-shifted value of the input parameter.</returns>
    public static Number operator <<(Number number, int value)
    {
        Number resultNumber = new Number(number);
        while (value > 0)
        {
            resultNumber.DecimalLeftShift();
            value--;
        }
        return resultNumber;
    }

    #endregion

    #region Operator >>
    /// <summary>
    /// Performs a decimal shift to the right.
    /// </summary>
    /// <param name="number">The number to shift.</param>
    /// <param name="value">The number of shifts to perform.</param>
    /// <returns>The right-shifted value of the input parameter.</returns>
    public static Number operator >>(Number number, int value)
    {
        Number resultNumber = new Number(number);
        while (value > 0)
        {
            resultNumber.DecimalRightShift();
            value--;
        }
        return resultNumber;
    }

    #endregion

    #endregion

    #region Type Conversion Operators
    /// <summary>
    /// Converts a decimal value to a value of type Number.
    /// </summary>
    /// <param name="numberToConvert">The decimal number to convert.</param>
    /// <returns>The converted value of the input parameter.</returns>
    public static implicit operator Number(decimal numberToConvert)
    {
        return new Number(numberToConvert);
    }

    /// <summary>
    /// Converts a double value to a value of type Number.
    /// </summary>
    /// <param name="numberToConvert">The double number to convert.</param>
    /// <returns>The converted value of the input parameter.</returns>
    public static implicit operator Number(double numberToConvert)
    {
        return new Number((decimal)numberToConvert);
    }

    /// <summary>
    /// Converts a long value to a value of type Number.
    /// </summary>
    /// <param name="numberToConvert">The long number to convert.</param>
    /// <returns>The converted value of the input parameter.</returns>
    public static implicit operator Number(long numberToConvert)
    {
        return new Number((decimal)numberToConvert);
    }

    /// <summary>
    /// Converts an integer value to a value of type Number.
    /// </summary>
    /// <param name="numberToConvert">The integer number to convert.</param>
    /// <returns>The converted value of the input parameter.</returns>
    public static implicit operator Number(int numberToConvert)
    {
        return new Number((decimal)numberToConvert);
    }

    public static implicit operator Number(string v)
    {
        throw new NotImplementedException();
    }

    #endregion

    #endregion

    #region Private Methods

    #region SetValue methods

    private void SetValue(decimal value)
    {
        digits.Clear();

        if (value == 0)
        {
            AppendLastDigit(0);
            this.floatingDigitsCount = 0;
            this.isNegative = false;
            return;
        }
        else if (value < 0)
        {
            this.isNegative = true;
            value = System.Math.Abs(value);
        }
        else
        {
            this.isNegative = false;
        }

        decimal integerPart = Decimal.Truncate(value);
        decimal fractionalPart = value - integerPart;

        if (fractionalPart != 0)
        {
            int currentDigit = 0;
            do
            {
                fractionalPart *= 10;
                currentDigit = (int)fractionalPart;
                AppendLastDigit(currentDigit, true);

                if (floatingDigitsCount > MAX_FLOATING_DIGITS)
                {
                    RoundLastDigit();
                    break;
                }
            }
            while ((fractionalPart -= currentDigit) != 0);

            if (integerPart == 0)
            {
                AppendFirstDigit(0);
            }
        }

        if (integerPart != 0)
        {
            int currentDigit = 0;
            do
            {
                currentDigit = (int)(integerPart % 10);
                AppendFirstDigit(currentDigit);
            }
            while ((integerPart /= 10) >= 1);
        }

        Normalize();
    }

    private void SetValue(Number value)
    {
        this.digits.Clear();

        foreach (int digit in value.digits)
        {
            this.AppendLastDigit(digit);
        }

        this.floatingDigitsCount = value.floatingDigitsCount;
        this.isNegative = value.isNegative;
    }

    private void SetValue(string value)
    {
        this.digits.Clear();

        int currentCharIndex = 0;
        int integerDigitsCount = 0;

        if (value[0] == '-')
        {
            this.isNegative = true;
            currentCharIndex = 1;
        }
        else
        {
            this.isNegative = false;
        }

        while (currentCharIndex < value.Length)
        {
            if (value[currentCharIndex] == '.')
            {
                if (integerDigitsCount == 0 &&
                    currentCharIndex > (value[0] == '-' ? 1 : 0) &&
                    currentCharIndex < value.Length - 1)
                {
                    integerDigitsCount = currentCharIndex;
                    if (value[0] == '-')
                    {
                        integerDigitsCount--;
                    }
                }
                else
                {
                    this.SetValue(0);
                    throw new ArgumentException(
                        "Invalid floating point placement or more than one floating points.", "value");
                }
            }
            else if (Char.IsDigit(value, currentCharIndex) == false)
            {
                this.SetValue(0);
                throw new ArgumentException(
                    String.Format("Unknown symbol: {0}", value[currentCharIndex]),
                    String.Format("value[{0}]", currentCharIndex));
            }
            else
            {
                this.AppendLastDigit((int)Char.GetNumericValue(value[currentCharIndex]));
            }
            currentCharIndex++;
        }

        if (integerDigitsCount != 0)
        {
            this.floatingDigitsCount = this.DigitsCount - integerDigitsCount;
        }
    }

    private void SetValue(LinkedList<int> value, int floatingDigitsCount, bool isNegative)
    {
        if (floatingDigitsCount < 0 || floatingDigitsCount >= value.Count)
        {
            this.SetValue(0);
            throw new ArgumentOutOfRangeException("floatingDigitsCount", "Invalid float position.");
        }

        this.digits.Clear();

        foreach (int digit in value)
        {
            if (digit >= 0 && digit <= 9)
            {
                this.AppendLastDigit(digit);
            }
            else
            {
                this.SetValue(0);
                throw new ArgumentOutOfRangeException("value", "The digits in the list must be between 0 and 9.");
            }
        }

        this.floatingDigitsCount = floatingDigitsCount;
        this.isNegative = isNegative;
        this.Normalize();
    }

    #endregion

    #region CalculateQuotient methods

    private static Number CalculateIntegerQuotient(Number leftNumber, Number rightNumber)
    {
        Number resultNumber = null;
        Stack<Number> resultsStack = new Stack<Number>();

        while (true)
        {
            if (leftNumber < rightNumber)
            {
                resultNumber = new Number(0);
            }
            else
            {
                LinkedListNode<int> leftCurrentNode = leftNumber.digits.First;
                LinkedListNode<int> rightCurrentNode = rightNumber.digits.First;

                while (leftCurrentNode.Value == 0)
                {
                    leftCurrentNode = leftCurrentNode.Next;
                    rightCurrentNode = rightCurrentNode.Next;
                }

                Number currentDividend = new Number(leftCurrentNode.Value);

                int currentDividendInitialLenght =
                    leftNumber.IntegerDigitsCount - rightNumber.IntegerDigitsCount;
                while (currentDividendInitialLenght > 0)
                {
                    leftCurrentNode = leftCurrentNode.Next;
                    currentDividend.AppendLastDigit(leftCurrentNode.Value);
                    currentDividendInitialLenght--;
                }

                while (rightCurrentNode.Value == 0)
                {
                    rightCurrentNode = rightCurrentNode.Next;
                    if (leftCurrentNode.Next == null)
                    {
                        currentDividend.AppendLastDigit(0);
                    }
                    else
                    {
                        leftCurrentNode = leftCurrentNode.Next;
                        currentDividend.AppendLastDigit(leftCurrentNode.Value);
                    }
                }

                resultNumber = DivideIntegerByDigit(currentDividend, rightCurrentNode.Value);
            }

            Number checkNumber = resultNumber * rightNumber - leftNumber;

            if (checkNumber < 0)
            {
                if (resultsStack.Count % 2 != 0)
                {
                    resultNumber++;
                }
                break;
            }
            else if ((resultsStack.Count % 2 == 0 && checkNumber == 0) ||
                     (resultsStack.Count % 2 != 0 && checkNumber < rightNumber))
            {
                break;
            }
            else
            {
                resultsStack.Push(resultNumber);
                leftNumber = checkNumber;
            }
        }

        while (resultsStack.Count > 0)
        {
            Number tempResult = resultsStack.Pop();
            resultNumber = tempResult - resultNumber;
        }

        return resultNumber;
    }

    private static LinkedList<int> CalculateFloatingQuotient(Number remainder, Number rightNumber)
    {
        LinkedList<int> resultDigits = new LinkedList<int>();

        while (remainder != 0)
        {
            bool extraZero = false;
            while (remainder < rightNumber)
            {
                remainder >>= 1;
                if (extraZero == true)
                {
                    resultDigits.AddLast(0);
                }
                else
                {
                    extraZero = true;
                }
            }

            Number innerResultNumber = CalculateIntegerQuotient(remainder, rightNumber);

            resultDigits.AddLast(innerResultNumber.LastDigit);

            if (resultDigits.Count > MAX_FLOATING_DIGITS)
            {
                break;
            }

            remainder -= innerResultNumber * rightNumber;
        }

        return resultDigits;
    }

    #endregion

    #region Append/Remove digits methods

    private void AppendFirstDigit(int newDigitValue)
    {
        if (newDigitValue >= 0 && newDigitValue <= 9)
        {
            digits.AddFirst(newDigitValue);
        }
        else
        {
            throw new ArgumentOutOfRangeException("newDigitValue", "The digit value must be between 0 and 9.");
        }
    }

    private void RemoveFirstDigit()
    {
        if (DigitsCount > 0)
        {
            digits.RemoveFirst();
        }
        else
        {
            throw new InvalidOperationException("The number has no digits to remove.");
        }
    }

    private void AppendLastDigit(int newDigitValue)
    {
        AppendLastDigit(newDigitValue, false);
    }

    private void AppendLastDigit(int newDigitValue, bool setFloat)
    {
        if (newDigitValue >= 0 && newDigitValue <= 9)
        {
            digits.AddLast(newDigitValue);

            if (floatingDigitsCount > 0 || setFloat == true)
            {
                floatingDigitsCount++;
            }
        }
        else
        {
            throw new ArgumentOutOfRangeException("newDigitValue", "The digit value must be between 0 and 9.");
        }
    }

    private void RemoveLastDigit()
    {
        if (DigitsCount > 0)
        {
            digits.RemoveLast();
            if (floatingDigitsCount > 0)
            {
                floatingDigitsCount--;
            }
        }
        else
        {
            throw new InvalidOperationException("The number has no digits to remove.");
        }
    }

    #endregion

    #region Helper methods

    private static Number DivideIntegerByDigit(Number dividendNumber, int dividerDigit)
    {
        if (dividendNumber.floatingDigitsCount != 0)
        {
            throw new ArgumentException("The dividend must be integer.", "dividendNumber");
        }
        else if (dividerDigit < 1 || dividerDigit > 9)
        {
            throw new ArgumentOutOfRangeException("dividerDigit", "Must be between 1 and 9.");
        }

        if (dividerDigit == 1)
        {
            return new Number(dividendNumber);
        }

        LinkedListNode<int> dividendCurrentNode = dividendNumber.digits.First;
        int dividendDigit;

        if (dividendCurrentNode.Value < dividerDigit)
        {
            dividendDigit = dividendCurrentNode.Value * 10 + dividendCurrentNode.Next.Value;
            dividendCurrentNode = dividendCurrentNode.Next;
        }
        else
        {
            dividendDigit = dividendCurrentNode.Value;
        }

        LinkedList<int> resultDigits = new LinkedList<int>();

        while (true)
        {
            if (dividendDigit >= dividerDigit)
            {
                resultDigits.AddLast(dividendDigit / dividerDigit);
            }
            else
            {
                resultDigits.AddLast(0);
            }

            dividendCurrentNode = dividendCurrentNode.Next;
            if (dividendCurrentNode == null)
            {
                break;
            }

            dividendDigit = (dividendDigit % dividerDigit) * 10 + dividendCurrentNode.Value;
        }

        return new Number(resultDigits, 0, false);
    }

    private void RoundLastDigit()
    {
        int lastDigit = LastDigit;
        RemoveLastDigit();
        RoundLastDigit(lastDigit);
    }

    private void RoundLastDigit(int lastDigit)
    {
        if (lastDigit > 4)
        {
            LinkedListNode<int> currentNode = digits.Last;
            while (++currentNode.Value == 10)
            {
                currentNode.Value = 0;
                currentNode = currentNode.Previous;

                if (currentNode == null)
                {
                    AppendFirstDigit(1);
                    break;
                }
            }
        }
        Normalize();
    }

    private void DecimalLeftShift()
    {
        floatingDigitsCount++;
        if (floatingDigitsCount == DigitsCount)
        {
            AppendFirstDigit(0);
        }
        Normalize();
    }

    private void DecimalRightShift()
    {
        if (floatingDigitsCount > 0)
        {
            floatingDigitsCount--;
        }
        else if (this != 0)
        {
            AppendLastDigit(0);
        }
        Normalize();
    }

    private void Normalize()
    {
        TrimExtraFloatDigits();
        TrimZeroesFromStart();
        TrimZeroesFromEnd();

        if (DigitsCount == 1 && FirstDigit == 0)
        {
            isNegative = false;
        }
    }

    private void TrimExtraFloatDigits()
    {
        while (floatingDigitsCount > MAX_FLOATING_DIGITS)
        {
            if (floatingDigitsCount == MAX_FLOATING_DIGITS + 1)
            {
                RoundLastDigit();
                break;
            }
            RemoveLastDigit();
        }
    }

    private void TrimZeroesFromStart()
    {
        while (FirstDigit == 0 && floatingDigitsCount < DigitsCount - 1)
        {
            RemoveFirstDigit();
        }
    }

    private void TrimZeroesFromEnd()
    {
        while (LastDigit == 0 && floatingDigitsCount > 0)
        {
            RemoveLastDigit();
        }
    }

    #endregion

    #endregion

    #region IComparable<LargeNumber> Members
    /// <summary>
    /// Compares two numbers.
    /// </summary>
    /// <param name="numberToCompareParam">The number to compare to the current unstance.</param>
    /// <returns>Greater than 0 if the current instance is greater than the input parameter; 0 if the two numbers are equal; Less than zero if the current instance is less than the input parameter.</returns>
    public int CompareTo(Number numberToCompareParam)
    {
        Number thisNumber = new Number(this);
        Number numberToCompare = new Number(numberToCompareParam);
        thisNumber.Normalize();
        numberToCompare.Normalize();

        if (thisNumber.isNegative != numberToCompare.isNegative)
        {
            return (thisNumber.isNegative == true ? -1 : 1);
        }
        else if (thisNumber.isNegative == true && numberToCompare.isNegative == true)
        {
            thisNumber.isNegative = false;
            numberToCompare.isNegative = false;
            return -(thisNumber.CompareTo(numberToCompare));
        }

        if (thisNumber.IntegerDigitsCount != numberToCompare.IntegerDigitsCount)
        {
            return (thisNumber.IntegerDigitsCount > numberToCompare.IntegerDigitsCount ? 1 : -1);
        }

        LinkedListNode<int> thisCurrentNode = thisNumber.digits.First;
        LinkedListNode<int> otherCurrentNode = numberToCompare.digits.First;

        while (thisCurrentNode != null && otherCurrentNode != null)
        {
            if (thisCurrentNode.Value != otherCurrentNode.Value)
            {
                return (thisCurrentNode.Value > otherCurrentNode.Value ? 1 : -1);
            }
            thisCurrentNode = thisCurrentNode.Next;
            otherCurrentNode = otherCurrentNode.Next;
        }

        if (thisCurrentNode == null && otherCurrentNode == null)
        {
            return 0;
        }
        else
        {
            return (thisCurrentNode == null ? -1 : 1);
        }
    }

    #endregion
}