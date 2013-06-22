using System;
using System.Collections.Generic;
using System.Text;

namespace Jdn45Common
{
    /// <summary>
    /// This class defines delegates that can be used for various tasks.
    /// </summary>
    public static class Delegates
    {
        /// <summary>
        /// Delegate to mark progress in operations that may take time.
        /// The progress should be a number between 0 and 100 that represents progress percentage, although it can contain other values.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public delegate void ProgressHandlerValue(int progress);

        /// <summary>
        /// Delegate to mark progress in operations that may take time.
        /// The text argument has information about the task (such as the task name).
        /// The progress should be a number between 0 and 100 that represents progress percentage, although it can contain other values.
        /// </summary>
        /// <param name="text">Text to set. Can be null for no changes. Can be empty ("") to set blank.</param>
        /// <param name="progress"></param>
        public delegate void ProgressHandlerTextAndValue(string text, int progress);
    }
}
