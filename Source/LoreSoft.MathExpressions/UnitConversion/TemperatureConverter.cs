using LoreSoft.MathExpressions.Metadata;

namespace LoreSoft.MathExpressions.UnitConversion
{
	/// <summary>Units for Temperature</summary>
	public enum TemperatureUnit
	{
		/// <summary>Degrees Celsius unit (c)</summary>
		[Abbreviation("c")]
		Celsius = 0,
		/// <summary>Degrees Fahrenheit unit (f)</summary>
		[Abbreviation("f")]
		Fahrenheit = 1,
		/// <summary>Degrees Kelvin unit (k)</summary>
		[Abbreviation("k")]
		Kelvin = 2,
	}

	/// <summary>
	/// Class representing temperature conversion.
	/// </summary>
	public static class TemperatureConverter
	{
		/// <summary>
		/// Converts the specified from unit to the specified unit.
		/// </summary>
		/// <param name="fromUnit">Covert from unit.</param>
		/// <param name="toUnit">Covert to unit.</param>
		/// <param name="fromValue">Covert from value.</param>
		/// <returns>The converted value.</returns>
		public static double Convert(
			 TemperatureUnit fromUnit,
			 TemperatureUnit toUnit,
			 double fromValue)
		{
			if (fromUnit == toUnit)
			{
				return fromValue;
			}

			if (fromUnit == TemperatureUnit.Celsius)
			{
				if (toUnit == TemperatureUnit.Kelvin)
				{
					return fromValue + 273.15d;
				}
				else if (toUnit == TemperatureUnit.Fahrenheit)
				{
					//(9/5 * C) + 32 = F
					return (9.0d / 5.0d * fromValue) + 32d;
				}
			}
			else if (fromUnit == TemperatureUnit.Kelvin)
			{
				if (toUnit == TemperatureUnit.Celsius)
				{
					return fromValue - 273.15d;
				}
				else if (toUnit == TemperatureUnit.Fahrenheit)
				{
					return 5.0d / 9.0d * ((fromValue - 273.15d) + 32d);
				}
			}
			else if (fromUnit == TemperatureUnit.Fahrenheit)
			{
				if (toUnit == TemperatureUnit.Celsius)
				{
					//(F - 32) * 5/9 = C
					return 5.0d / 9.0d * (fromValue - 32d);
				}
				else if (toUnit == TemperatureUnit.Kelvin)
				{
					return (5.0d / 9.0d * (fromValue - 32d)) + 273.15;
				}
			}
			return 0;
		}
	}
}
