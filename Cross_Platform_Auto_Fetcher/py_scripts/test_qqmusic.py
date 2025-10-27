from qqmusic_optimized import QQMusicAPI

def test_all_toplists():
    """测试QQ音乐所有指定的热门排行榜"""
    qq = QQMusicAPI()
    
    toplists = {
        "飙升榜": 62,
        "热歌榜": 26,
        "新歌榜": 27,
    }
    
    try:
        for name, topid in toplists.items():
            print(f"--- 正在测试 {name} (ID: {topid}) ---")
            # limit=300 尝试获取所有歌曲
            result = qq.get_toplist(topid, limit=300)
            
            if result and result['songs']:
                print(f"榜单名称: {result['title']}")
                print(f"成功获取歌曲数量: {len(result['songs'])}")
                print("前3首歌曲:")
                for song in result['songs'][:3]:
                    print(f"  {song['排名']}. {song['歌曲名']} - {song['歌手']}")
            elif result:
                print(f"榜单 '{result['title']}' 中没有歌曲。")
            else:
                print(f"获取 {name} 失败")
            
            print("\n" + "="*50 + "\n")
            
    except Exception as e:
        print(f"测试过程中出现意外错误: {e}")
    finally:
        print("测试完成。")
        qq.close()

if __name__ == "__main__":
    test_all_toplists()
