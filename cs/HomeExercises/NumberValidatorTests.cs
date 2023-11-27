﻿using System;
using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;

namespace HomeExercises
{
	[TestFixture]
	public class NumberValidatorTests
	{
		[Test]
		public void Constructor_ShouldThrow_WhenPrecisionIsNotPositive()
		{
			Action act = () => new NumberValidator(0);
			act.Should()
				.Throw<ArgumentException>()
				.WithMessage("precision must be a positive number");
		}

		[Test]
		public void Constructor_ShouldThrow_WhenScaleIsNegative()
		{
			Action act = () => new NumberValidator(3, -1);
			act.Should()
				.Throw<ArgumentException>()
				.WithMessage("precision must be a non-negative number less or equal than precision");
		}

		[Test]
		public void Constructor_ShouldThrow_WhenScaleIsGreaterOrEqualPrecision()
		{
			Action act = () => new NumberValidator(3, 4);
			act.Should()
				.Throw<ArgumentException>()
				.WithMessage("precision must be a non-negative number less or equal than precision");
		}

		[Test]
		public void Constructor_ShouldNotThrow_WhenScaleIsZero()
		{
			Action act = () => new NumberValidator(3, 0);
			act.Should().NotThrow<ArgumentException>();
		}

		[TestCase("0")]
		[TestCase("00.0")]
		[TestCase("+2.0")]
		[TestCase("1111")]
		public void IsValidNumber_ShouldBeTrue_WhenNumberIsValid(string value)
		{
			var validator = new NumberValidator(4, 2, true);

			var result = validator.IsValidNumber(value);

			result.Should().BeTrue();
		}

		[TestCase("")]
		[TestCase("   _")]
		[TestCase("4.sd")]
		[TestCase("-0.0")]
		[TestCase("0.000")]
		[TestCase("+-0")]
		public void IsValidNumber_ShouldBeFalse_WhenNumberIsNotValid(string value)
		{
			var validator = new NumberValidator(4, 2, true);

			var result = validator.IsValidNumber(value);

			result.Should().BeFalse();
		}
	}

	public class NumberValidator
	{
		private readonly Regex numberRegex;
		private readonly bool onlyPositive;
		private readonly int precision;
		private readonly int scale;

		public NumberValidator(int precision, int scale = 0, bool onlyPositive = false)
		{
			this.precision = precision;
			this.scale = scale;
			this.onlyPositive = onlyPositive;
			if (precision <= 0)
				throw new ArgumentException("precision must be a positive number");
			if (scale < 0 || scale >= precision)
				throw new ArgumentException("precision must be a non-negative number less or equal than precision");
			numberRegex = new Regex(@"^([+-]?)(\d+)([.,](\d+))?$", RegexOptions.IgnoreCase);
		}

		public bool IsValidNumber(string value)
		{
			// Проверяем соответствие входного значения формату N(m,k), в соответствии с правилом, 
			// описанным в Формате описи документов, направляемых в налоговый орган в электронном виде по телекоммуникационным каналам связи:
			// Формат числового значения указывается в виде N(m.к), где m – максимальное количество знаков в числе, включая знак (для отрицательного числа), 
			// целую и дробную часть числа без разделяющей десятичной точки, k – максимальное число знаков дробной части числа. 
			// Если число знаков дробной части числа равно 0 (т.е. число целое), то формат числового значения имеет вид N(m).

			if (string.IsNullOrEmpty(value))
				return false;

			var match = numberRegex.Match(value);
			if (!match.Success)
				return false;

			// Знак и целая часть
			var intPart = match.Groups[1].Value.Length + match.Groups[2].Value.Length;
			// Дробная часть
			var fracPart = match.Groups[4].Value.Length;

			if (intPart + fracPart > precision || fracPart > scale)
				return false;

			if (onlyPositive && match.Groups[1].Value == "-")
				return false;
			return true;
		}
	}
}