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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;

namespace UX.Lib2
{
    public class CsvParser
    {
        #region Fields

        private readonly StreamReader _reader;
        private ReadOnlyDictionary<int, string> _columnTitles;
        private List<ReadOnlyDictionary<string, string>> _results;
        private Dictionary<int, int> _columnWidth;

        #endregion

        #region Constructors

        public CsvParser(Stream stream)
        {
            _reader = new StreamReader(stream);
        }

        #endregion

        #region Finalizers
        #endregion

        #region Events
        #endregion

        #region Delegates
        #endregion

        #region Properties

        public List<string> ColumnTitles
        {
            get { return _columnTitles.Values.ToList(); }
        }

        public List<ReadOnlyDictionary<string, string>> Results
        {
            get { return _results; }
        }

        #endregion

        #region Methods

        public void Parse()
        {
            var titles = new Dictionary<int, string>();
            _columnWidth = new Dictionary<int, int>();
            try
            {
                var line = _reader.ReadLine();
                var columnIndex = 0;
                foreach (var title in Regex.Split(line, @",(?=(?:[^\""]*\""[^\""]*\"")*(?![^\""]*\""))"))
                {
                    titles[columnIndex] = title;
                    _columnWidth[columnIndex] = title.Length;
                    columnIndex ++;
                }
                _columnTitles = new ReadOnlyDictionary<int, string>(titles);
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Could not parse header line, {0}", e.Message));
            }

            _results = new List<ReadOnlyDictionary<string, string>>();
            var lineCount = 0;
            while (_reader.Peek() >= 0)
            {
                lineCount ++;
                try
                {
                    var line = _reader.ReadLine();
                    var columnIndex = 0;
                    var result = new Dictionary<string, string>();
                    foreach (var value in Regex.Split(line, @",(?=(?:[^\""]*\""[^\""]*\"")*(?![^\""]*\""))"))
                    {
                        var newValue = value;
                        if (value.Length > 1 && value.StartsWith("\"") && value.EndsWith("\""))
                        {
                            newValue = value.Substring(1, value.Length - 2);
                        }
                        result[_columnTitles[columnIndex]] = newValue;
                        if (newValue.Length > _columnWidth[columnIndex])
                        {
                            _columnWidth[columnIndex] = newValue.Length;
                        }
                        columnIndex++;
                    }
                    _results.Add(new ReadOnlyDictionary<string, string>(result));
                }
                catch (Exception e)
                {
                    throw new Exception(string.Format("Error parsing csv on line {0}, {1}", lineCount, e.Message));
                }
            }
        }

        public int GetColumnWidth(int columnIndex)
        {
            return _columnWidth[columnIndex];
        }

        #endregion
    }
}