// Converters/TeamIdToColorConverter.cs 
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace debug_interface.Converters 
{
    public class TeamIdToColorConverter : IValueConverter
    {
        // 队伍 0 (取经队) 绿色，队伍 1 (妖怪队) 红色
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is long teamId)
            {
                return teamId == 0 ? Brushes.LightGreen : Brushes.LightCoral;
            }
            // 默认或无效时返回灰色
            return Brushes.LightGray;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}