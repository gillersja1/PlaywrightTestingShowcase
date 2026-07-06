namespace CalculatorLib;

/// <summary>
/// A small, deliberately simple class used as the "system under test"
/// for demonstrating unit testing patterns in this repo.
/// </summary>
public class Calculator
{
    public double Add(double a, double b) => a + b;

    public double Subtract(double a, double b) => a - b;

    public double Multiply(double a, double b) => a * b;

    public double Divide(double a, double b)
    {
        if (b == 0)
        {
            throw new DivideByZeroException("Cannot divide by zero.");
        }

        return a / b;
    }

    public bool IsPrime(int number)
    {
        if (number < 2)
        {
            return false;
        }

        for (var i = 2; i <= Math.Sqrt(number); i++)
        {
            if (number % i == 0)
            {
                return false;
            }
        }

        return true;
    }
}
