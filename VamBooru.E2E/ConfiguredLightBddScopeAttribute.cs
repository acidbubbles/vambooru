using LightBDD.NUnit3;
using VamBooru.E2E;
using VamBooru.E2E.Runtime;

[assembly: ConfiguredLightBddScope]

namespace VamBooru.E2E
{
	public class ConfiguredLightBddScopeAttribute : LightBddScopeAttribute
	{
		private TestRuntimeInitializer _runtime;

		protected override void OnSetUp()
		{
			_runtime = new TestRuntimeInitializer();
			_runtime.Initialize();
		}

		protected override void OnTearDown()
		{
			_runtime.Dispose();
		}
	}
}
