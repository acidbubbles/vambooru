namespace VamBooru.Services
{
	public interface IProjectParser
	{
		string[] GetTagsFromProject(byte[] projectStream);
	}
}