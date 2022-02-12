// <copyright file="Counters.cs" company="Nic Jansma">
//  Copyright (c) Nic Jansma 2022 All Right Reserved
// </copyright>
// <author>Nic Jansma</author>
// <email>nic@nicj.net</email>
using System.Text;

namespace RenameRegex
{
    /// <summary>
    /// Numbers of affected items fo info
    /// </summary>
    internal class Counters
    {
        /// <summary>
        /// Nimber of items found to process
        /// </summary>
        public static int Listed = 0;

        /// <summary>
        /// Number of items skipped due Move fails
        /// </summary>
        public static int Fails = 0;

        /// <summary>
        /// Number of items skipped due Auth errors
        /// </summary>
        public static int Errors = 0;

        /// <summary>
        /// Number of items processed
        /// </summary>
        public static int Done = 0;

        /// <summary>
        /// Final info of counters
        /// </summary>
        /// <returns></returns>
        public static string Total()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine().Append($"Total {Listed} items listed");

            if (Fails > 0)
            {
                sb.Append($", {Fails} skipped");
            }

            if (Errors > 0)
            {
                sb.Append($", {Errors} errors");
            }

            sb.Append($", {Done} done.");

            return sb.ToString();
        }
    }
}
