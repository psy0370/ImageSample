using System.IO;

namespace ImageSample.Utils.Services
{
    public class ImageValidationService
    {
        public static bool ValidationImage(Stream stream, string filename)
        {
            // サイズチェック
            if (stream.Length > 10 * 1024 * 1024)
            {
                return false;
            }

            var imageData = new byte[stream.Length];
            stream.Read(imageData, 0, imageData.Length);

            // 拡張子チェック
            Dto.ImageFormatDto dto;
            var extension = Path.GetExtension(filename).ToLower();
            switch (extension)
            {
                case ".png":
                    dto = new Dto.PngImageFormatDto();
                    break;

                case ".jpg":
                case ".jpeg":
                    dto = new Dto.JpegImageFormatDto();
                    break;

                case ".gif":
                    dto = new Dto.GifImageFormatDto();
                    break;

                default:
                    return false;
            }

            // 拡張子偽造チェック・バイナリ解析・Exif情報/メタデータ削除
            if (!dto.CheckMagicNumber(imageData) || !dto.CreateImageNoMetaInfo(imageData))
            {
                return false;
            }

            File.WriteAllBytes(filename, dto.ImageData);

            return true;
        }
    }
}
