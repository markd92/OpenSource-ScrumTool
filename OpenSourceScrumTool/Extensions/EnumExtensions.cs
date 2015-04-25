using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace OpenSourceScrumTool.Extensions
{
    public static class EnumExtensions
    {
        public static string DisplayName(this Enum value)
        {
            var enumType = value.GetType();
            var enumValue = Enum.GetName(enumType, value);
            var member = enumType.GetMember(enumValue)[0];

            var attracts = member.GetCustomAttributes(typeof(DisplayAttribute), false);

            if (attracts.Length == 0)
                return value.ToString();

            var outString = ((DisplayAttribute)attracts[0]).Name;

            if (((DisplayAttribute)attracts[0]).ResourceType != null)
            {
                outString = ((DisplayAttribute)attracts[0]).GetName();
            }

            return outString;
        }
    }
}