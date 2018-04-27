namespace VamBooru.Repository
{
	public enum PostedSince
	{
		Default = Forever,
		Forever = 0,
		LastDay = 1,
		LastWeek = 7,
		LastMonth = 30,
		LastYear = 365,
	}
}