﻿// ------------------------------------------------------------------------------
// <copyright file="SylkParser.cs" company="Drake53">
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.
// </copyright>
// ------------------------------------------------------------------------------

using System;
using System.IO;
using System.Text;

namespace War3Net.IO.Slk
{
    public sealed class SylkParser
    {
        private SylkTable _table;
        private int? _lastY;

        public SylkParser()
        {
            _lastY = null;
        }

        public SylkTable Parse(Stream input, bool leaveOpen = false)
        {
            using var reader = new StreamReader(input, Encoding.UTF8, true, 1024, leaveOpen);

            var isOnFirstLine = true;
            while (true)
            {
                var line = reader.ReadLine();
                var fields = line.Split(';');
                var recordType = fields[0];

                string GetField(string fieldName, bool mandatory)
                {
                    foreach (var field in fields)
                    {
                        if (field.StartsWith(fieldName))
                        {
                            return field.Substring(fieldName.Length);
                        }
                    }

                    if (mandatory)
                    {
                        throw new InvalidDataException($"Record does not contain mandatory field of type '{fieldName}'.");
                    }

                    return null;
                }

                if (isOnFirstLine)
                {
                    isOnFirstLine = false;
                    if (recordType != "ID")
                    {
                        throw new InvalidDataException("SYLK file must start with 'ID'.");
                    }

                    GetField("P", true);
                }
                else
                {
                    switch (recordType)
                    {
                        case "ID":
                            throw new InvalidDataException("Record type 'ID' can only occur on the first line.");

                        case "B":
                            if (_table != null)
                            {
                                throw new InvalidDataException("Only one record of type 'B' may be present.");
                            }

                            _table = new SylkTable(int.Parse(GetField("X", true)), int.Parse(GetField("Y", true)));
                            break;

                        case "C":
                            if (_table == null)
                            {
                                throw new InvalidDataException("Unable to parse record of type 'C' before encountering a record of type 'B'.");
                            }

                            SetCellContent(GetField("X", true), GetField("Y", false), GetField("K", false));
                            break;

                        case "E":
                            return _table;

                        default:
                            throw new NotSupportedException($"Support for record type '{recordType}' is not implemented. Only records of type 'ID', 'B', 'C', and 'E' are supported.");
                    }
                }
            }
        }

        private void SetCellContent(string x, string y, string value)
        {
            if (y == null && _lastY == null)
            {
                throw new InvalidDataException("Row for cell is not defined.");
            }

            var xi = int.Parse(x);
            var yi = y == null ? _lastY.Value : int.Parse(y);

            if (value.StartsWith('"') && value.EndsWith('"'))
            {
                _table[xi - 1, yi - 1] = value[1..^1];
            }
            else if (int.TryParse(value, out var @int))
            {
                _table[xi - 1, yi - 1] = @int;
            }
            else if (float.TryParse(value, out var @float))
            {
                _table[xi - 1, yi - 1] = @float;
            }
            else if (string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
            {
                _table[xi - 1, yi - 1] = true;
            }
            else if (string.Equals(value, "false", StringComparison.OrdinalIgnoreCase))
            {
                _table[xi - 1, yi - 1] = false;
            }
            else if (string.Equals(value, "#VALUE!", StringComparison.Ordinal))
            {
                _table[xi - 1, yi - 1] = 0;
            }
            else
            {
                throw new NotSupportedException("Can only parse strings, integers, floats, and booleans.");
            }

            _lastY = yi;
        }
    }
}