namespace MyChat.Client
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using MyChat.Client.Model;

    internal sealed class EnumToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            UserState state = (UserState)value;
            switch (state)
            {
                case UserState.Idle:
                    return "/Images/bullet_ball_yellow.png";
                case UserState.Offline:
                    return "/Images/bullet_ball_grey.png";
                case UserState.New:
                case UserState.Online:
                    return "/Images/bullet_ball_green.png";
                case UserState.Writing:
                    return "/Images/bullet_ball_blue.png";
            }

            return "/Images/bullet_ball_grey.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
