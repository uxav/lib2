using System;
using System.Linq;

namespace UX.Lib2.UI.Formatters
{
    public class DefaultDateFormatter : IFormatProvider, ICustomFormatter
    {
        #region Implementation of IFormatProvider

        public object GetFormat(Type formatType)
        {
            return formatType == typeof (ICustomFormatter) ? this : null;
        }

        #endregion

        #region Implementation of ICustomFormatter

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (!(arg is DateTime)) throw new NotSupportedException();

            var dt = (DateTime)arg;

            string suffix;

            if (new[] { 11, 12, 13 }.Contains(dt.Day))
            {
                suffix = "th";
            }
            else switch (dt.Day % 10)
            {
                case 1:
                    suffix = "st";
                    break;
                case 2:
                    suffix = "nd";
                    break;
                case 3:
                    suffix = "rd";
                    break;
                default:
                    suffix = "th";
                    break;
            }

            return string.Format("{0:dddd} {1}{2} {0:MMMM}", arg, dt.Day, suffix);
        }

        #endregion
    }
}