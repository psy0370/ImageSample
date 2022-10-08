using System;
using System.Collections.Generic;
using System.Linq;

namespace ImageSample.Utils.Dto
{
    /// <summary>
    /// Jpeg画像処理用のDtoクラス
    /// </summary>
    public class JpegImageFormatDto : IImageFormatDto
    {
        /// <summary>SOI(Start of Image)</summary>
        private readonly static byte[] SoiSegment = { 0xff, 0xd8 };
        /// <summary>SOS(Start Of Scan)</summary>
        private const byte SosMarkerName = 0xda;

        /// <summary>マーカー識別子</summary>
        private const byte MarkerId = 0xff;
        /// <summary>データを持たないセグメントのマーカー名</summary>
        private readonly static byte[] NoDataSegmentMarkerNames = { 0xd8, 0xd9 };
        /// <summary>削除するセグメントのマーカー名</summary>
        private readonly static byte[] IgnoreSegmentMarkerNames = { 0xe1, 0xe2, 0xe3, 0xe4, 0xe5, 0xe6, 0xe7, 0xe8, 0xe9, 0xea, 0xeb, 0xec, 0xed, 0xef, 0xfe };
        /// <summary>スキャンデータとして扱うマーカー名</summary>
        private readonly static byte[] SkipScanMarkerName = { 0x00, 0xd0, 0xd1, 0xd2, 0xd3, 0xd4, 0xd5, 0xd6, 0xd7 };

        /// <summary>マーカー名のオフセット値</summary>
        private const int MarkerNameOffset = 1;
        /// <summary>セグメント長のオフセット値</summary>
        private const int SegmentLengthOffset = 2;
        /// <summary>マーカーのバイト数</summary>
        private const int MarkerSize = 2;

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
        /// 　0000h: FF D8<br/>
        /// </summary>
        /// <param name="imageData">画像データ</param>
        /// <returns>
        /// true：画像データをJpegとして認識できる場合
        /// false：画像データをJpegとして認識できない場合
        /// </returns>
        public bool CheckImageData(byte[] imageData)
        {
            return SoiSegment.SequenceEqual(imageData.Take(SoiSegment.Length));
        }

        /// <summary>
        /// 画像データをJpegとして解析し、以下のセグメントを削除した新たな画像データを生成します。<br/>
        /// 　・COM(Comment)<br/>
        /// 　・APPn(Application Data)（APP0とAPP14は除外）<br/>
        /// </summary>
        /// <param name="imageData">画像データ</param>
        /// <returns>
        /// true：画像データを生成できた場合<br/>
        /// false：解析エラーが発生した場合<br/>
        /// </returns>
        public bool CreateImageDataNoMetaInfo(byte[] imageData)
        {
            _imageData.Clear();

            try
            {
                return GetSegments(imageData);
            }
            catch
            {
                return false;
            }
        }

        #region Private Method

        /// <summary>
        /// スキャンデータのバイト数を取得します。
        /// </summary>
        /// <param name="imageData">画像データ</param>
        /// <param name="offset">スキャンデータのオフセット値</param>
        /// <returns>スキャンデータのバイト数</returns>
        private int GetScanDataSize(byte[] imageData, int offset)
        {
            var scanDataSize = 0;
            var isMarker = false;
            while (offset < imageData.Length)
            {
                var data = imageData[offset++];

                if (isMarker)
                {
                    if (!SkipScanMarkerName.Contains(data))
                    {
                        break;
                    }

                    scanDataSize += MarkerSize;
                    isMarker = false;
                }
                else if (data == MarkerId)
                {
                    isMarker = true;
                }
                else
                {
                    scanDataSize++;
                }
            }

            return scanDataSize;
        }

        /// <summary>
        /// 画像データから各種セグメントを取得します。
        /// </summary>
        /// <param name="imageData">画像データ</param>
        /// <returns>処理結果</returns>
        private bool GetSegments(byte[] imageData)
        {
            var offset = 0;
            while (offset < imageData.Length)
            {
                if (imageData[offset] != MarkerId)
                {
                    break;
                }

                var segmentSize = MarkerSize;
                var markerName = imageData[offset + MarkerNameOffset];
                if (!NoDataSegmentMarkerNames.Contains(markerName))
                {
                    var segmentLength = BitConverter.ToInt16(imageData.Skip(offset + SegmentLengthOffset).Take(2).Reverse().ToArray(), 0);
                    segmentSize += segmentLength;
                }

                if (markerName == SosMarkerName)
                {
                    segmentSize += GetScanDataSize(imageData, offset + segmentSize);
                }

                if (!IgnoreSegmentMarkerNames.Contains(markerName))
                {
                    _imageData.AddRange(imageData.Skip(offset).Take(segmentSize));
                }

                offset += segmentSize;
            }

            return true;
        }

        #endregion
    }
}
