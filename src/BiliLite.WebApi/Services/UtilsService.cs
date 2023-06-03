using System;
using System.Security.Cryptography;
using System.Text;

namespace BiliLite.WebApi.Services
{
    public class UtilsService
    {
        /// <summary>
        /// 构造CorrespondPath，.net standard2.1以上可用，以后更新WinUI再启用
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public string BuildCorrespondPath(long timestamp)
        {
            // 定义公钥
            string publicKey = "-----BEGIN PUBLIC KEY-----\nMIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDLgd2OAkcGVtoE3ThUREbio0Eg\nUc/prcajMKXvkCKFCWhJYJcLkcM2DKKcSeFpD/j6Boy538YXnR6VhcuUJOhH2x71\nnzPjfdTcqMz7djHum0qSZA0AyCBDABUqCrfNgCiJ00Ra7GmRj+YCK1NJEuewlb40\nJNrRuoEUXpabUzGB8QIDAQAB\n-----END PUBLIC KEY-----";
            // 定义字符串payload
            string payload = $"refresh_{timestamp}";

            // 将字符串payload转换为字节数组
            byte[] data = Encoding.UTF8.GetBytes(payload);

            RSAEncryptionPadding oaepsha256 = RSAEncryptionPadding.OaepSHA256;

            RSA rsa;

            if(Environment.OSVersion.Platform == PlatformID.Unix)
            {
                rsa = new RSAOpenSsl();
            }
            else
            {
                rsa = new RSACng();
            }

            rsa.ImportFromPem(publicKey);
            var ciphertext = rsa.Encrypt(data, oaepsha256);

            // 将字节数组转换为十六进制字符串
            var hex = BitConverter.ToString(ciphertext);

            // 去掉分隔符
            hex = hex.Replace("-", "");
            return hex.ToLower();
        }
    }
}
