namespace VamBooru.VamFormat
{
	public interface ISceneFormat
	{
		object Deserialize(byte[] jsonFile);
		string[] GetTags(dynamic project);
	}
}
