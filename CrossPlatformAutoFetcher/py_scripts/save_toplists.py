import csv
from qqmusic_optimized import QQMusicAPI
import os

def save_toplists_to_csv():
    """
    获取QQ音乐排行榜数据并保存到CSV文件中。
    """
    qq = QQMusicAPI()
    
    toplists = {
        "飙升榜": 62,
        "热歌榜": 26,
        "新歌榜": 27,
    }
    
    # 创建一个目录来存放CSV文件
    output_dir = "qqmusic_toplists"
    if not os.path.exists(output_dir):
        os.makedirs(output_dir)
        print(f"创建目录: {output_dir}")
        
    try:
        for name, topid in toplists.items():
            print(f"--- 正在获取 {name} (ID: {topid}) ---")
            result = qq.get_toplist(topid, limit=300)
            
            if result and result['songs']:
                filename = os.path.join(output_dir, f"{name}.csv")
                headers = ['排名', '歌曲名', '歌手', '专辑']
                
                try:
                    with open(filename, 'w', newline='', encoding='utf-8-sig') as f:
                        writer = csv.DictWriter(f, fieldnames=headers, extrasaction='ignore')
                        writer.writeheader()
                        writer.writerows(result['songs'])
                    print(f"成功将 {len(result['songs'])} 首歌曲保存到 {filename}")
                except IOError as e:
                    print(f"写入文件时出错: {e}")

            elif result:
                print(f"榜单 '{result['title']}' 中没有歌曲。")
            else:
                print(f"获取 {name} 失败")
            
            print("\n" + "="*50 + "\n")
            
    except Exception as e:
        print(f"处理过程中出现意外错误: {e}")
    finally:
        print("所有榜单处理完成。")
        qq.close()

if __name__ == "__main__":
    save_toplists_to_csv()
