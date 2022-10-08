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
            Dto.IImageFormatDto dto;
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

            // 拡張子偽造チェック
            if (!dto.CheckImageData(imageData))
            {
                return false;
            }

            // 解析・Exif情報・メタデータ削除
            if (!dto.CreateImageDataNoMetaInfo(imageData))
            {
                return false;
            }

            File.WriteAllBytes(filename, dto.ImageData);

            return true;
        }
    }
}
