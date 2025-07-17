using System;

namespace Utilizr.Extensions
{
    public static class VersionEx
    {
        public static string SafeToString(this Version version, int fieldCount)
        {
            var safeFieldCount = Math.Min(fieldCount, GetDefinedFieldCount(version));
            return version.ToString(safeFieldCount);
        }

        /// <summary>
        /// Check against the various components to ensure a valid fieldCount.
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public static int GetDefinedFieldCount(this Version? version)
        {
            if (version == null)
                return 0;

            if (version.Revision >= 0)
                return 4;

            if (version.Build >= 0)
                return 3;

            return 2;
        }
    }
}
