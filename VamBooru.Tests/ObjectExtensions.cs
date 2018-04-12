using KellermanSoftware.CompareNetObjects;
using NUnit.Framework;

namespace VamBooru.Tests
{
	public static class ObjectExtensions
	{
		public static void ShouldDeepEqual(this object actual, object expected)
		{
			var logic = new CompareLogic();
			var result = logic.Compare(actual, expected);
			if(!result.AreEqual)
				Assert.Fail(result.DifferencesString);
		}
	}
}
