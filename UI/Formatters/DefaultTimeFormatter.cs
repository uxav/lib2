using System;

namespace UX.Lib2.UI.Formatters
{
    public class DefaultTimeFormatter : IFormatProvider, ICustomFormatter
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

            return string.Format("{0:h:mm} {0:tt}", arg);
        }

        #endregion
    }
}