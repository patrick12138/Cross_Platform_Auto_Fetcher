import requests
import json
import time
from datetime import datetime

def debug_qqmusic_api_fixed():
    """调试修复后的QQ音乐API"""
    
    url = 'https://u.y.qq.com/cgi-bin/musicu.fcg'
    headers = {
        'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36',
        'Referer': 'https://y.qq.com/'
    }
    
    period = datetime.now().strftime('%Y-%m-%d')
    
    # 测试飙升榜 (ID: 62)
    data = {
        "comm": {"cv": 4747474, "ct": 24, "format": "json", "inCharset": "utf-8", "outCharset": "utf-8", "notice": 0, "platform": "yqq.json", "needNewCode": 1, "uin": 0, "g_tk_new_20200303": 5381, "g_tk": 5381},
        "detail": {
            "module": "musicToplist.ToplistInfoServer",
            "method": "GetDetail",
            "param": {
                "topId": 62,
                "offset": 0,
                "num": 100,
                "period": period
            }
        }
    }
    
    params = {
        '_': str(int(time.time() * 1000)),
        'data': json.dumps(data)
    }
    
    try:
        response = requests.get(url, params=params, headers=headers, timeout=10)
        response.raise_for_status()
        
        if response.text.strip():
            json_data = response.json()
            
            # 检查数据结构
            detail_data = json_data.get('detail', {})
            print("detail字段内容:", json.dumps(detail_data, indent=2, ensure_ascii=False)[:500] + "...")
            
            toplist_data = detail_data.get('data', {})
            print("\ntoplist_data字段:", json.dumps(toplist_data, indent=2, ensure_ascii=False)[:500] + "...")
            
            # 检查song数组
            song_list = toplist_data.get('song', [])
            print(f"\nsong数组长度: {len(song_list)}")
            
            if song_list:
                print("前3首歌曲:")
                for i, song in enumerate(song_list[:3]):
                    print(f"  {i+1}. {song.get('title', '')} - {song.get('singerName', '')}")
            
            # 检查songInfoList数组（旧字段）
            song_info_list = toplist_data.get('songInfoList', [])
            print(f"\nsongInfoList数组长度: {len(song_info_list)}")
            
            # 检查title
            title = toplist_data.get('title', '未知排行榜')
            print(f"\n榜单标题: {title}")
            
        else:
            print("响应内容为空")
            
    except Exception as e:
        print(f"请求失败: {e}")

if __name__ == "__main__":
    debug_qqmusic_api_fixed()