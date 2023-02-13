using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace TotalCommanderProject
{

    [ValueConversion(typeof(string), typeof(BitmapImage))]
    public class HeaderToImageConverter : IValueConverter
    {
        public static HeaderToImageConverter Instance = new HeaderToImageConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Cela putanja
            var path = (string)value;

            // ako je putanja prazna, ignoriši
            if (path == null)
                return null;

            // dobijemo naziv fajla/foldera
            var name = MainWindow.GetFileFolderName(path);

            // po default-u, pretpostavimo...
            var image = "Images/file.png";

            // ako je naziv prazan, pretpostavimo da je drive jer ne možemo fajl/folder dobiti
            if (string.IsNullOrEmpty(name))
                image = "Images/drive.png";
            else if (new FileInfo(path).Attributes.HasFlag(FileAttributes.Directory))
                image = "Images/folder-closed.png";

            return new BitmapImage(new Uri($"pack://application:,,,/{image}"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
