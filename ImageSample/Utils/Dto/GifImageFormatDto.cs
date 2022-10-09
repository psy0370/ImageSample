using System;

namespace ImageSample.Utils.Dto
{
    /// <summary>
    /// GIF画像処理用のDtoクラス
    /// </summary>
    public class GifImageFormatDto : ImageFormatDto
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
        public override bool CheckImageData(byte[] imageData)
        {
            return CompareArray(Signature, imageData, 0) && (CompareArray(Version87a, imageData, Signature.Length) || CompareArray(Version89a, imageData, Signature.Length));
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
        public override bool CreateImageDataNoMetaInfo(byte[] imageData)
        {
            imageData_ = new byte[imageData.Length];

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

            Buffer.BlockCopy(imageData, 0, imageData_, 0, headerSize);

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
            var srcOffset = offset;
            var dstOffset = offset;
            while (srcOffset < imageData.Length)
            {
                var blockId = imageData[srcOffset];
                if (blockId == ImageSeparator)
                {
                    GetImageBlock(imageData, ref srcOffset, ref dstOffset);
                }
                else if (blockId == ExtensionIntroducer)
                {
                    GetExtension(imageData, ref srcOffset, ref dstOffset);
                }
                else if (blockId == GifTrailer)
                {
                    imageData_[dstOffset++] = GifTrailer;
                    break;
                }
                else
                {
                    break;
                }
            }

            Array.Resize(ref imageData_, dstOffset);

            return true;
        }

        /// <summary>
        /// 画像データからImage Blockを取得します。
        /// </summary>
        /// <param name="imageData">画像データ</param>
        /// <param name="srcOffset">Image Blockのオフセット値</param>
        /// <param name="dstOffset">出力先のオフセット値</param>
        /// <returns>Image Blockのバイト数</returns>
        private void GetImageBlock(byte[] imageData, ref int srcOffset, ref int dstOffset)
        {
            var imageBlockSize = 0;
            var packedField = imageData[srcOffset + ImageBlockPackedFieldOffset];
            var colorTableSize = GetColorTableSize(packedField);
            imageBlockSize += ImageBlockBaseSize + colorTableSize;

            var blockOffset = srcOffset + imageBlockSize;
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

            Buffer.BlockCopy(imageData, srcOffset, imageData_, dstOffset, imageBlockSize);
            srcOffset += imageBlockSize;
            dstOffset += imageBlockSize;
        }

        /// <summary>
        /// 画像データからExtensionを取得します。<br/>
        /// </summary>
        /// <param name="imageData">画像データ</param>
        /// <param name="srcOffset">Extensionのオフセット値</param>
        /// <param name="dstOffset">出力先のオフセット値</param>
        /// <returns>Extensionのバイト数</returns>
        /// <exception cref="Exception">認識できないExtensionだった場合</exception>
        private void GetExtension(byte[] imageData, ref int srcOffset, ref int dstOffset)
        {
            switch (imageData[srcOffset + ExtensionLabelOffset])
            {
                case GraphicControlLabel:
                    Buffer.BlockCopy(imageData, srcOffset, imageData_, dstOffset, GraphicControlBaseSize);
                    srcOffset += GraphicControlBaseSize;
                    dstOffset += GraphicControlBaseSize;
                    break;

                case CommentLabel:
                    srcOffset += CommentBaseSize + imageData[srcOffset + CommentBlockSizeOffset];
                    break;

                case PlainTextLabel:
                    srcOffset += PlainTextBaseSize + imageData[srcOffset + PlainTextBlockSizeOffset];
                    break;

                case ExtensionLabel:
                    srcOffset += ExtensionBaseSize + imageData[srcOffset + ExtensionBlockSizeOffset];
                    break;

                default:
                    throw new Exception();
            }
        }

        #endregion
    }
}
