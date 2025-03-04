// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY
namespace ProjectMakoto.Plugins.ScoreSaber.Entities;
#pragma warning disable CS8981
#pragma warning disable CS8618
#pragma warning disable IDE1006
public class Translations : ITranslations
{
    public Dictionary<string, int> Progress = new();
    public CommandTranslation[] CommandList { get; set; }
    #region AutoGenerated
    public commands Commands;
    public sealed class commands
    {
        public scoreSaber ScoreSaber;
        public sealed class scoreSaber
        {
            public unlink Unlink;
            public sealed class unlink
            {
                public SingleTranslationKey NoLink;
                public SingleTranslationKey Unlinked;
            }
            public search Search;
            public sealed class search
            {
                public SingleTranslationKey NoSearchResult;
                public SingleTranslationKey FoundCount;
                public SingleTranslationKey SelectPlayer;
                public SingleTranslationKey Searching;
                public SingleTranslationKey SelectedCountry;
                public SingleTranslationKey SelectCountry;
                public SingleTranslationKey SelectContinent;
                public SingleTranslationKey NextStep;
                public SingleTranslationKey StartSearch;
                public SingleTranslationKey SelectCountryDropdown;
                public SingleTranslationKey SelectContinentDropdown;
                public SingleTranslationKey NoCountryFilter;
            }
            public profile Profile;
            public sealed class profile
            {
                public SingleTranslationKey InvalidId;
                public SingleTranslationKey UserDoesNotExist;
                public SingleTranslationKey Placement;
                public SingleTranslationKey GraphToday;
                public SingleTranslationKey GraphDays;
                public SingleTranslationKey ReplaysWatched;
                public SingleTranslationKey TotalScore;
                public SingleTranslationKey TotalPlayCount;
                public SingleTranslationKey AverageRankedAccuracy;
                public SingleTranslationKey TotalRankedScore;
                public SingleTranslationKey RankedPlayCount;
                public SingleTranslationKey MapLeaderboard;
                public SingleTranslationKey RecentScores;
                public SingleTranslationKey TopScores;
                public SingleTranslationKey InactiveUser;
                public MultiTranslationKey LinkSuccessful;
                public SingleTranslationKey OpenInBrowser;
                public SingleTranslationKey LinkProfileToAccount;
                public SingleTranslationKey ShowRecentScores;
                public SingleTranslationKey ShowTopScores;
                public SingleTranslationKey ShowProfile;
                public SingleTranslationKey LoadingPlayer;
                public SingleTranslationKey NoProfile;
                public SingleTranslationKey NoUser;
                public SingleTranslationKey InvalidInput;
            }
            public mapLeaderboard MapLeaderboard;
            public sealed class mapLeaderboard
            {
                public SingleTranslationKey Profile;
                public SingleTranslationKey PageNotExist;
                public SingleTranslationKey ScoreboardNotExist;
                public SingleTranslationKey LoadingScoreboard;
            }
            public SingleTranslationKey ForbiddenError;
            public SingleTranslationKey InternalServerError;
        }
    }
    #endregion AutoGenerated
}