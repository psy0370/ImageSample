using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ImageSample.Utils.Dto
{
    internal class PngImageFormatDto : IImageFormatDto
    {
        /// <summary>シグネチャ</summary>
        private readonly static byte[] Signature = { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a };

        /// <summary>IEND Image trailer</summary>
        private const string ImageTrailerChunkType = "IEND";

        /// <summary>削除するチャンクタイプ</summary>
        private readonly static string[] IgnoreChunkTypes = { "tEXt", "zTXt", "iTXt" };

        /// <summary>チャンクタイプの長さ</summary>
        private const int ChunkTypeLength = 4;
        /// <summary>チャンクの基本バイト数</summary>
        private const int ChunkBaseSize = 12;

        /// <summary>処理済み画像データ</summary>
        public byte[] ImageData
        {
            get
            {
                return _imageData.ToArray();
            }
        }

        private List<byte> _imageData = new List<byte>();

        /// <summary>
        /// 先頭データが以下の通り一致するかチェックします。<br/>
        /// 　0000h: 89 50 4E 47 0D 0A 1A 0A<br/>
        /// </summary>
        /// <param name="imageData">画像データ</param>
        /// <returns>
        /// true：画像データをPNGとして認識できる場合
        /// false：画像データをPNGとして認識できない場合
        /// </returns>
        public bool CheckImageData(byte[] imageData)
        {
            return Signature.SequenceEqual(imageData.Take(Signature.Length));
        }

        /// <summary>
        /// 画像データをPNGとして解析し、以下のチャンクを削除した新たな画像データを生成します。<br/>
        /// 　・tEXt(Textual data)<br/>
        /// 　・zTXt(Compressed textual data)<br/>
        /// 　・iTXt(International textual data)<br/>
        /// </summary>
        /// <param name="imageData">画像データ</param>
        /// <returns>
        /// true：画像データを生成できた場合<br/>
        /// false：解析エラーが発生した場合<br/>
        /// </returns>
        public bool CreateImageDataNoMetaInfo(byte[] imageData)
        {
            _imageData.Clear();
            _imageData.AddRange(Signature);

            try
            {
                return GetChunks(imageData);
            }
            catch
            {
                return false;
            }
        }

        #region Private Method

        /// <summary>
        /// 画像データからチャンクを取得します。
        /// </summary>
        /// <param name="imageData">画像データ</param>
        /// <returns>処理結果</returns>
        private bool GetChunks(byte[] imageData)
        {
            var offset = Signature.Length;
            while (offset < imageData.Length)
            {
                var chunkLength = BitConverter.ToInt32(imageData.Skip(offset).Take(Marshal.SizeOf(typeof(int))).Reverse().ToArray(), 0);
                var chunkType = Encoding.ASCII.GetString(imageData.Skip(offset + Marshal.SizeOf(typeof(int))).Take(ChunkTypeLength).ToArray());
                var chunkSize = ChunkBaseSize + chunkLength;
                if (!IgnoreChunkTypes.Contains(chunkType))
                {
                    _imageData.AddRange(imageData.Skip(offset).Take(chunkSize));
                }

                if (chunkType == ImageTrailerChunkType)
                {
                    return true;
                }

                offset += chunkSize;
            }

            return false;
        }

        #endregion
    }
}
