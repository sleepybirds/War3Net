﻿// ------------------------------------------------------------------------------
// <copyright file="SoundType.cs" company="Drake53">
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.
// </copyright>
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace War3Net.Runtime.Enums
{
    public sealed class SoundType
    {
        private static readonly Dictionary<int, SoundType> _types = GetTypes().ToDictionary(t => (int)t, t => new SoundType(t));

        private readonly Type _type;

        private SoundType(Type type)
        {
            _type = type;
        }

        public enum Type
        {
            Effect = 0,
            EffectLooped = 1,
        }

        public static SoundType GetSoundType(int i)
        {
            if (!_types.TryGetValue(i, out var soundType))
            {
                soundType = new SoundType((Type)i);
                _types.Add(i, soundType);
            }

            return soundType;
        }

        private static IEnumerable<Type> GetTypes()
        {
            foreach (Type type in Enum.GetValues(typeof(Type)))
            {
                yield return type;
            }
        }
    }
}