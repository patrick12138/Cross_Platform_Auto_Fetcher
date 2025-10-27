using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;
using Cross_Platform_Auto_Fetcher.Services;

namespace Cross_Platform_Auto_Fetcher
{
    public class QQMusicService : MusicServiceBase
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public override async Task<List<Song>> GetTopListAsync(string topId, int limit = 100)
        {
            if (!int.TryParse(topId, out int topIdInt))
            {
                throw new ArgumentException("topId must be a valid integer for QQ Music.");
            }

            var period = DateTime.Now.ToString("yyyy-MM-dd");

            var requestData = new
            {
                comm = new { cv = 4747474, ct = 24, format = "json", inCharset = "utf-8", outCharset = "utf-8", notice = 0, platform = "yqq.json", needNewCode = 1, uin = 0, g_tk_new_20200303 = 5381, g_tk = 5381 },
                detail = new
                {
                    module = "musicToplist.ToplistInfoServer",
                    method = "GetDetail",
                    param = new { topId = topIdInt, offset = 0, num = limit, period }
                }
            };

            var dataPayload = JsonSerializer.Serialize(requestData);
            var url = $"https://u.y.qq.com/cgi-bin/musicu.fcg?_={DateTimeOffset.Now.ToUnixTimeMilliseconds()}&data={HttpUtility.UrlEncode(dataPayload)}";

            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                
                var qqResponse = JsonSerializer.Deserialize<QQMusicResponse>(jsonString);

                var songs = new List<Song>();
                // API数据结构已更新，歌曲信息现在在Data.Data.Song数组中
                if (qqResponse?.Detail?.Data?.TopListData?.Song != null)
                {
                    int rank = 1;
                    foreach (var songInfo in qqResponse.Detail.Data.TopListData.Song)
                    {
                        songs.Add(new Song
                        {
                            Rank = rank++,
                            Title = songInfo.Title,
                            Artist = songInfo.SingerName,
                            Album = "" // 新API结构中不包含专辑信息
                        });
                    }
                }
                // 向后兼容：如果新的数据结构中没有歌曲，尝试旧的SongInfoList字段
                else if (qqResponse?.Detail?.Data?.SongInfoList != null)
                {
                    int rank = 1;
                    foreach (var songInfo in qqResponse.Detail.Data.SongInfoList)
                    {
                        var artists = new List<string>();
                        if (songInfo.Singer != null)
                        {
                            foreach (var singer in songInfo.Singer)
                            {
                                artists.Add(singer.Name);
                            }
                        }

                        songs.Add(new Song
                        {
                            Rank = rank++,
                            Title = songInfo.Name,
                            Artist = string.Join(" / ", artists),
                            Album = songInfo.Album?.Name
                        });
                    }
                }
                return songs;
            }
            catch (Exception ex)
            {
                // In a real app, log this exception
                Console.WriteLine(ex.Message);
                return new List<Song>(); // Return empty list on error
            }
        }
    }

    // Helper classes for JSON deserialization
    public class QQMusicResponse
    {
        [JsonPropertyName("detail")]
        public Detail Detail { get; set; }
    }

    public class Detail
    {
        [JsonPropertyName("data")]
        public Data Data { get; set; }
    }

    public class Data
    {
        [JsonPropertyName("data")]
        public TopListData TopListData { get; set; }
        
        // 向后兼容：保留旧的字段
        [JsonPropertyName("songInfoList")]
        public List<SongInfo> SongInfoList { get; set; }
    }

    public class TopListData
    {
        [JsonPropertyName("song")]
        public List<NewSongInfo> Song { get; set; }
        
        [JsonPropertyName("title")]
        public string Title { get; set; }
    }

    // 新API结构的歌曲信息
    public class NewSongInfo
    {
        [JsonPropertyName("rank")]
        public int Rank { get; set; }
        
        [JsonPropertyName("title")]
        public string Title { get; set; }
        
        [JsonPropertyName("singerName")]
        public string SingerName { get; set; }
        
        [JsonPropertyName("songId")]
        public long SongId { get; set; }
    }

    // 旧API结构的歌曲信息（向后兼容）
    public class SongInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("singer")]
        public List<Singer> Singer { get; set; }

        [JsonPropertyName("album")]
        public Album Album { get; set; }
    }

    public class Singer
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    public class Album
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
