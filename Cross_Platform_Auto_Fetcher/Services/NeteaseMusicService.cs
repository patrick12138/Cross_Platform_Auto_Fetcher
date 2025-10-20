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
        private const string SongDetailApiUrl = "https://music.163.com/weapi/v6/song/detail";

        public NeteaseMusicService()
        {
            // Add required headers to avoid being blocked
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
                // Step 1: Get track IDs from the playlist
                var playlistPayload = new { id = topId, offset = 0, total = true, limit = 1000, n = 1000, csrf_token = "" };
                var encryptedPlaylistParams = NeteaseCrypto.Weapi(playlistPayload);
                var requestContent = new FormUrlEncodedContent(encryptedPlaylistParams);

                var response = await _httpClient.PostAsync(PlaylistApiUrl, requestContent);
                response.EnsureSuccessStatusCode();
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var playlistResponse = JsonSerializer.Deserialize<NeteasePlaylistResponse>(jsonResponse);

                var trackIds = playlistResponse?.Playlist?.TrackIds?.Take(limit).Select(t => t.Id).ToList();
                if (trackIds == null || !trackIds.Any())
                {
                    return new List<Song>();
                }

                // Step 2: Get song details from track IDs
                var songDetailPayload = new
                {
                    c = "[" + string.Join(",", trackIds.Select(id => $"{{\"id\":{id}}}")) + "]"
                };
                var encryptedSongParams = NeteaseCrypto.Weapi(songDetailPayload);
                requestContent = new FormUrlEncodedContent(encryptedSongParams);

                response = await _httpClient.PostAsync(SongDetailApiUrl, requestContent);
                response.EnsureSuccessStatusCode();
                jsonResponse = await response.Content.ReadAsStringAsync();
                var songDetailResponse = JsonSerializer.Deserialize<NeteaseSongDetailResponse>(jsonResponse);

                // Step 3: Map to our Song model
                var songs = new List<Song>();
                if (songDetailResponse?.Songs != null)
                {
                    int rank = 1;
                    foreach (var track in songDetailResponse.Songs)
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
                Console.WriteLine($"Netease API Error: {ex.Message}");
                return new List<Song>();
            }
        }
    }

    // Helper classes for deserialization
    public class NeteasePlaylistResponse
    {
        [JsonPropertyName("playlist")]
        public NeteasePlaylist Playlist { get; set; }
    }

    public class NeteasePlaylist
    {
        [JsonPropertyName("trackIds")]
        public List<NeteaseTrackId> TrackIds { get; set; }
    }

    public class NeteaseTrackId
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }
    }

    public class NeteaseSongDetailResponse
    {
        [JsonPropertyName("songs")]
        public List<NeteaseTrackDetail> Songs { get; set; }
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