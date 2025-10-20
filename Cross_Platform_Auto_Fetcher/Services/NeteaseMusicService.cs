using Cross_Platform_Auto_Fetcher.Services.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Cross_Platform_Auto_Fetcher.Services.Log;

namespace Cross_Platform_Auto_Fetcher.Services
{
    public class NeteaseMusicService : MusicServiceBase
    {
        private const string PlaylistApiUrl = "https://music.163.com/weapi/v3/playlist/detail";

        public override async Task<List<Song>> GetTopListAsync(string topId, int limit = 100)
        {
            using var httpClient = new HttpClient();

            try
            {
                FileLogger.Log($"[Netease] 请求榜单 ID: {topId}");

                var payload = new
                {
                    id = topId.ToString(),
                    offset = 0,
                    total = true,
                    limit = 1000,
                    n = 1000,
                    csrf_token = ""
                };

                var encryptedParams = NeteaseCrypto.Weapi(payload);
                var requestContent = new FormUrlEncodedContent(encryptedParams);

                var request = new HttpRequestMessage(HttpMethod.Post, PlaylistApiUrl)
                {
                    Content = requestContent
                };

                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
                request.Headers.Add("Referer", "https://music.163.com/");

                var response = await httpClient.SendAsync(request);
                var jsonResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    FileLogger.Log($"[Netease] HTTP错误 {response.StatusCode}: {jsonResponse}");
                    return new List<Song>();
                }

                var playlistResponse = JsonSerializer.Deserialize<NeteasePlaylistResponse>(jsonResponse);

                var songs = new List<Song>();
                if (playlistResponse?.Playlist?.Tracks != null && playlistResponse.Playlist.Tracks.Count > 0)
                {
                    FileLogger.Log($"[Netease] 成功获取 {playlistResponse.Playlist.Tracks.Count} 首歌曲");
                    int rank = 1;
                    foreach (var track in playlistResponse.Playlist.Tracks.Take(limit))
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
                    FileLogger.Log($"[Netease] 警告: 响应中未找到歌曲数据");
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

    public class NeteasePlaylistResponse
    {
        [JsonPropertyName("playlist")]
        public NeteasePlaylist Playlist { get; set; }
    }

    public class NeteasePlaylist
    {
        [JsonPropertyName("tracks")]
        public List<NeteaseTrackDetail> Tracks { get; set; }
    }

    public class NeteaseTrackDetail
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("ar")]
        public List<NeteaseArtist> Artists { get; set; }

        [JsonPropertyName("al")]
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
