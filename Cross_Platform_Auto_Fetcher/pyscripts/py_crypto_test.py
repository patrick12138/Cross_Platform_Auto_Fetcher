# -*- coding: utf-8 -*-
import requests
import json
import base64
from Crypto.Cipher import AES

# 核心加密算法的Python实现
# 参考自: https://github.com/Binaryify/NeteaseCloudMusicApi/blob/master/util/crypto.js

# --- 常量 --- #
MODULUS = '00e0b509f6259df8642dbc35662901477df22677ec152b5ff68ace615bb7b725152b3ab17a876aea8a5aa76d2e417629ec4ee341f56135fccf695280104e0312ecbda92557c93870114af6c9d05c4f7f0c3685b7a46bee255932575cce10b424d813cfe4875d3e82047b97ddef52741d546b8e289dc6935b3ece0462db0a22b8e7'
NONCE = b'0CoJUm6Qyw8W8jud'
PUBKEY = '010001'

# --- 辅助函数 --- #
def aes_encrypt(text, key, iv):
    pad = 16 - len(text) % 16
    text = text + bytes([pad]) * pad
    cipher = AES.new(key, AES.MODE_CBC, iv)
    encrypted_bytes = cipher.encrypt(text)
    return base64.b64encode(encrypted_bytes)

def rsa_encrypt(text, pub_key, modulus):
    text = text[::-1] # reverse
    rs = int(text.hex(), 16) ** int(pub_key, 16) % int(modulus, 16)
    return format(rs, 'x').zfill(256)

def create_secret_key(size):
    # Use a fixed key for debugging
    return b'abcdefg123456789'

# --- 主加密函数 --- #
def weapi_encrypt(data):
    data_bytes = json.dumps(data).encode('utf-8')
    secret_key = create_secret_key(16)
    iv = b'0102030405060708'

    # 第一次AES加密
    params = aes_encrypt(data_bytes, NONCE, iv)
    # 第二次AES加密
    params = aes_encrypt(params, secret_key, iv)

    # RSA加密
    enc_sec_key = rsa_encrypt(secret_key, PUBKEY, MODULUS)

    return {
        'params': params.decode('utf-8'),
        'encSecKey': enc_sec_key
    }

# --- 测试执行 --- #
def main():
    chart_id = "3778678" # 热歌榜
    url = "https://music.163.com/weapi/v3/playlist/detail"
    
    payload = {
        "id": chart_id,
        "offset": 0,
        "total": True,
        "limit": 1000,
        "n": 1000,
        "csrf_token": ""
    }

    print(f"Python脚本开始测试...")
    print(f"目标榜单ID: {chart_id}")
    print(f"业务数据 (Payload): {json.dumps(payload)}")

    encrypted_data = weapi_encrypt(payload)

    print(f"\n加密后的 encSecKey (部分): {encrypted_data['encSecKey'][:64]}...")

    headers = {
        'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36',
        'Referer': 'https://music.163.com/',
        'Content-Type': 'application/x-www-form-urlencoded'
    }

    try:
        response = requests.post(url, headers=headers, data=encrypted_data)
        print(f"\nHTTP状态码: {response.status_code}")
        print("服务器响应 (原始文本):")
        # 尝试格式化为JSON，如果失败则直接打印文本
        try:
            print(json.dumps(response.json(), indent=2, ensure_ascii=False))
        except json.JSONDecodeError:
            print(response.text)

    except requests.exceptions.RequestException as e:
        print(f"\n请求发生错误: {e}")

if __name__ == '__main__':
    main()
