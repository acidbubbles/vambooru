using System;
using KellermanSoftware.CompareNetObjects;
using NUnit.Framework;

namespace VamBooru.Tests
{
	public static class ObjectExtensions
	{
		public static void ShouldDeepEqual(this object actual, object expected, Action<ComparisonConfig> configure = null)
		{
			var logic = new CompareLogic();
			configure?.Invoke(logic.Config);
			var result = logic.Compare(actual, expected);
			if(!result.AreEqual)
				Assert.Fail(result.DifferencesString);
		}
	}
}
