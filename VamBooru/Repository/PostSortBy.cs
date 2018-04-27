using System;

namespace VamBooru.Repository
{
	public enum PostSortBy
	{
		Default = Created,
		[Obsolete] Newest = Created,
		Created = 0,
		[Obsolete] HighestRated = Votes,
		Votes = 1,
		Updated = 2,
	}
}
