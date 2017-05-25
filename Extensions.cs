using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NumberTooLong
{
	public static class Extensions
	{
		public static string WithDecimalSeparators(this string number)
		{
			int insertBeforeModIndex = number.Length % 3;
			StringBuilder newNumberText = new StringBuilder();
			for (int i = 0; i < number.Length; i++)
			{
				if (i > 0 && i % 3 == insertBeforeModIndex)
					newNumberText.Append("_");

				newNumberText.Append(number[i]);
			}

			return newNumberText.ToString();
		}

		public static int GetUnderscoreCount(this string text)
			=> text.Count(c => c == '_');
	}
}
