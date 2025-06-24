using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using MusicLyricApp.Core.Service;
using System;
using Microsoft.AspNetCore.Hosting;
using MusicLyricApp.ViewModels;
using MusicLyricApp.Core.Utils;
using MusicLyricApp.Models;
using System.Linq;

namespace MusicLyricApp
{
    public static class WebApiStartup
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static void StartWebApi(string[] args)
        {
            try
            {
                Console.Out.WriteLine("Starting web API...args:" + string.Join(", ", args));
                string? port = args.FirstOrDefault(arg => arg.StartsWith("-p"))?.Substring("-p".Length);
                var builder = WebApplication.CreateBuilder(args);
                var url = "http://0.0.0.0:" + (string.IsNullOrEmpty(port) ? "43210" : port);
                builder.WebHost.UseUrls(url); // Listen on port 8080

                // var storageService = new StorageService();
                // var settingBean = storageService.ReadAppConfig();
                // builder.Services.AddScoped<ISearchService>(provider => new SearchService(settingBean));
                builder.Services.AddScoped<IStorageService>(provider => new StorageService());
                builder.Services.AddScoped<ISearchService>(provider =>
                {
                    var storage = provider.GetRequiredService<IStorageService>();
                    return new SearchService(storage.ReadAppConfig());
                });

                var app = builder.Build();

                app.MapGet("/test", () => "Hello World~~~");
                // 根据歌曲名模糊搜索歌曲列表
                app.MapGet("/listSong", (string? name, string? source, ISearchService searchService, IStorageService storageService) =>
                {
                    Logger.Info("/getSongs endpoint called with name: {Name}, {Source}", name, source);
                    Console.Out.WriteLine("name: " + name + ", source: " + source);
                    if (string.IsNullOrEmpty(name)) //  || string.IsNullOrEmpty(id)
                    {
                        return Results.BadRequest("请传入name, 可选source, 0: 网易云(需后台设置Cookie) 1(默认): QQ音乐.");
                    }

                    try
                    {
                        var config = storageService.ReadAppConfig(); // 默认1 QQ音乐
                        if (source == "0")
                        {
                            config.Param.SearchSource = Models.SearchSourceEnum.NET_EASE_MUSIC;
                        }
                        var seach = new SearchParamViewModel();
                        seach.SearchText = name;
                        var result = searchService.BlurSearch(seach, config);
                        return Results.Ok(result[0].SongVos); 
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Error searching songs: {ErrorMsg}", ex.Message);
                        return Results.BadRequest(ex.Message);
                    }
                });

                // 根据列表接口返回的id获取歌词信息
                app.MapGet("/getLyric", async (string? id, string? source, ISearchService searchService, IStorageService storageService) =>
                {
                    Logger.Info("/getLyric endpoint called with id: {Id}, {Source}", id, source);
                    Console.Out.WriteLine("id: " + id + ", source: " + source);
                    if (string.IsNullOrEmpty(id))
                    {
                        return Results.BadRequest("请传入id (listSong接口返回的displayId).");
                    }

                    try
                    {
                        var config = storageService.ReadAppConfig();
                        // 默认1 QQ音乐
                        if (source == "0")
                        {
                            config.Param.SearchSource = SearchSourceEnum.NET_EASE_MUSIC;
                        }
                        else
                        {
                            config.Param.SearchSource = SearchSourceEnum.QQ_MUSIC;
                        }
                        var seach = new SearchParamViewModel();
                        var idPrefix = GlobalUtils.SearchSourceKeywordDict[config.Param.SearchSource] + "/" +
                        GlobalUtils.SearchTypeKeywordDict[config.Param.SearchSource][SearchTypeEnum.SONG_ID];

                        seach.SearchText = idPrefix + id;
                        Console.Out.WriteLine("SearchText: " + seach.SearchText);
                        searchService.InitSongIds(seach, config);

                        var songIds = seach.SongIds;
                        if (songIds.Count == 0)
                        {
                            return Results.BadRequest("搜索结果为空");
                        }

                        var result = searchService.SearchSongs(seach.SongIds, config);
                        var LampVm = new SignalLampViewModel();
                        LampVm.UpdateLampInfo(result, config);
                        var resultView = new SearchResultViewModel();
                        await searchService.RenderSearchResult(seach, resultView, config, result);
                        return Results.Ok(resultView); // JsonSerializer.Serialize(api)
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Error searching songs: {ErrorMsg}", ex.Message);
                        // return Results.StatusCode(StatusCodes.Status500InternalServerError); // Or a more specific error result
                        return Results.BadRequest(ex.Message); // Or a more specific error result
                    }
                });

                Logger.Info("Web API started successfully: " + url);
                app.Run();

            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Web API failed to start: {ErrorMsg}", ex.Message);
            }
        }
    }
}
