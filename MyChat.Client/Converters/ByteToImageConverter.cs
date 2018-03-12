namespace MyChat.Client
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Windows.Data;
    using System.Windows.Media.Imaging;

    internal sealed class ByteToImageConverter : IValueConverter
    {        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var bytes = value as IReadOnlyCollection<byte>;
            if (bytes != null && bytes.Count > 0)
            {
                using (MemoryStream memStream = new MemoryStream(bytes.ToArray()))
                {
                    return BitmapFrame.Create(memStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
