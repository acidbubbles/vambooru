using System;
using System.Threading.Tasks;
using VamBooru.Models;

namespace VamBooru.Repository
{
	public interface IPostFilesRepository
	{
		Task<PostFile[]> LoadPostFilesAsync(Guid postId);
		Task<PostFile> LoadPostFileAsync(Guid postId, string urn);
	}
}