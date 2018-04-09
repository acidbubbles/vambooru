using System.Collections;
using VamBooru.Models;

namespace VamBooru.Tests.TestUtils
{
	public class SceneViewModelComparer : IComparer
	{
		public int Compare(object o1, object o2)
		{
			if (!(o1 is SceneViewModel scene1)) return -1;
			if (!(o2 is SceneViewModel scene2)) return -1;
			return scene1.Title == scene2.Title && scene1.ImageUrl == scene2.ImageUrl ? 0 : 1;
		}
	}
}