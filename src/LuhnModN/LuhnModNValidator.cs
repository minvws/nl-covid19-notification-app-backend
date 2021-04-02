using System;

namespace LuhnModN
{
    public class LuhnModNValidator
    {
        private readonly LuhnModNConfig _config;

        public LuhnModNValidator(LuhnModNConfig config)
        {
            _config = config;
        }

        public bool Validate(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            var factor = 1;
            var sum = 0;
            for (var index = value.Length - 1; index >= 0; --index)
            {
                var codePoint = Array.IndexOf(_config.CharacterSet, value[index]);
                var addend = factor * codePoint;
                factor = factor == 2 ? 1 : 2;
                sum += addend / _config.CharacterSet.Length + addend % _config.CharacterSet.Length;
            }
            return sum % _config.CharacterSet.Length == 0;
        }
    }

}
