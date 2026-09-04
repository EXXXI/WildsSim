namespace SimModel.Model
{
    /// <summary>
    /// 装備種類
    /// </summary>
    public enum EquipKind
    {
        head,
        body,
        arm,
        waist,
        leg,
        deco,
        charm,
        weapon,
        error
    }

    /// <summary>
    /// 装備種類拡張メソッド用クラス
    /// </summary>
    public static class EquipKindExt
    {
        /// <summary>
        /// 日本語名を返す
        /// </summary>
        /// <param name="kind">Kind</param>
        /// <returns>日本語名</returns>
        public static string Str(this EquipKind kind)
        {
            switch (kind)
            {
                case EquipKind.weapon:
                    return "武器";
                case EquipKind.head:
                    return "頭";
                case EquipKind.body:
                    return "胴";
                case EquipKind.arm:
                    return "腕";
                case EquipKind.waist:
                    return "腰";
                case EquipKind.leg:
                    return "足";
                case EquipKind.deco:
                    return "装飾品";
                case EquipKind.charm:
                    return "護石";
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// 日本語名と区切り用のコロンを返す
        /// </summary>
        /// <param name="kind">Kind</param>
        /// <returns>日本語名と区切り用のコロン</returns>
        public static string StrWithColon(this EquipKind kind)
        {
            return Str(kind) + '：';
        }

        // TODO: 現在未使用。傀異錬成のような防具カスタマイズが来たら使うかも
        /// <summary>
        /// 文字列をEquipKindに
        /// </summary>
        /// <param name="str">文字列</param>
        /// <returns>Kind</returns>
        public static EquipKind ToEquipKind(this string? str)
        {
            return str switch
            {
                "武器" => EquipKind.weapon,
                "頭" => EquipKind.head,
                "胴" => EquipKind.body,
                "腕" => EquipKind.arm,
                "腰" => EquipKind.waist,
                "足" => EquipKind.leg,
                "脚" => EquipKind.leg,// 誤記
                "装飾品" => EquipKind.deco,
                "護石" => EquipKind.charm,
                _ => EquipKind.error,
            };
        }
    }
}
