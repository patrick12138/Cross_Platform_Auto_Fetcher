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
            // 每次请求创建新的 HttpClient 实例，避免请求头污染
            using var httpClient = new HttpClient();

            try
            {
                // 设置完整的请求头，模拟真实浏览器
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                httpClient.DefaultRequestHeaders.Add("Referer", "https://music.163.com/");
                httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
                httpClient.DefaultRequestHeaders.Add("Accept-Language", "zh-CN,zh;q=0.9,en;q=0.8");
                httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
                httpClient.DefaultRequestHeaders.Add("Origin", "https://music.163.com");
                httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
                httpClient.DefaultRequestHeaders.Add("Cookie", "_ntes_nnid=7eced200af0c791006963adcc3676f521513154142; _ntes_nuid=7eced200af0c791006963adcc3676f52");

                FileLogger.Log($"[NeteaseMusicService] 准备请求榜单 ID: {topId}");

                var payload = new { id = topId, offset = 0, total = true, limit = 1000, n = 1000, csrf_token = "" };
                var encryptedParams = NeteaseCrypto.Weapi(payload);
                var requestContent = new FormUrlEncodedContent(encryptedParams);

                FileLogger.Log($"[NeteaseMusicService] 发送加密请求...");
                var response = await httpClient.PostAsync(PlaylistApiUrl, requestContent);

                var jsonResponse = await response.Content.ReadAsStringAsync();
                FileLogger.Log($"[NeteaseMusicService] 收到响应，状态码: {response.StatusCode}, 响应长度: {jsonResponse.Length}");

                if (!response.IsSuccessStatusCode)
                {
                    FileLogger.Log($"[NeteaseMusicService] HTTP 错误: {response.StatusCode}");
                    return new List<Song>();
                }

                // 记录响应的前200个字符用于调试
                var preview = jsonResponse.Length > 200 ? jsonResponse.Substring(0, 200) : jsonResponse;
                FileLogger.Log($"[NeteaseMusicService] 响应预览: {preview}...");

                var playlistResponse = JsonSerializer.Deserialize<NeteasePlaylistResponse>(jsonResponse);

                var songs = new List<Song>();
                if (playlistResponse?.Playlist?.Tracks != null && playlistResponse.Playlist.Tracks.Count > 0)
                {
                    FileLogger.Log($"[NeteaseMusicService] 成功解析，获取到 {playlistResponse.Playlist.Tracks.Count} 首歌曲");
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
                    FileLogger.Log($"[NeteaseMusicService] 警告: 响应中未找到歌曲数据 (playlistResponse={playlistResponse != null}, Playlist={playlistResponse?.Playlist != null}, Tracks={playlistResponse?.Playlist?.Tracks != null})");
                }

                return songs;
            }
            catch (Exception ex)
            {
                FileLogger.Log($"[NeteaseMusicService] 异常: {ex.GetType().Name} - {ex.Message}");
                FileLogger.Log($"[NeteaseMusicService] 堆栈: {ex.StackTrace}");
                return new List<Song>();
            }
        }
    }

    // Adjusted helper classes to match the actual JSON structure from playlist/detail
    public class NeteasePlaylistResponse
    {
        [JsonPropertyName("playlist")]
        public NeteasePlaylist Playlist { get; set; }
    }

    public class NeteasePlaylist
    {
        // This is the actual list of songs we need
        [JsonPropertyName("tracks")]
        public List<NeteaseTrackDetail> Tracks { get; set; }
    }

    // This class is no longer needed as we get full track details at once
    // public class NeteaseTrackId
    // {
    //     [JsonPropertyName("id")]
    //     public long Id { get; set; }
    // }

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