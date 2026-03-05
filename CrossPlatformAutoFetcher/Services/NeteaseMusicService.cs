using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CrossPlatformAutoFetcher.Services.Log;

namespace CrossPlatformAutoFetcher.Services
{
    public class NeteaseMusicService : MusicServiceBase
    {
        // 使用简单的 GET API，无需加密，稳定性更高。
        private const string PlaylistApiUrl = "http://music.163.com/api/playlist/detail";

        public override async Task<List<Song>> GetTopListAsync(string topId, int limit = 100)
        {
            using var httpClient = new HttpClient();

            try
            {
                FileLogger.Log($"[Netease] 请求榜单 ID: {topId} (GET API)");

                var url = $"{PlaylistApiUrl}?id={topId}";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
                request.Headers.Add("Referer", "http://music.163.com/");

                var response = await httpClient.SendAsync(request);
                var jsonResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    FileLogger.Log($"[Netease] HTTP 错误 {response.StatusCode}: {jsonResponse}");
                    return new List<Song>();
                }

                var apiResponse = JsonSerializer.Deserialize<NeteaseApiResponse>(jsonResponse);
                if (apiResponse == null || apiResponse.Code != 200)
                {
                    FileLogger.Log($"[Netease] API 返回错误码: {apiResponse?.Code}");
                    return new List<Song>();
                }

                var songs = new List<Song>();
                if (apiResponse.Result?.Tracks != null && apiResponse.Result.Tracks.Count > 0)
                {
                    FileLogger.Log($"[Netease] 成功获取 {apiResponse.Result.Tracks.Count} 首歌曲");
                    var rank = 1;
                    foreach (var track in apiResponse.Result.Tracks.Take(limit))
                    {
                        if (track?.Name != null && track.Artists != null && track.Album != null)
                        {
                            songs.Add(new Song
                            {
                                Rank = rank++,
                                Title = track.Name,
                                Artist = string.Join(" / ", track.Artists.Select(a => a.Name)),
                                Album = track.Album.Name
                            });
                        }
                    }
                }
                else
                {
                    FileLogger.Log("[Netease] 警告: 响应中未找到歌曲数据");
                }

                return songs;
            }
            catch (Exception ex)
            {
                FileLogger.Log($"[Netease] 异常: {ex.GetType().Name} - {ex.Message}");
                if (ex.InnerException != null)
                {
                    FileLogger.Log($"[Netease] 内部异常: {ex.InnerException.Message}");
                }

                return new List<Song>();
            }
        }
    }

    public class NeteaseApiResponse
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("result")]
        public NeteasePlaylistResult Result { get; set; }
    }

    public class NeteasePlaylistResult
    {
        [JsonPropertyName("tracks")]
        public List<NeteaseTrackDetail> Tracks { get; set; }
    }

    public class NeteaseTrackDetail
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("artists")]
        public List<NeteaseArtist> Artists { get; set; }

        [JsonPropertyName("album")]
        public NeteaseAlbum Album { get; set; }
    }

    public class NeteaseArtist
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    public class NeteaseAlbum
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
