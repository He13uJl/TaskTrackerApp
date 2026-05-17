using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using TaskTracker.Models;

namespace TaskTracker.Converters
{
    public class PriorityToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TaskPriority priority)
            {
                return priority switch
                {
                    TaskPriority.Высокий => new SolidColorBrush(Colors.Red),
                    TaskPriority.Средний => new SolidColorBrush(Colors.Orange),
                    TaskPriority.Низкий => new SolidColorBrush(Colors.Green),
                    _ => new SolidColorBrush(Colors.Gray)
                };
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}