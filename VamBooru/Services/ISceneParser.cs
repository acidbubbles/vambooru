namespace VamBooru.Services
{
	public interface ISceneParser
	{
		string[] GetTags(byte[] projectStream);
	}
}
