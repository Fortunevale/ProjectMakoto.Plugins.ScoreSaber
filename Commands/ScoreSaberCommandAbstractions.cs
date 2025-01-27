// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using System.Globalization;
using ProjectMakoto.Entities.ScoreSaber;
using ProjectMakoto.Enums.ScoreSaber;
using ProjectMakoto.Exceptions;
using ProjectMakoto.Plugins.ScoreSaber;

namespace ProjectMakoto.Commands;

internal static class ScoreSaberCommandAbstractions
{
    internal static async Task SendScoreSaberProfile(SharedCommandContext ctx, BaseCommand cmd, string id = "", bool AddLinkButton = true)
    {
        var CommandKey = ((Plugins.ScoreSaber.Entities.Translations)ScoreSaberPlugin.Plugin!.Translations).Commands.ScoreSaber;

        if (string.IsNullOrWhiteSpace(id))
        {
            if (ScoreSaberPlugin.Plugin!.Users![ctx.User.Id].ScoreSaberId != 0)
            {
                id = ScoreSaberPlugin.Plugin!.Users![ctx.User.Id].ScoreSaberId.ToString();
            }
            else
            {
                ctx.BaseCommand.SendSyntaxError();
                return;
            }
        }

        var embed = new DiscordEmbedBuilder
        {
            Description = cmd.GetString(CommandKey.Profile.LoadingPlayer, true)
        }.AsLoading(ctx, "Score Saber");

        _ = await ctx.BaseCommand.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));

        try
        {
            var player = await ScoreSaberPlugin.ScoreSaber!.GetPlayerById(id);

            CancellationTokenSource cancellationTokenSource = new();

            DiscordButtonComponent ShowProfileButton = new(ButtonStyle.Primary, "getmain", cmd.GetString(CommandKey.Profile.ShowProfile), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👤")));
            DiscordButtonComponent TopScoresButton = new(ButtonStyle.Primary, "gettopscores", cmd.GetString(CommandKey.Profile.ShowTopScores), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🎇")));
            DiscordButtonComponent RecentScoresButton = new(ButtonStyle.Primary, "getrecentscores", cmd.GetString(CommandKey.Profile.ShowRecentScores), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🕒")));

            DiscordButtonComponent LinkButton = new(ButtonStyle.Primary, "thats_me", cmd.GetString(CommandKey.Profile.LinkProfileToAccount), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("↘")));

            DiscordLinkButtonComponent OpenProfileInBrowser = new($"https://scoresaber.com/u/{id}", cmd.GetString(CommandKey.Profile.OpenInBrowser), false);

            List<DiscordComponent> ProfileInteractionRow = new()
            {
                OpenProfileInBrowser,
                TopScoresButton,
                RecentScoresButton
            };

            List<DiscordComponent> RecentScoreInteractionRow = new()
            {
                OpenProfileInBrowser,
                ShowProfileButton,
                TopScoresButton
            };

            List<DiscordComponent> TopScoreInteractionRow = new()
            {
                OpenProfileInBrowser,
                ShowProfileButton,
                RecentScoresButton
            };

            PlayerScores? CachedTopScores = null;
            PlayerScores? CachedRecentScores = null;

            async Task RunInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
            {
                _ = Task.Run(async () =>
                {
                    if (e.Message?.Id == ctx.ResponseMessage.Id && e.User.Id == ctx.User.Id)
                    {
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        if (e.GetCustomId() == "thats_me")
                        {
                            AddLinkButton = false;
                            _ = ShowProfile().Add(ctx.Bot, ctx);
                            ScoreSaberPlugin.Plugin!.Users![ctx.User.Id].ScoreSaberId = Convert.ToUInt64(player.id);

                            var new_msg = await cmd.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                            {
                                Description = cmd.GetString(CommandKey.Profile.LinkSuccessful).Build(true, 
                                    new TVar("ProfileName", player.name),
                                    new TVar("ProfileId", player.id),
                                    new TVar("ProfileCommand", $"{ctx.Prefix}scoresaber profile"),
                                    new TVar("UnlinkCommand", $"{ctx.Prefix}scoresaber unlink"))
                            }.AsSuccess(ctx, "Score Saber")));

                            await Task.Delay(5000);
                        }
                        else if (e.GetCustomId() == "gettopscores")
                        {
                            try
                            {
                                CachedTopScores ??= await ScoreSaberPlugin.ScoreSaber!.GetScoresById(id, ScoreType.Top);

                                _ = ShowScores(CachedTopScores, ScoreType.Top).Add(ctx.Bot, ctx);
                            }
                            catch (InternalServerErrorException)
                            {
                                cancellationTokenSource.Cancel();
                                ctx.Client.ComponentInteractionCreated -= RunInteraction;

                                embed = embed.AsError(ctx, "Score Saber");
                                embed.Description = cmd.GetString(CommandKey.InternalServerError, true);
                                _ = await ctx.BaseCommand.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));

                                return;
                            }
                            catch (ForbiddenException)
                            {
                                cancellationTokenSource.Cancel();
                                ctx.Client.ComponentInteractionCreated -= RunInteraction;

                                embed = embed.AsError(ctx, "Score Saber");
                                embed.Description = cmd.GetString(CommandKey.ForbiddenError, true);
                                _ = await ctx.BaseCommand.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));
                                return;
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                        }
                        else if (e.GetCustomId() == "getrecentscores")
                        {
                            try
                            {
                                CachedRecentScores ??= await ScoreSaberPlugin.ScoreSaber!.GetScoresById(id, ScoreType.Recent);

                                _ = ShowScores(CachedRecentScores, ScoreType.Recent).Add(ctx.Bot, ctx);
                            }
                            catch (InternalServerErrorException)
                            {
                                cancellationTokenSource.Cancel();
                                ctx.Client.ComponentInteractionCreated -= RunInteraction;

                                embed = embed.AsError(ctx, "Score Saber");
                                embed.Description = cmd.GetString(CommandKey.InternalServerError, true);
                                _ = await ctx.BaseCommand.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));
                                return;
                            }
                            catch (ForbiddenException)
                            {
                                cancellationTokenSource.Cancel();
                                ctx.Client.ComponentInteractionCreated -= RunInteraction;

                                embed = embed.AsError(ctx, "Score Saber");
                                embed.Description = cmd.GetString(CommandKey.ForbiddenError, true);
                                _ = await ctx.BaseCommand.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));
                                return;
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                        }
                        else if (e.GetCustomId() == "getmain")
                        {
                            _ = ShowProfile().Add(ctx.Bot, ctx);
                        }

                        cancellationTokenSource.Cancel();
                        cancellationTokenSource = new();

                        try
                        {
                            await Task.Delay(120000, cancellationTokenSource.Token);

                            ctx.Client.ComponentInteractionCreated -= RunInteraction;

                            ctx.BaseCommand.ModifyToTimedOut(true);
                        }
                        catch { }
                    }
                }).Add(ctx.Bot, ctx);
            }

            async Task ShowScores(PlayerScores scores, ScoreType scoreType)
            {
                _ = embed.ClearFields();
                embed.ImageUrl = "";

                if (!player.inactive)
                    embed.Description = $":globe_with_meridians: **#{player.rank}** 󠂪 󠂪 󠂪| 󠂪 󠂪 󠂪:flag_{player.country.ToLower()}: **#{player.countryRank}**\n\n" +
                                    $"{(scoreType == ScoreType.Top ? $"**{cmd.GetString(CommandKey.Profile.TopScores)}**" : $"**{cmd.GetString(CommandKey.Profile.RecentScores)}**")}";
                else
                    embed.Description = $"{cmd.GetString(CommandKey.Profile.InactiveUser, true)}\n\n" +
                                    $"{(scoreType == ScoreType.Top ? $"**{cmd.GetString(CommandKey.Profile.TopScores)}**" : $"**{cmd.GetString(CommandKey.Profile.RecentScores)}**")}";

                foreach (var score in scores.playerScores.Take(5))
                {
                    var page = Math.Ceiling((decimal)score.score.rank / (decimal)12);

                    decimal rank = score.score.rank / 6;
                    var odd = (rank % 2 != 0);

                    _ = embed.AddField(new DiscordEmbedField($"{score.leaderboard.songName.FullSanitize()}{(!string.IsNullOrWhiteSpace(score.leaderboard.songSubName) ? $" {score.leaderboard.songSubName.FullSanitize()}" : "")} - {score.leaderboard.songAuthorName.FullSanitize()} [{score.leaderboard.levelAuthorName.FullSanitize()}]".TruncateWithIndication(256),
                        $":globe_with_meridians: **#{score.score.rank}**  󠂪 󠂪| 󠂪 󠂪 {Formatter.Timestamp(score.score.timeSet, TimestampFormat.RelativeTime)}\n" +
                        $"{(score.leaderboard.ranked ? $"**`{((decimal)((decimal)score.score.modifiedScore / (decimal)score.leaderboard.maxScore) * 100).ToString("N2", CultureInfo.CreateSpecificCulture("en-US"))}%`**󠂪 󠂪 󠂪| 󠂪 󠂪 󠂪**`{(score.score.pp).ToString("N2", CultureInfo.CreateSpecificCulture("en-US"))}pp [{(score.score.pp * score.score.weight).ToString("N2", CultureInfo.CreateSpecificCulture("en-US"))}pp]`**\n" : "\n")}" +
                        $"`{score.score.modifiedScore.ToString("N0", CultureInfo.CreateSpecificCulture("en-US"))}` 󠂪 󠂪| 󠂪 󠂪 **{(score.score.fullCombo ? "✅ `FC`" : $"{false.ToEmote(ctx.Bot)} `{score.score.missedNotes + score.score.badCuts}`")}**\n" +
                        $"{cmd.GetString(CommandKey.Profile.MapLeaderboard)}: `{ctx.Prefix}scoresaber map-leaderboard {score.leaderboard.difficulty.leaderboardId} {page}{(odd ? " 1" : "")}`"));
                }

                DiscordMessageBuilder builder = new();

                if (ScoreSaberPlugin.Plugin!.Users![ctx.User.Id].ScoreSaberId == 0 && AddLinkButton)
                    _ = builder.AddComponents(LinkButton);

                _ = await ctx.BaseCommand.RespondOrEdit(builder.WithEmbed(embed).AddComponents((scoreType == ScoreType.Top ? TopScoreInteractionRow : RecentScoreInteractionRow)));
            }

            var LoadedGraph = "";

            async Task ShowProfile()
            {
                embed = embed.AsInfo(ctx, "Score Saber");

                _ = embed.ClearFields();
                embed.Title = $"{player.name.FullSanitize()} 󠂪 󠂪 󠂪| 󠂪 󠂪 󠂪`{player.pp.ToString("N2", CultureInfo.CreateSpecificCulture("en-US"))}pp`";
                embed.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = player.profilePicture };
                if (!player.inactive)
                    embed.Description = $":globe_with_meridians: **#{player.rank}** 󠂪 󠂪 󠂪| 󠂪 󠂪 󠂪:flag_{player.country.ToLower()}: **#{player.countryRank}**\n";
                else
                    embed.Description = $"{cmd.GetString(CommandKey.Profile.InactiveUser, true)}";
                _ = embed.AddField(new DiscordEmbedField(cmd.GetString(CommandKey.Profile.RankedPlayCount), $"`{player.scoreStats.rankedPlayCount}`", true));
                _ = embed.AddField(new DiscordEmbedField(cmd.GetString(CommandKey.Profile.TotalRankedScore), $"`{player.scoreStats.totalRankedScore.ToString("N0", CultureInfo.GetCultureInfo("en-US"))}`", true));
                _ = embed.AddField(new DiscordEmbedField(cmd.GetString(CommandKey.Profile.AverageRankedAccuracy), $"`{Math.Round(player.scoreStats.averageRankedAccuracy, 2).ToString().Replace(",", ".")}%`", true));
                _ = embed.AddField(new DiscordEmbedField(cmd.GetString(CommandKey.Profile.TotalPlayCount), $"`{player.scoreStats.totalPlayCount}`", true));
                _ = embed.AddField(new DiscordEmbedField(cmd.GetString(CommandKey.Profile.TotalScore), $"`{player.scoreStats.totalScore.ToString("N", CultureInfo.GetCultureInfo("en-US")).Replace(".000", "")}`", true));
                _ = embed.AddField(new DiscordEmbedField(cmd.GetString(CommandKey.Profile.ReplaysWatched), $"`{player.scoreStats.replaysWatched}`", true));

                DiscordMessageBuilder builder = new();

                if (ScoreSaberPlugin.Plugin!.Users![ctx.User.Id].ScoreSaberId == 0 && AddLinkButton)
                    _ = builder.AddComponents(LinkButton);

                if (!string.IsNullOrWhiteSpace(LoadedGraph))
                {
                    embed = embed.AsInfo(ctx, "Score Saber");
                    embed.ImageUrl = LoadedGraph;
                    _ = builder.AddComponents(ProfileInteractionRow);
                }

                _ = await ctx.BaseCommand.RespondOrEdit(builder.WithEmbed(embed));

                var file = $"{Guid.NewGuid()}.png";

                var labels = new List<string>();

                for (var i = 50; i >= 0; i -= 2)
                {
                    if (i == 0)
                    {
                        labels.Add(cmd.GetString(CommandKey.Profile.GraphToday));
                        break;
                    }
                    if (i == 2)
                    {
                        labels.Add(cmd.GetString(CommandKey.Profile.GraphDays, false, new TVar("Count", i)));
                        continue;
                    }

                    labels.Add(cmd.GetString(CommandKey.Profile.GraphDays, false, new TVar("Count", i)));
                    labels.Add("");
                }

                if (string.IsNullOrWhiteSpace(LoadedGraph))
                    try
                    {
                        if (player.inactive)
                            throw new Exception("Player is inactive");

                        var qc = ctx.Bot.ChartsClient.GetChart(1000, 500, labels, new ChartGeneration.Dataset[]
                        {
                            new(cmd.GetString(CommandKey.Profile.Placement), 
                                player.histories.Split(",").Append(player.rank.ToString()), 
                                "getGradientFillHelper('vertical', ['#6b76da', '#a336eb', '#FC0000'])", "yaxis2", true),
                        }, -1, -1);

                        qc.ToFile(file);

                        using var stream = File.Open(file, FileMode.Open);

                        var asset = await (await ctx.Client.GetChannelAsync(ctx.Bot.status.SafeReadOnlyConfig.Channels.GraphAssets)).SendMessageAsync(new DiscordMessageBuilder().WithFile(file, stream));

                        LoadedGraph = asset.Attachments[0].Url;

                        embed = embed.AsInfo(ctx, "Score Saber");
                        embed.ImageUrl = asset.Attachments[0].Url;
                        builder = builder.WithEmbed(embed);
                        _ = builder.AddComponents(ProfileInteractionRow);
                        _ = await ctx.BaseCommand.RespondOrEdit(builder);
                    }
                    catch (Exception)
                    {
                        embed = embed.AsInfo(ctx, "Score Saber");
                        _ = builder.AddComponents(ProfileInteractionRow);
                        _ = await ctx.BaseCommand.RespondOrEdit(builder);
                    }

                try
                {
                    await Task.Delay(1000);
                    File.Delete(file);
                }
                catch { }
            }

            _ = ShowProfile().Add(ctx.Bot, ctx);

            ctx.Client.ComponentInteractionCreated += RunInteraction;

            try
            {
                await Task.Delay(120000, cancellationTokenSource.Token);

                ctx.Client.ComponentInteractionCreated -= RunInteraction;

                ctx.BaseCommand.ModifyToTimedOut(true);
            }
            catch { }
        }
        catch (InternalServerErrorException)
        {
            embed.Description = cmd.GetString(CommandKey.InternalServerError, true);
            _ = await ctx.BaseCommand.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsError(ctx, "Score Saber")));
        }
        catch (ForbiddenException)
        {
            embed.Description = cmd.GetString(CommandKey.ForbiddenError, true);
            _ = await ctx.BaseCommand.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsError(ctx, "Score Saber")));
        }
        catch (NotFoundException)
        {
            embed.Description = cmd.GetString(CommandKey.Profile.InvalidId, true);
            _ = await ctx.BaseCommand.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsError(ctx, "Score Saber")));
        }
        catch (UnprocessableEntityException)
        {
            embed.Description = cmd.GetString(CommandKey.Profile.InvalidId, true);
            _ = await ctx.BaseCommand.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsError(ctx, "Score Saber")));
        }
        catch (Exception)
        {
            throw;
        }
    }
}
