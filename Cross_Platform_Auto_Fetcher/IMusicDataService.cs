using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cross_Platform_Auto_Fetcher
{
    public interface IMusicDataService
    {
        Task<List<Song>> GetTopListAsync(string topId, int limit = 100);
    }
}
