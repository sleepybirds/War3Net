﻿// ------------------------------------------------------------------------------
// <copyright file="RandomItemTable.cs" company="Drake53">
// Copyright (c) 2019 Drake53. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// ------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Text;

namespace War3Net.Build
{
    public sealed class RandomItemTable
    {
        private readonly List<RandomItemSet> _sets;

        private int _tableNumber;
        private string _tableName;

        public RandomItemTable()
        {
            _sets = new List<RandomItemSet>();
        }

        public static RandomItemTable Parse(Stream stream, bool leaveOpen = false)
        {
            var table = new RandomItemTable();
            using (var reader = new BinaryReader(stream, new UTF8Encoding(false, true), leaveOpen))
            {
                table._tableNumber = reader.ReadInt32();
                table._tableName = reader.ReadChars();

                var setCount = reader.ReadInt32();
                for (var i = 0; i < setCount; i++)
                {
                    var set = new RandomItemSet();
                    var setSize = reader.ReadInt32();
                    for (var j = 0; j < setSize; j++)
                    {
                        set.AddItem(reader.ReadInt32(), reader.ReadChars(4));
                    }

                    table._sets.Add(set);
                }
            }

            return table;
        }

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(_tableNumber);
            writer.WriteString(_tableName);

            writer.Write(_sets.Count);
            foreach (var set in _sets)
            {
                writer.Write(set.Size);
                foreach (var (chance, id) in set)
                {
                    writer.Write(chance);
                    writer.Write(id);
                }
            }
        }
    }
}