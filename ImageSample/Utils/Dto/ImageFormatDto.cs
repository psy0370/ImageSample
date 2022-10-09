using System;

namespace ImageSample.Utils.Dto
{
    /// <summary>
    /// 画像処理用のDtoクラス
    /// </summary>
    public abstract class ImageFormatDto
    {
        /// <summary>マジックナンバー</summary>
        public abstract byte[][] MagickNumbers { get; }
        /// <summary>処理済み画像データ</summary>
        public byte[] ImageData => imageData_;

        protected byte[] imageData_ = null;

        /// <summary>
        /// 画像データを解析し、Exif情報やメタデータなどを削除した新たな画像データを生成します。
        /// </summary>
        /// <param name="imageData">画像データ</param>
        /// <returns>
        /// true：画像データを生成できた場合<br/>
        /// false：解析エラーが発生した場合<br/>
        /// </returns>
        public abstract bool CreateImageNoMetaInfo(byte[] imageData);

        /// <summary>
        /// 画像データのマジックナンバーをチェックします。
        /// </summary>
        /// <param name="imageData">画像データ</param>
        /// <returns>
        /// true：マジックナンバーが一致する場合
        /// false：マジックナンバーが一致しない場合
        /// </returns>
        public bool CheckMagicNumber(byte[] imageData)
        {
            foreach (var magicNumber in MagickNumbers)
            {
                if (CompareArray(magicNumber, imageData, 0))
                {
                    return true;
                }
            }

            return false;
        }

        #region Protected Method

        /// <summary>
        /// 配列のオフセットから始まるデータを元配列と比較します。
        /// </summary>
        /// <param name="srcArray"></param>
        /// <param name="dstArray"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        protected bool CompareArray(byte[] srcArray, byte[] dstArray, int offset)
        {
            for (var i = 0; i < srcArray.Length; i++)
            {
                if (srcArray[i] != dstArray[offset + i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// バッファから指定バイト数のデータを逆順で切り出します。
        /// </summary>
        /// <param name="buffer">バッファ</param>
        /// <param name="offset">オフセット</param>
        /// <param name="length">バイト数</param>
        /// <returns>切り出した配列</returns>
        protected byte[] GetSlicedReverseArray(byte[] buffer, int offset, int length)
        {
            var slicedBuffer = new byte[length];
            for (var i = 0; i < length; i++)
            {
                slicedBuffer[i] = buffer[offset + length - (i + 1)];
            }

            return slicedBuffer;
        }

        #endregion
    }
}
