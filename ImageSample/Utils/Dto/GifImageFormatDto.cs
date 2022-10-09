using System;
using System.Collections.Generic;
using System.Linq;

namespace ImageSample.Utils.Dto
{
    /// <summary>
    /// GIF画像処理用のDtoクラス
    /// </summary>
    public class GifImageFormatDto : IImageFormatDto
    {
        /// <summary>シグネチャ</summary>
        private readonly static byte[] Signature = { 0x47, 0x49, 0x46 };
        /// <summary>バージョン 87a</summary>
        private readonly static byte[] Version87a = { 0x38, 0x37, 0x61 };
        /// <summary>バージョン 89a</summary>
        private readonly static byte[] Version89a = { 0x38, 0x39, 0x61 };

        /// <summary>Image Separator</summary>
        private const byte ImageSeparator = 0x2c;
        /// <summary>Extension Introducer</summary>
        private const byte ExtensionIntroducer = 0x21;
        /// <summary>Block Terminator</summary>
        private const byte BlockTerminator = 0x00;
        /// <summary>Gif Trailer</summary>
        private const byte GifTrailer = 0x3b;

        /// <summary>Graphic Control Label</summary>
        private const byte GraphicControlLabel = 0xf9;
        /// <summary>Comment Label</summary>
        private const byte CommentLabel = 0xfe;
        /// <summary>Plain Text Label</summary>
        private const byte PlainTextLabel = 0x01;
        /// <summary>Extension Label</summary>
        private const byte ExtensionLabel = 0xff;

        /// <summary>HeaderのPacked Filedのオフセット値</summary>
        private const int HeaderPackedFieldOffset = 10;
        /// <summary>Image BlockのPacked Filedのオフセット値</summary>
        private const int ImageBlockPackedFieldOffset = 9;
        /// <summary>Extension Labelのオフセット値</summary>
        private const int ExtensionLabelOffset = 1;
        /// <summary>Comment ExtensionのBlock Sizeのオフセット値</summary>
        private const int CommentBlockSizeOffset = 2;
        /// <summary>Plain Text ExtensionのBlock Sizeのオフセット値</summary>
        private const int PlainTextBlockSizeOffset = 15;
        /// <summary>Application ExtensionのBlock Sizeのオフセット値</summary>
        private const int ExtensionBlockSizeOffset = 14;
        /// <summary>Color Table Flagのビットマスク</summary>
        private const byte ColorTableFlag = 0x80;
        /// <summary>Size of Color Tableのビットマスク</summary>
        private const byte SizeOfColorTable = 0x07;

        /// <summary>Headerの基本バイト数</summary>
        private const int HeaderBaseSize = 13;
        /// <summary>Image Blockの基本バイト数</summary>
        private const int ImageBlockBaseSize = 11;
        /// <summary>Graphic Control Extensionの基本バイト数</summary>
        private const int GraphicControlBaseSize = 8;
        /// <summary>Comment Extensionの基本バイト数</summary>
        private const int CommentBaseSize = 4;
        /// <summary>Plain Text Extensionの基本バイト数</summary>
        private const int PlainTextBaseSize = 17;
        /// <summary>Application Extensionの基本バイト数</summary>
        private const int ExtensionBaseSize = 16;
        /// <summary>1色当たりのバイト数</summary>
        private const int ColorSize = 3;

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
        /// 　0000h: 47 49 46<br/>
        /// 　0003h: 38 37 61 or 38 39 61<br/>
        /// </summary>
        /// <param name="imageData"></param>
        /// <returns>
        /// true：画像データをGIFとして認識できる場合
        /// false：画像データをGIFとして認識できない場合
        /// </returns>
        public bool CheckImageData(byte[] imageData)
        {
            return Signature.SequenceEqual(imageData.Take(Signature.Length)) &&
                (Version87a.SequenceEqual(imageData.Skip(Signature.Length).Take(Version87a.Length)) ||
                Version89a.SequenceEqual(imageData.Skip(Signature.Length).Take(Version89a.Length)));
        }

        /// <summary>
        /// 画像データをGIFとして解析し、以下のブロックを削除した新たな画像データを生成します。<br/>
        /// 　・Comment Extension<br/>
        /// 　・Plain Text Extension<br/>
        /// 　・Application Extension<br/>
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
                var offset = GetHeader(imageData);
                return GetBlocks(imageData, offset);
            }
            catch
            {
                return false;
            }
        }

        #region Private Method

        /// <summary>
        /// Header / Image Blockのカラーテーブルのバイト数を取得します。
        /// </summary>
        /// <param name="packedField">Color Table FlagとSize of Color Tableを含む値</param>
        /// <returns>カラーテーブルのバイト数</returns>
        private int GetColorTableSize(byte packedField)
        {
            var colorTableSize = 0;
            if ((packedField & ColorTableFlag) == ColorTableFlag)
            {
                var sizeOfColorTable = (packedField & SizeOfColorTable) + 1;
                colorTableSize = ColorSize * (int)Math.Pow(2, sizeOfColorTable);
            }

            return colorTableSize;
        }

        /// <summary>
        /// 画像データからHeaderを取得します。
        /// </summary>
        /// <param name="imageData">画像データ</param>
        /// <returns>ヘッダーのバイト数</returns>
        private int GetHeader(byte[] imageData)
        {
            var headerSize = 0;
            var packedField = imageData[HeaderPackedFieldOffset];
            var colorTableSize = GetColorTableSize(packedField);
            headerSize += HeaderBaseSize + colorTableSize;

            _imageData.AddRange(imageData.Take(headerSize).ToArray());

            return headerSize;
        }

        /// <summary>
        /// 画像データからブロックを取得します。
        /// </summary>
        /// <param name="imageData">画像データ</param>
        /// <param name="offset">ブロックのオフセット値</param>
        /// <returns>処理結果</returns>
        private bool GetBlocks(byte[] imageData, int offset)
        {
            while (offset < imageData.Length)
            {
                var blockId = imageData[offset];
                if (blockId == GifTrailer)
                {
                    break;
                }

                switch (blockId)
                {
                    case ImageSeparator:
                        offset += GetImageBlock(imageData, offset);
                        break;

                    case ExtensionIntroducer:
                        offset += GetExtension(imageData, offset);
                        break;

                    default:
                        return false;
                }
            }

            _imageData.Add(GifTrailer);

            return true;
        }

        /// <summary>
        /// 画像データからImage Blockを取得します。
        /// </summary>
        /// <param name="imageData">画像データ</param>
        /// <param name="offset">Image Blockのオフセット値</param>
        /// <returns>Image Blockのバイト数</returns>
        private int GetImageBlock(byte[] imageData, int offset)
        {
            var imageBlockSize = 0;
            var packedField = imageData[offset + ImageBlockPackedFieldOffset];
            var colorTableSize = GetColorTableSize(packedField);
            imageBlockSize += ImageBlockBaseSize + colorTableSize;

            var blockOffset = offset + imageBlockSize;
            while (blockOffset < imageData.Length)
            {
                var blockSize = imageData[blockOffset];
                if (blockSize == BlockTerminator)
                {
                    imageBlockSize++;
                    break;
                }

                imageBlockSize += blockSize + 1;
                blockOffset += blockSize + 1;
            }

            _imageData.AddRange(imageData.Skip(offset).Take(imageBlockSize));

            return imageBlockSize;
        }

        /// <summary>
        /// 画像データからExtensionを取得します。<br/>
        /// </summary>
        /// <param name="imageData">画像データ</param>
        /// <param name="offset">Extensionのオフセット値</param>
        /// <returns>Extensionのバイト数</returns>
        /// <exception cref="Exception">認識できないExtensionだった場合</exception>
        private int GetExtension(byte[] imageData, int offset)
        {
            switch (imageData[offset + ExtensionLabelOffset])
            {
                case GraphicControlLabel:
                    _imageData.AddRange(imageData.Skip(offset).Take(GraphicControlBaseSize));
                    return GraphicControlBaseSize;

                case CommentLabel:
                    return CommentBaseSize + imageData[offset + CommentBlockSizeOffset];

                case PlainTextLabel:
                    return PlainTextBaseSize + imageData[offset + PlainTextBlockSizeOffset];

                case ExtensionLabel:
                    return ExtensionBaseSize + imageData[offset + ExtensionBlockSizeOffset];

                default:
                    throw new Exception();
            }
        }

        #endregion
    }
}
