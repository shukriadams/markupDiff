using System.Collections.Generic;

namespace MarkupDiff
{
    /// <summary>
    /// Utility library for doing things with strings.
    /// </summary>
    public class StringHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="main"></param>
        /// <param name="startTag"></param>
        /// <param name="endTag"></param>
        /// <returns></returns>
        public static string[] ReturnBetweenAll(
            string main,
            string startTag,
            string endTag
            )
        {
            List<string> items = new List<string>();

            while (true)
            {
                if (!main.Contains(startTag) || !main.Contains(endTag))
                    break;
                else
                {
                    int startPosition = main.IndexOf(startTag) + startTag.Length;
                    if (startPosition >= main.Length)
                        break;
                    int endPosition = main.IndexOf(endTag, startPosition);
                    if (endPosition >= main.Length)
                        break;

                    string find = main.Substring(startPosition, endPosition - startPosition);
                    items.Add(find);

                    if (endPosition + endTag.Length >= main.Length)
                        break;
                    main = main.Substring(endPosition + endTag.Length);
                }
            }

            return items.ToArray();

        }

        /// <summary> Returns all text in a string after the first occurrence of a substring </summary>
        /// <param name="main"></param>
        /// <param name="sub"></param>
        /// <returns></returns>
        public static string ReturnAfter(
            string main,
            string sub
            )
        {
            if ((main.Length == 0) || (sub.Length == 0) || (main.IndexOf(sub) == -1))
            {
                return string.Empty;
            }

            int intReturnStringStartPosition = main.IndexOf(sub) + sub.Length;
            int intReturnStringLength = main.Length - intReturnStringStartPosition;
            return main.Substring(intReturnStringStartPosition, intReturnStringLength);
        }

        public static string ReturnUpto(
            string main,
            string sub
            )
        {
            if ((main.Length == 0) || (sub.Length == 0) || (main.IndexOf(sub) == -1))
                return string.Empty;


            int position = main.IndexOf(sub);
            main = main.Substring(0, position);
            return main;
        }

        /// <summary> 
        /// Returns all text in a string from the first occurrence of a substring to the first occurrence 
        /// of another substring. 
        /// </summary>
        /// <param name="main"></param>
        /// <param name="startTag"></param>
        /// <param name="endTag"></param>
        /// <returns></returns>
        public static string ReturnBetween(
            string main,
            string startTag,
            string endTag
            )
        {
            //note the argument for the end_tag. start searching for it one after the start_tag incase tags are equal. that way, it won't detect one tag for both arguments.

            if (!main.Contains(startTag) || !main.Contains(endTag))
                return string.Empty;
            else
            {
                if (!main.Contains(startTag) || !main.Contains(endTag))
                    return string.Empty;
                else
                {
                    int startPosition = main.IndexOf(startTag) + startTag.Length;
                    if (startPosition >= main.Length)
                        return string.Empty;
                    int endPosition = main.IndexOf(endTag, startPosition);
                    if (endPosition >= main.Length)
                        return string.Empty;

                    return main.Substring(startPosition, endPosition - startPosition);
                }
            }
        }
    }
}
