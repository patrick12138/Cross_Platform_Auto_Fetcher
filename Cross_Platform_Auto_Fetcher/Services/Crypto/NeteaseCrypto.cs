using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Cross_Platform_Auto_Fetcher.Services.Crypto
{
    public static class NeteaseCrypto
    {
        private static readonly string _modulus = "00e0b509f6259df8642dbc35662901477df22677ec152b5ff68ace615bb7b725152b3ab17a876aea8a5aa76d2e417629ec4ee341f56135fccf695280104e0312ecbda92557c93870114af6c9d05c4f7f0c3685b7a46bee255932575cce10b424d813cfe4875d3e82047b97ddef52741d546b8e289dc6935b3ece0462db0a22b8e7";
        private static readonly string _nonce = "0CoJUm6Qyw8W8jud";
        private static readonly string _pubKey = "010001";

        public static Dictionary<string, string> Weapi(object payload)
        {
            var jsonPayload = JsonSerializer.Serialize(payload);
            var secretKey = CreateSecretKey(16);

            var encText = AesEncrypt(AesEncrypt(jsonPayload, _nonce, CipherMode.CBC), secretKey, CipherMode.CBC);
            var encSecKey = RsaEncrypt(secretKey, _pubKey, _modulus);

            return new Dictionary<string, string>
            {
                { "params", encText },
                { "encSecKey", encSecKey }
            };
        }

        private static string CreateSecretKey(int size)
        {
            const string keys = "0123456789abcdef";
            var key = new StringBuilder();
            var rnd = new Random();
            for (int i = 0; i < size; i++)
            {
                key.Append(keys[rnd.Next(0, 16)]);
            }
            return key.ToString();
        }

        private static string AesEncrypt(string text, string key, CipherMode mode)
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.Mode = mode;
            aes.IV = Encoding.UTF8.GetBytes("0102030405060708");
            aes.Padding = PaddingMode.PKCS7;

            var textBytes = Encoding.UTF8.GetBytes(text);
            using var encryptor = aes.CreateEncryptor();
            var encryptedBytes = encryptor.TransformFinalBlock(textBytes, 0, textBytes.Length);

            return Convert.ToBase64String(encryptedBytes);
        }

        private static string RsaEncrypt(string text, string pubKey, string modulus)
        {
            // This implementation now mirrors the Python script's logic exactly.
            // Python: text = text[::-1]  => reverse
            // Python: rs = int(text.hex(), 16) ** int(pub_key, 16) % int(modulus, 16)

            // 1. Reverse the secret key string.
            var reversedText = new string(text.Reverse().ToArray());

            // 2. Get its bytes.
            var textBytes = Encoding.UTF8.GetBytes(reversedText);

            // 3. Convert the bytes to a hex string (lowercase to match Python's .hex())
            var hexText = Convert.ToHexString(textBytes).ToLower();

            // 4. Parse the hex string into a BigInteger.
            var biText = BigInteger.Parse("0" + hexText, NumberStyles.HexNumber);

            var biPubKey = BigInteger.Parse(pubKey, NumberStyles.HexNumber);
            var biModulus = BigInteger.Parse(modulus, NumberStyles.HexNumber);

            // 5. Perform modular exponentiation
            var biResult = BigInteger.ModPow(biText, biPubKey, biModulus);

            // 6. Return as lowercase hex string, padded to 256 characters
            return biResult.ToString("x").PadLeft(256, '0');
        }
    }
}