import requests  
  
class KugouAPI:  
    def __init__(self):  
        self.headers = {  
            'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36'  
        }  
      
    def get_playlist(self, playlist_id):  
        """获取歌单/排行榜数据"""  
        url = f'https://m.kugou.com/plist/list/{playlist_id}?json=true'  
        response = requests.get(url, headers=self.headers)  
        data = response.json()  
          
        # 获取歌单信息  
        info = data['info']['list']  
          
        # 获取歌曲列表  
        songs = []  
        for idx, item in enumerate(data['list']['list']['info'], 1):  
            # 获取详细歌曲信息  
            song_url = f"https://m.kugou.com/app/i/getSongInfo.php?cmd=playInfo&hash={item['hash']}"  
            song_response = requests.get(song_url, headers=self.headers)  
            song_data = song_response.json()  
              
            songs.append({  
                '排名': idx,  
                '歌曲名': song_data.get('songName', ''),  
                '歌手': song_data.get('singerName', '未知'),  
                '专辑': song_data.get('albumName', ''),  
                'Hash': item['hash']  
            })  
          
        return {  
            'title': info['specialname'],  
            'cover': info['imgurl'].replace('{size}', '400') if info.get('imgurl') else '',  
            'songs': songs  
        }  
  
# 使用示例  
kg = KugouAPI()  
  
# 获取酷狗飙升榜 (playlist_id=6666)  
playlist = kg.get_playlist(6666)  
print(f"排行榜: {playlist['title']}")  
for song in playlist['songs'][:10]:  # 只显示前10首  
    print(f"{song['排名']}. {song['歌曲名']} - {song['歌手']}")