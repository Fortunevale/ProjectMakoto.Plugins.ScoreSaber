// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using ProjectMakoto.Plugins.ScoreSaber;

namespace ProjectMakoto.Commands;

internal sealed class ScoreSaberUnlinkCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var CommandKey = ((Plugins.ScoreSaber.Entities.Translations)ScoreSaberPlugin.Plugin!.Translations).Commands.ScoreSaber;

            if (await ctx.DbUser.Cooldown.WaitForHeavy(ctx))
                return;

            if (ScoreSaberPlugin.Plugin!.Users![ctx.User.Id].ScoreSaberId != 0)
            {
                ScoreSaberPlugin.Plugin!.Users![ctx.User.Id].ScoreSaberId = 0;

                _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = this.GetString(CommandKey.Unlink.Unlinked, true)
                }.AsSuccess(ctx, "Score Saber")));
            }
            else
            {
                _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = this.GetString(CommandKey.Unlink.NoLink, true)
                }.AsError(ctx, "Score Saber")));
            }
        });
    }
}