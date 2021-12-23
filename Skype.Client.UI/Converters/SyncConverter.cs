using Skype.Client.UI.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Skype.Client.UI.Converters
{
    public class SyncConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            Filter ti = value as Filter;
            if (ti != null)
            {
                if (ti.Flag == true) //Pause All Animations
                    return false;
                if (ti.Worked == false)
                    return true;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return new NotImplementedException();
        }
    }
}
