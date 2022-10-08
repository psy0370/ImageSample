using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;

namespace ImageSample
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true;
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return;
            }

            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var file in files)
            {
                var directory = Path.GetDirectoryName(file);
                var filename = Path.GetFileNameWithoutExtension(file);
                var fullpath = Path.Combine(directory, filename + " (1)" + Path.GetExtension(file));

                using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    if (Utils.Services.ImageValidationService.ValidationImage(stream, fullpath))
                    {
                        Info.Text = "";
                        ProcessedImage.Source = new BitmapImage(new Uri(fullpath));
                    }
                    else
                    {
                        Info.Text = "ダメファイル";
                        ProcessedImage.Source = null;
                    }
                    stream.Close();
                }
            }
        }
    }
}
