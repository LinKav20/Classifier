using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace peer10
{
    [ValueConversion(typeof(string), typeof(BitmapImage))]
    // Class to set image in treeView.
    public class ImageConverter : IValueConverter
    {
        // Create an instance of our class.
        public static ImageConverter Instance = new ImageConverter();
        /// <summary>
        /// Set image by appropriate tag treeViewItem.
        /// </summary>
        /// <param name="value">String tag of treeViewItem.</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>Image to set.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var path = (string)value;

            if (path == null)
                return null;

            if (path[path.Length-1] == '/')
                return new BitmapImage(new Uri($"pack://application:,,,/folder.jpg"));
            else
                return new BitmapImage(new Uri($"pack://application:,,,/item.png"));
        }
        /// <summary>
        /// For real, i don`t need, just interface need.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
