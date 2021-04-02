using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuhnModN
{
    public class LuhnModNConfig
    {
        /// <summary>
        /// Default for tracer token
        /// </summary>
        public LuhnModNConfig() : this("ABCDEFGHJKLMNPRSTUVWXYZ23456789", 10) { }

        /// <summary>
        /// TODO load from config
        /// </summary>
        public LuhnModNConfig(string characterSet, int valueLength)
        {
            if (characterSet.Distinct().Count() != characterSet.Length)
                throw new ArgumentException("Duplicates characters.", nameof(characterSet));

            if (valueLength < 2)
                throw new ArgumentException();

            CharacterSet = characterSet.ToCharArray();
            ValueLength = valueLength;
        }

        public char[] CharacterSet { get; }
        public int ValueLength { get; }
    }

}
