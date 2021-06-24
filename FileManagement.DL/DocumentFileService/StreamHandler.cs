﻿
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FileManagementAPI.DL.DocumentFileService
{
    public interface IStreamHandler
    {
        Task<byte[]> CopyStreamToByteBuffer(Stream stream);

        Task<byte[]> EncryptData(byte[] buffer);

        Task<byte[]> DecryptData(byte[] buffer);

        Task WriteBufferToFile(byte[] buffer, string path, string fileName);
    }

    public class StreamHandler : IStreamHandler
    {
        private readonly AesCryptoServiceProvider _cryptor;

        private readonly string _cryptoKey; // consider removing this var to somewhere more secret place
        private readonly string _IV; // consider removing this var to somewhere more secret place

        public StreamHandler()
        {
            _cryptoKey = "SERVANDASERVANDASERVANDASERVANDA"; // obviously change this in the future :)
            _IV = "SERVANDASERVANDA";

            _cryptor = new AesCryptoServiceProvider();
            _cryptor.BlockSize = 128;
            _cryptor.KeySize = 256;
            _cryptor.Padding = PaddingMode.PKCS7;
            _cryptor.Key = Encoding.ASCII.GetBytes(_cryptoKey);
            _cryptor.IV = Encoding.ASCII.GetBytes(_IV);
        }

        public async Task<byte[]> CopyStreamToByteBuffer(Stream stream)
        {
            var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);

            return memoryStream.ToArray();
        }

        public async Task<byte[]> DecryptData(byte[] buffer)
        {
            using (var encryptor = _cryptor.CreateDecryptor())
            {
                return await PerformCryptography(encryptor, buffer);
            }
        }

        public async Task<byte[]> EncryptData(byte[] buffer)
        {
            using (var encryptor = _cryptor.CreateEncryptor())
            {
                return await PerformCryptography(encryptor, buffer);
            }
        }

        private async Task<byte[]> PerformCryptography(ICryptoTransform cryptoTransform, byte[] data)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
                {
                    await cryptoStream.WriteAsync(data, 0, data.Length);
                    cryptoStream.FlushFinalBlock();
                    return memoryStream.ToArray();
                }
            }
        }

        // todo refactor
        public async Task WriteBufferToFile(byte[] buffer, string path, string fileName)
        {
            using (var fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            using (var memoryStream = new MemoryStream(buffer))
            {
                try
                {
                    byte[] bytes = new byte[memoryStream.Length];
                    await memoryStream.ReadAsync(bytes, 0, (int)memoryStream.Length);
                     await fileStream.WriteAsync(bytes, 0, bytes.Length);
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    fileStream.Close();
                }
            };
        }
       
    }
}