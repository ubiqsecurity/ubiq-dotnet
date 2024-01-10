using System;

namespace UbiqSecurity.Fpe.Helpers
{
	internal static class ArrayHelper
	{
		internal static void Fill<T>(T[] array, T value, int startIndex, int toIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException(nameof(array), UbiqResources.NullArrayArgument);
			}

			for (int i = startIndex; i < toIndex; i++)
			{
				array[i] = value;
			}
		}
	}
}
