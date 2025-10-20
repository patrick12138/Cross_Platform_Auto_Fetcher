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
            // 每次请求创建新的 HttpClient 实例,避免请求头污染
            using var httpClient = new HttpClient();

            try
            {
                FileLogger.Log($"[NeteaseMusicService] 准备请求榜单 ID: {topId}");

                // 构建payload,确保与Python版本格式完全一致
                // Python: {"id": str(chart_id), "offset": 0, "total": True, "limit": 1000, "n": 1000, "csrf_token": ""}
                var payload = new
                {
                    id = topId.ToString(),  // 明确转换为字符串
                    offset = 0,
                    total = true,
                    limit = 1000,
                    n = 1000,
                    csrf_token = ""
                };

                FileLogger.Log($"[NeteaseMusicService] Payload: {JsonSerializer.Serialize(payload)}");

                // 使用网易云加密
                var encryptedParams = NeteaseCrypto.Weapi(payload);
                FileLogger.Log($"[NeteaseMusicService] 加密完成, params长度: {encryptedParams["params"].Length}, encSecKey长度: {encryptedParams["encSecKey"].Length}");

                // 创建表单内容
                var requestContent = new FormUrlEncodedContent(encryptedParams);

                // 设置关键请求头,与Python保持一致
                // Python的HEADERS只包含3个关键头部
                var request = new HttpRequestMessage(HttpMethod.Post, PlaylistApiUrl)
                {
                    Content = requestContent
                };

                // 精简请求头,只保留关键的3个(与Python一致)
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
                request.Headers.Add("Referer", "https://music.163.com/");
                // Content-Type会由FormUrlEncodedContent自动设置为application/x-www-form-urlencoded

                FileLogger.Log($"[NeteaseMusicService] 发送加密请求到: {PlaylistApiUrl}");
                var response = await httpClient.SendAsync(request);

                var jsonResponse = await response.Content.ReadAsStringAsync();
                FileLogger.Log($"[NeteaseMusicService] 收到响应,状态码: {response.StatusCode}, 响应长度: {jsonResponse.Length}");

                if (!response.IsSuccessStatusCode)
                {
                    FileLogger.Log($"[NeteaseMusicService] HTTP 错误: {response.StatusCode}");
                    FileLogger.Log($"[NeteaseMusicService] 错误响应内容: {jsonResponse}");
                    return new List<Song>();
                }

                // 记录响应的前300个字符用于调试
                var preview = jsonResponse.Length > 300 ? jsonResponse.Substring(0, 300) : jsonResponse;
                FileLogger.Log($"[NeteaseMusicService] 响应预览: {preview}...");

                var playlistResponse = JsonSerializer.Deserialize<NeteasePlaylistResponse>(jsonResponse);

                var songs = new List<Song>();
                if (playlistResponse?.Playlist?.Tracks != null && playlistResponse.Playlist.Tracks.Count > 0)
                {
                    FileLogger.Log($"[NeteaseMusicService] 成功解析,获取到 {playlistResponse.Playlist.Tracks.Count} 首歌曲");
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
                    FileLogger.Log($"[NeteaseMusicService] 实际返回 {songs.Count} 首歌曲(limit={limit})");
                }
                else
                {
                    FileLogger.Log($"[NeteaseMusicService] 警告: 响应中未找到歌曲数据");
                    FileLogger.Log($"[NeteaseMusicService] playlistResponse != null: {playlistResponse != null}");
                    FileLogger.Log($"[NeteaseMusicService] Playlist != null: {playlistResponse?.Playlist != null}");
                    FileLogger.Log($"[NeteaseMusicService] Tracks != null: {playlistResponse?.Playlist?.Tracks != null}");
                    if (playlistResponse?.Playlist?.Tracks != null)
                    {
                        FileLogger.Log($"[NeteaseMusicService] Tracks.Count: {playlistResponse.Playlist.Tracks.Count}");
                    }
                }

                return songs;
            }
            catch (Exception ex)
            {
                FileLogger.Log($"[NeteaseMusicService] 异常: {ex.GetType().Name} - {ex.Message}");
                FileLogger.Log($"[NeteaseMusicService] 堆栈: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    FileLogger.Log($"[NeteaseMusicService] 内部异常: {ex.InnerException.Message}");
                }
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
