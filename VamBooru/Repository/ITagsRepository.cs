using System.Threading.Tasks;
using VamBooru.Models;

namespace VamBooru.Repository
{
	public interface ITagsRepository
	{
		Task<Tag[]> SearchTags(string q);
		Task<Tag[]> LoadTopTags(int max);
	}
}