using Allure.NUnit;
using CalculatorLib;
using NUnit.Framework;

namespace CalculatorLib.UnitTests;

[TestFixture]
[AllureNUnit]
public class CalculatorTests
{
    private Calculator _calculator = null!;

    [SetUp]
    public void SetUp()
    {
        _calculator = new Calculator();
    }

    [TestCase(2, 3, 5)]
    [TestCase(-1, 1, 0)]
    [TestCase(0, 0, 0)]
    [TestCase(-5, -5, -10)]
    public void Add_ReturnsExpectedSum(double a, double b, double expected)
    {
        Assert.That(_calculator.Add(a, b), Is.EqualTo(expected));
    }

    [TestCase(10, 4, 6)]
    [TestCase(0, 5, -5)]
    public void Subtract_ReturnsExpectedDifference(double a, double b, double expected)
    {
        Assert.That(_calculator.Subtract(a, b), Is.EqualTo(expected));
    }

    [TestCase(3, 4, 12)]
    [TestCase(-2, 5, -10)]
    public void Multiply_ReturnsExpectedProduct(double a, double b, double expected)
    {
        Assert.That(_calculator.Multiply(a, b), Is.EqualTo(expected));
    }

    [Test]
    public void Divide_ByNonZero_ReturnsQuotient()
    {
        Assert.That(_calculator.Divide(10, 2), Is.EqualTo(5));
    }

    [Test]
    public void Divide_ByZero_ThrowsDivideByZeroException()
    {
        Assert.Throws<DivideByZeroException>(() => _calculator.Divide(10, 0));
    }

    [TestCase(2, true)]
    [TestCase(4, false)]
    [TestCase(17, true)]
    [TestCase(1, false)]
    [TestCase(0, false)]
    [TestCase(-3, false)]
    public void IsPrime_ReturnsExpectedResult(int number, bool expected)
    {
        Assert.That(_calculator.IsPrime(number), Is.EqualTo(expected));
    }
}
