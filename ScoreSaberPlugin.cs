using ProjectMakoto.Plugins.ScoreSaber.Entities;
using ProjectMakoto.Plugins.ScoreSaber.Util;

namespace ProjectMakoto.Plugins.ScoreSaber;

public class ScoreSaberPlugin : BasePlugin
{
    public override string Name => "ScoreSaber Commands";
    public override string Description => "This plugin adds integration for the ScoreSaber API.";
    public override SemVer Version => new(1, 0, 0);
    public override int[] SupportedPluginApis => [1];
    public override string Author => "Mira";
    public override ulong? AuthorId => 411950662662881290;
    public override string UpdateUrl => "https://github.com/Fortunevale/ProjectMakoto.Plugins.ScoreSaber";
    public override Octokit.Credentials? UpdateUrlCredentials => base.UpdateUrlCredentials;

    public SelfFillingDatabaseDictionary<ScoreSaberUser>? Users { get; set; } = null;

    public static ScoreSaberPlugin? Plugin { get; set; }
    public static ScoreSaberClient? ScoreSaber { get; set; }

    public override ScoreSaberPlugin Initialize()
    {
        ScoreSaberPlugin.Plugin = this;
        ScoreSaberPlugin.ScoreSaber = new();

        this.DatabaseInitialized += (s, e) =>
        {
            this.Users = new SelfFillingDatabaseDictionary<ScoreSaberUser>(this, typeof(ScoreSaberUser), (id) =>
            {
                return new ScoreSaberUser(this, id);
            });
        };

        return this;
    }

    public override Task<IEnumerable<MakotoModule>> RegisterCommands()
    {
        return Task.FromResult<IEnumerable<MakotoModule>>(new List<MakotoModule>
        {
            new("ScoreSaber", [
                    new MakotoCommand("scoresaber", "Interact with the ScoreSaber API.",
                        new MakotoCommand("profile", "Displays you the registered profile of the mentioned user or looks up a profile by a ScoreSaber Id.", typeof(ScoreSaberProfileCommand),
                            new MakotoCommandOverload(typeof(string), "profile", "ScoreSaber Id | @User", false, true)),
                        new MakotoCommand("search", "Search a user on Score Saber by name.", typeof(ScoreSaberSearchCommand),
                            new MakotoCommandOverload(typeof(string), "name", "Search a user on Score Saber by name.")),
                        new MakotoCommand("map-leaderboard", "Display the leaderboard off a specific map.", typeof(ScoreSaberMapLeaderboardCommand),
                            new MakotoCommandOverload(typeof(int), "leaderboardid", "The Leaderboard Id"),
                            new MakotoCommandOverload(typeof(int), "page", "The page", false),
                            new MakotoCommandOverload(typeof(int), "internal_page", "The internal page", false)),
                        new MakotoCommand("unlink", "Allows you to remove the saved ScoreSaber profile from your Discord account.", typeof(ScoreSaberUnlinkCommand)))
                    .WithAliases("ss")
                ])
        });
    }

    public override Task<IEnumerable<Type>?> RegisterTables()
    {
        return Task.FromResult<IEnumerable<Type>?>(new List<Type>
        {
            typeof(ScoreSaberUser),
        });
    }

    public override (string? path, Type? type) LoadTranslations()
    {
        return ("Translations/strings.json", typeof(Entities.Translations));
    }

    public override Task Shutdown()
        => base.Shutdown();
}
