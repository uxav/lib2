/* License
 * ------------------------------------------------------------------------------
 * Copyright (c) 2019 UX Digital Systems Ltd
 *
 * Permission is hereby granted, to any person obtaining a copy of this software
 * and associated documentation files (the "Software"), to deal in the Software
 * for the continued use and development of the system on which it was installed,
 * and to permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * Any persons obtaining the software have no rights to use, copy, modify, merge,
 * publish, distribute, sublicense, and/or sell copies of the Software without
 * written persmission from UX Digital Systems Ltd, if it is not for use on the
 * system on which it was originally installed.
 * ------------------------------------------------------------------------------
 * UX.Digital
 * ----------
 * http://ux.digital
 * support@ux.digital
 */

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UX.Lib2
{
    public class ConsoleTable
    {
        #region Fields

        private readonly List<string> _headers;
        private readonly int[] _columnWidths;
        private readonly List<IEnumerable<string>> _rows = new List<IEnumerable<string>>(); 

        #endregion

        #region Constructors

        public ConsoleTable(params string[] headers)
        {
            _headers = new List<string>(headers);
            _columnWidths = new int[headers.Count()];

            var col = 0;
            foreach (var header in _headers)
            {
                _columnWidths[col] = header.Length;
                col++;
            }
        }

        #endregion

        #region Finalizers
        #endregion

        #region Events
        #endregion

        #region Delegates
        #endregion

        #region Properties

        public int TotalWidth
        {
            get
            {
                return _columnWidths.Aggregate(1, (current, width) => current + (width + 3));
            }
        }

        #endregion

        #region Methods

        public void AddRow(params object[] items)
        {
            var values = new List<string>();
            var col = 0;
            foreach (var item in items)
            {
                if (item == null)
                {
                    values.Add(string.Empty);
                    col++;
                    continue;
                }
                var s = item.ToString();
                values.Add(s);
                if (s.Length > _columnWidths[col])
                    _columnWidths[col] = s.Length;
                col++;
            }
            _rows.Add(values);
        }

        public override string ToString()
        {
            return ToString(false);
        }

        public string ToString(bool useColor)
        {
            var sb = new StringBuilder();

            var colTag1 = useColor ? Debug.AnsiYellow : string.Empty;
            var colTag2 = useColor ? Debug.AnsiGreen : string.Empty;
            var colTagClose = useColor ? Debug.AnsiReset : string.Empty;
            
            sb.Append('|');
            var divLine = "|";

            var col = 0;
            foreach (var header in _headers)
            {
                var s = colTag1 + header.PadRight(_columnWidths[col]) + colTagClose;
                sb.Append(" " + s + " |");
                var dashes = string.Empty;
                for (var i = 0; i < _columnWidths[col]; i++)
                {
                    dashes = dashes + "-";
                }
                divLine = divLine + " " + dashes + " |";
                col++;
            }
            sb.AppendLine();

            sb.AppendLine(divLine);

            foreach (var row in _rows)
            {
                col = 0;
                var items = row as string[] ?? row.ToArray();
                sb.Append('|');
                foreach (var item in items)
                {
                    var s = item.PadRight(_columnWidths[col]);
                    if (col == 0)
                    {
                        s = colTag2 + s + colTagClose;
                    }
                    sb.Append(" " + s + " |");
                    col++;
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        #endregion
    }
}