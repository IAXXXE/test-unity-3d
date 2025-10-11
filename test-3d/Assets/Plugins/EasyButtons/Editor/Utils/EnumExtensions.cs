namespace EasyButtons.Editor.Utils
{
    using System;

    public static class EnumExtensions
    {
        /// <summary>
        /// Allocation-less version of <see cref="Enum.HasFlag"/> that still runs very fast.
        /// </summary>
        /// <param name="thisEnum">The enum to check for flags.</param>
        /// <param name="flag">The flag to search for in <paramref name="thisEnum"/>.</param>
        /// <typeparam name="TEnum">The type of enum.</typeparam>
        /// <returns>Whether <paramref name="thisEnum"/> contains the specified <paramref name="flag"/>.</returns>
        /// <exception cref="Exception">If it was not possible to determine the underlying type of the enum.</exception>
        /// <remarks>
        /// Taken from here https://forum.unity.com/threads/c-hasaflag-method-extension-how-to-not-create-garbage-allocation.616924/#post-4420699
        /// </remarks>
        public static bool ContainsFlag<TEnum>(this TEnum thisEnum, TEnum flag) where TEnum : Enum
        {
            int flagsValue = (int)(object)thisEnum;
            int flagValue = (int)(object)flag;

            return (flagsValue & flagValue) != 0;
        }
    }
}