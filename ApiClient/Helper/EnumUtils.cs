using System;
using System.ComponentModel;
using System.Linq;

namespace ApiClient.Helper {
    public static class EnumUtils {
        /// <summary>
        /// Gets enum description attribute value or converts enum to string
        /// </summary>
        /// <typeparam name="TEnum">enum type</typeparam>
        /// <param name="tEnum">enum</param>
        /// <returns>string</returns>
        public static string ToDescription<TEnum>(TEnum tEnum) {
            var type = typeof(TEnum);
            if (!type.IsEnum) {
                throw new ArgumentException();
            }
            var attribute = type.GetField(tEnum.ToString())
                .GetCustomAttributes(typeof (DescriptionAttribute), false)
                .SingleOrDefault() as DescriptionAttribute;
            return attribute != null ? attribute.Description : Convert.ToString(tEnum);
        }

        /// <summary>
        /// Gets enum property from description attribute value or default enum property
        /// </summary>
        /// <typeparam name="TEnum">enum type</typeparam>
        /// <param name="description">string</param>
        /// <returns>enum</returns>
        public static TEnum ToEnum<TEnum>(string description) {
            var type = typeof (TEnum);
            if (!type.IsEnum) {
                throw new ArgumentException();
            }
            var field = type.GetFields().SelectMany(f => f
                .GetCustomAttributes(typeof (DescriptionAttribute), false), 
                    (f, a) => new {Field = f, Attr = a})
                .SingleOrDefault(a => ((DescriptionAttribute) a.Attr)
                    .Description.Equals(description, StringComparison.OrdinalIgnoreCase));
            return field == null ? default(TEnum) : (TEnum) field.Field.GetRawConstantValue();
        }
    }
}