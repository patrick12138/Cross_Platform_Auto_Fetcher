using Cross_Platform_Auto_Fetcher.Services.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Cross_Platform_Auto_Fetcher.Services
{
    public class NeteaseMusicService : IMusicDataService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private const string PlaylistApiUrl = "https://music.163.com/weapi/v3/playlist/detail";

        public NeteaseMusicService()
        {
            if (_httpClient.DefaultRequestHeaders.UserAgent.Count == 0)
            {
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
                _httpClient.DefaultRequestHeaders.Add("Referer", "https://music.163.com/");
            }
        }

        public async Task<List<Song>> GetTopListAsync(string topId, int limit = 100)
        {
            try
            {
                var payload = new { id = topId, offset = 0, total = true, limit = 1000, n = 1000, csrf_token = "" };
                var encryptedParams = NeteaseCrypto.Weapi(payload);
                var requestContent = new FormUrlEncodedContent(encryptedParams);

                var response = await _httpClient.PostAsync(PlaylistApiUrl, requestContent);
                response.EnsureSuccessStatusCode();
                var jsonResponse = await response.Content.ReadAsStringAsync();

                // Simplified parsing logic, directly targeting playlist.tracks
                var playlistResponse = JsonSerializer.Deserialize<NeteasePlaylistResponse>(jsonResponse);

                var songs = new List<Song>();
                if (playlistResponse?.Playlist?.Tracks != null)
                {
                    int rank = 1;
                    foreach (var track in playlistResponse.Playlist.Tracks.Take(limit))
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
                return songs;
            }
            catch (Exception ex)
            {
                // Log to console or a file in a real app
                Console.WriteLine($"Netease API Error: {ex.Message}");
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