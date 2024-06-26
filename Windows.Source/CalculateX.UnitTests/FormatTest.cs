﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CalculateX.UnitTests;

[TestClass]
public class FormatTest
{
	[ClassInitialize]
	public static void ClassSetup(TestContext _)
	{
	}

	[ClassCleanup]
	public static void ClassTeardown()
	{
	}


	[TestInitialize]
	public void TestSetup()
	{
	}

	[TestCleanup]
	public void TestTeardown()
	{
	}


	[DataTestMethod]
	[DataRow("0",							0)]
	[DataRow("0",							0.0)]
	[DataRow("4",							4)]
	[DataRow("4",							4.0)]
	[DataRow("4.53",						4.53)]
	[DataRow("104.53",					104.53)]
	[DataRow("2,134",						2134)]
	[DataRow("2,345.53",					2345.53)]
	[DataRow("2,345.123456789",		2345.123456789)]
	[DataRow("0.1234",					0.1234)]
	[DataRow("0.123456789",				0.123456789)]
	[DataRow("0.989669363564753",		0.989669363564753)]
	[DataRow("5.989669363564753",		5.989669363564753)]
	[DataRow("3,456,789.123456789",	3456789.123456789)]
	[DataRow("989,669,363,564,753",	989669363564753)]
	public void TestFormatNumberWithGroupingSeparators(string expected, double input)
	{
		Assert.AreEqual("$", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol);
		Assert.AreEqual(",", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator);
		Assert.AreEqual(".", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);

		Assert.AreEqual(expected, Shared.Numbers.FormatNumberWithGroupingSeparators(input));
	}


	[DataTestMethod]
	[DataRow("",						"")]
	[DataRow("  ",						"  ")]
	[DataRow("cat",					"cat")]
	[DataRow("0",						"0")]
	[DataRow("0cat",					"0cat")]
	[DataRow("1234",					"1,234")]
	[DataRow("1234.56789",			"1,234.56789")]
	[DataRow("1234567",				"1,234,567")]
	[DataRow("1234567.8901",		"1,234,567.8901")]
	[DataRow("1234567.8901",		"1234,,567.8901")]
	[DataRow("1234",					"$1,234")]
	[DataRow("1234.56789",			"$1,234.56789")]
	[DataRow("1234567",				"$1,234,567")]
	[DataRow("1234567.8901",		"$1,234,567.8901")]
	[DataRow("1234567.8901",		"$1234,,567.8901")]
	[DataRow("1234567.89",			"$1,234,567.89")]
	[DataRow("1234567.8901234",	"$1,234,567.8901234")]
	public void TestRemoveCurrencySymbolAndGroupingSeparators(string expected, string input)
	{
		Assert.AreEqual("$", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol);
		Assert.AreEqual(",", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator);
		Assert.AreEqual(".", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);

		Assert.AreEqual(expected, Shared.Numbers.RemoveCurrencySymbolAndGroupingSeparators(input));
	}
}
