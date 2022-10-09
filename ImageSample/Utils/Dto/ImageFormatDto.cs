using System;

namespace ImageSample.Utils.Dto
{
    /// <summary>
    /// 画像処理用のDtoクラス
    /// </summary>
    public abstract class ImageFormatDto
    {
        /// <summary>処理済み画像データ</summary>
        public byte[] ImageData
        {
            get
            {
                return imageData_;
            }
            private set
            {
                imageData_ = value;
            }
        }

        protected byte[] imageData_ = null;

        /// <summary>
        /// 画像データを認識できるかチェックします。
        /// </summary>
        /// <param name="imageData">画像データ</param>
        /// <returns>
        /// true：画像データを認識できる場合
        /// false：画像データを認識できない場合
        /// </returns>
        public abstract bool CheckImageData(byte[] imageData);

        /// <summary>
        /// 画像データを解析し、Exif情報やメタデータなどを削除した新たな画像データを生成します。
        /// </summary>
        /// <param name="imageData">画像データ</param>
        /// <returns>
        /// true：画像データを生成できた場合<br/>
        /// false：解析エラーが発生した場合<br/>
        /// </returns>
        public abstract bool CreateImageDataNoMetaInfo(byte[] imageData);

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
