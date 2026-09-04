namespace SimModel.Domain
{
    /// <summary>
    /// Parse系のUtilクラス
    /// </summary>
    public static class ParseUtil
    {
        /// <summary>
        /// int.Parseを実行
        /// </summary>
        /// <param name="str">Parse対象</param>
        /// <returns>Parse結果、ただし、失敗した場合は0</returns>
        static public int Parse(string str)
        {
            return Parse(str, 0);
        }

        /// <summary>
        /// int.Parseを実行
        /// </summary>
        /// <param name="str">Parse対象</param>
        /// <param name="def">失敗時の値</param>
        /// <returns>Parse結果、ただし、失敗した場合は指定した失敗時の値</returns>
        static public int Parse(string str, int def)
        {
            if (int.TryParse(str, out int num))
            {
                return num;
            }
            else
            {
                return def;
            }
        }
    }
}
