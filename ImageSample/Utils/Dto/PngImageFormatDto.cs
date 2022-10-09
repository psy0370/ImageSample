using System;

namespace ImageSample.Utils.Dto
{
    /// <summary>
    /// PNG画像処理用のDtoクラス
    /// </summary>
    public class PngImageFormatDto : ImageFormatDto
    {
        /// <summary>シグネチャ</summary>
        private readonly static byte[] Signature = { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a };
        /// <summary>IEND Image trailer</summary>
        private readonly static byte[] ImageTrailerChunkType = { 0x49, 0x45, 0x4e, 0x44 };
        /// <summary>削除するチャンクタイプ</summary>
        private readonly static byte[][] IgnoreChunkTypes = new byte[][] { new byte[] { 0x74, 0x45, 0x58, 0x74 }, new byte[] { 0x7a, 0x54, 0x58, 0x74 }, new byte[] { 0x69, 0x54, 0x58, 0x74 } };
        /// <summary>チャンク長さのバイト数</summary>
        private const int ChunkLengthSize = 4;
        /// <summary>チャンクタイプの長さ</summary>
        private const int ChunkLengthOffset = 4;
        /// <summary>チャンクの基本バイト数</summary>
        private const int ChunkBaseSize = 12;

        /// <summary>マジックナンバー</summary>
        public override byte[][] MagickNumbers => new byte[][] { Signature };

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
        public override bool CreateImageNoMetaInfo(byte[] imageData)
        {
            imageData_ = new byte[imageData.Length];
            Buffer.BlockCopy(Signature, 0, imageData_, 0, Signature.Length);

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
        /// 削除するチャンクタイプかチェックします。
        /// </summary>
        /// <param name="imageData">画像データ</param>
        /// <param name="offset">チャンクのオフセット値</param>
        /// <returns>結果</returns>
        private bool CheckIgnoreChunkType(byte[] imageData, int offset)
        {
            foreach (var ignoreChunkType in IgnoreChunkTypes)
            {
                if (CompareArray(ignoreChunkType, imageData, offset + ChunkLengthOffset))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// IEND Image trailerかチェックします。
        /// </summary>
        /// <param name="imageData">画像データ</param>
        /// <param name="offset">チャンクのオフセット値</param>
        /// <returns>結果</returns>
        private bool CheckImageTrailerChunkType(byte[] imageData, int offset)
        {
            return CompareArray(ImageTrailerChunkType, imageData, offset + ChunkLengthOffset);
        }

        /// <summary>
        /// 画像データからチャンクを取得します。
        /// </summary>
        /// <param name="imageData">画像データ</param>
        /// <returns>処理結果</returns>
        private bool GetChunks(byte[] imageData)
        {
            var srcOffset = Signature.Length;
            var dstOffset = Signature.Length;
            while (srcOffset < imageData.Length)
            {
                var chunkLength = BitConverter.ToInt32(GetSlicedReverseArray(imageData, srcOffset, ChunkLengthSize), 0);
                var chunkSize = ChunkBaseSize + chunkLength;
                if (!CheckIgnoreChunkType(imageData, srcOffset))
                {
                    Buffer.BlockCopy(imageData, srcOffset, imageData_, dstOffset, chunkSize);
                    dstOffset += chunkSize;
                }

                if (CheckImageTrailerChunkType(imageData, srcOffset))
                {
                    break;
                }

                srcOffset += chunkSize;
            }

            Array.Resize(ref imageData_, dstOffset);

            return true;
        }

        #endregion
    }
}
