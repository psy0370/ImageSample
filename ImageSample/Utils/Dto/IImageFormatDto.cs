namespace ImageSample.Utils.Dto
{
    /// <summary>
    /// 画像データを扱うDtoインターフェース
    /// </summary>
    public interface IImageFormatDto
    {
        /// <summary>処理済み画像データ</summary>
        byte[] ImageData { get; }

        /// <summary>
        /// 画像データを認識できるかチェックします。
        /// </summary>
        /// <param name="imageData">画像データ</param>
        /// <returns>
        /// true：画像データを認識できる場合
        /// false：画像データを認識できない場合
        /// </returns>
        bool CheckImageData(byte[] imageData);

        /// <summary>
        /// 画像データを解析し、Exif情報やメタデータなどを削除した新たな画像データを生成します。
        /// </summary>
        /// <param name="imageData">画像データ</param>
        /// <returns>
        /// true：画像データを生成できた場合<br/>
        /// false：解析エラーが発生した場合<br/>
        /// </returns>
        bool CreateImageDataNoMetaInfo(byte[] imageData);
    }
}
