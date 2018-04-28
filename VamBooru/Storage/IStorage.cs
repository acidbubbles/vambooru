using System.IO;
using System.Threading.Tasks;

namespace VamBooru.Storage
{
	public interface IStorage
	{
		Task<string> SaveFileAsync(MemoryStream stream, bool compressed);
		Task<Stream> LoadFileStreamAsync(string urn, bool compressed);
	}
}
