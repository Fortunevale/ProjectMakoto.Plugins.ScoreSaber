using Newtonsoft.Json;
using ProjectMakoto.Database;
using ProjectMakoto.Enums;

namespace ProjectMakoto.Plugins.ScoreSaber.Entities;

[TableName("users")]
public sealed class ScoreSaberUser : PluginDatabaseTable
{
    public ScoreSaberUser(BasePlugin plugin, ulong identifierValue) : base(plugin, identifierValue)
    {
        this.Id = identifierValue;
    }

    [ColumnName("UserId"), ColumnType(ColumnTypes.BigInt), Primary]
    internal ulong Id { get; init; }

    [ColumnName("ScoreSaberId"), ColumnType(ColumnTypes.BigInt), Default("0")]
    public ulong ScoreSaberId
    {
        get => this.GetValue<ulong>(this.Id, "ScoreSaberId");
        set => this.SetValue(this.Id, "ScoreSaberId", value);
    }
}
