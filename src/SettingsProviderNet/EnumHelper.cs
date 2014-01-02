using System;
using System.Collections.Generic;
using System.Linq;

namespace SettingsProviderNet
{
    public static class EnumHelper
    {
        public static IEnumerable<T> GetValues<T>()
        {
            return GetValues(typeof(T))
                .OfType<T>();
        }

        public static IEnumerable<object> GetValues(Type enumType)
        {
            if (!enumType.IsEnum)
                throw new ArgumentException("enumType must be an Enum type", "enumType");

            return enumType
                .GetFields()
                .Where(field => field.IsLiteral)
                .Select(field => field.GetValue(enumType));
        }
    }
}