using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cross_Platform_Auto_Fetcher.Services;

namespace Cross_Platform_Auto_Fetcher
{
    public class KugouMusicService : MusicServiceBase
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly Regex _jsonRegex = new Regex(@"global.features\s*=\s*(\[.*\]);", RegexOptions.Singleline);

        public override async Task<List<Song>> GetTopListAsync(string topId, int limit = 100)
        {
            var url = $"https://www.kugou.com/yy/rank/home/1-{topId}.html";

            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36");

                var htmlContent = await _httpClient.GetStringAsync(url);
                var match = _jsonRegex.Match(htmlContent);

                if (!match.Success)
                {
                    return new List<Song>();
                }

                var json = match.Groups[1].Value;
                var kugouSongs = JsonSerializer.Deserialize<List<KugouSongInfo>>(json);

                var songs = new List<Song>();
                int rank = 1;
                foreach (var kugouSong in kugouSongs)
                {
                    if (rank > limit) break;

                    var parts = kugouSong.FileName.Split(" - ", 2, StringSplitOptions.RemoveEmptyEntries);
                    var artist = parts.Length > 1 ? parts[0].Trim() : "未知歌手";
                    var title = parts.Length > 1 ? parts[1].Trim() : parts[0].Trim();

                    songs.Add(new Song
                    {
                        Rank = rank++,
                        Artist = artist,
                        Title = title,
                        Album = kugouSong.AlbumName
                    });
                }
                return songs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<Song>();
            }
        }
    }

    public class KugouSongInfo
    {
        [JsonPropertyName("FileName")]
        public string FileName { get; set; }

        [JsonPropertyName("album_name")]
        public string AlbumName { get; set; }
    }
}
