import requests  
import json  
import re  
from html.parser import HTMLParser  
  
class QQMusicAPI:  
    def __init__(self):  
        self.headers = {  
            'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36',  
            'Referer': 'https://y.qq.com/'  
        }  
      
    def get_toplist_period(self, topid):  
        """获取排行榜周期信息"""  
        url = 'https://c.y.qq.com/node/pc/wk_v15/top.html'  
        response = requests.get(url, headers=self.headers)  
        html = response.text  
          
        # 使用正则表达式提取周期信息  
        pattern = r'data-listname="(.+?)" data-tid=".*?\/(.+?)" data-date="(.+?)"'  
        matches = re.findall(pattern, html)  
          
        for match in matches:  
            if match[1] == str(topid):  
                return match[2]  # 返回period  
        return None  
      
    def get_toplist(self, topid, limit=100):  
        """获取排行榜数据"""  
        period = self.get_toplist_period(topid)  
        if not period:  
            return None  
          
        url = 'https://u.y.qq.com/cgi-bin/musicu.fcg'  
        data = {  
            "comm": {"cv": 1602, "ct": 20},  
            "toplist": {  
                "module": "musicToplist.ToplistInfoServer",  
                "method": "GetDetail",  
                "param": {  
                    "topid": topid,  
                    "num": limit,  
                    "period": period  
                }  
            }  
        }  
          
        params = {  
            'format': 'json',  
            'inCharset': 'utf8',  
            'outCharset': 'utf-8',  
            'platform': 'yqq.json',  
            'needNewCode': '0',  
            'data': json.dumps(data)  
        }  
          
        response = requests.get(url, params=params, headers=self.headers)  
        result = response.json()  
          
        # 解析歌曲列表  
        songs = []  
        for song in result['toplist']['data']['songInfoList']:  
            songs.append({  
                '排名': len(songs) + 1,  
                '歌曲名': self.html_decode(song['name']),  
                '歌手': self.html_decode(song['singer'][0]['name']),  
                '专辑': self.html_decode(song['album']['name']),  
                '歌曲ID': song['mid']  
            })  
          
        return {  
            'title': result['toplist']['data']['data']['title'],  
            'songs': songs  
        }  
      
    @staticmethod  
    def html_decode(text):  
        """HTML解码"""  
        h = HTMLParser()  
        return h.unescape(text)  
  
# 使用示例  
qq = QQMusicAPI()  
  
# 获取飙升榜 (topid=62)  
toplist = qq.get_toplist(62)  
print(f"排行榜: {toplist['title']}")  
for song in toplist['songs'][:10]:  # 只显示前10首  
    print(f"{song['排名']}. {song['歌曲名']} - {song['歌手']}")