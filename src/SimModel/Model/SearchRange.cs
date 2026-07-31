using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimModel.Model
{
    /// <summary>
    /// 検索対象を定めるクラス
    /// </summary>
    public class SearchRange
    {
        /// <summary>
        /// 検索対象の武器一覧
        /// </summary>
        public List<Weapon> Weapons { get; set; }

        /// <summary>
        /// 検索対象の頭一覧
        /// </summary>
        public List<Equipment> Heads { get; set; }

        /// <summary>
        /// 検索対象の胴一覧
        /// </summary>
        public List<Equipment> Bodys { get; set; }

        /// <summary>
        /// 検索対象の腕一覧
        /// </summary>
        public List<Equipment> Arms { get; set; }

        /// <summary>
        /// 検索対象の腰一覧
        /// </summary>
        public List<Equipment> Waists { get; set; }

        /// <summary>
        /// 検索対象の足一覧
        /// </summary>
        public List<Equipment> Legs { get; set; }

        /// <summary>
        /// 検索対象の護石一覧
        /// </summary>
        public List<Equipment> Charms { get; set; }

        /// <summary>
        /// 検索対象の装飾品一覧
        /// </summary>
        public List<Deco> Decos { get; set; }

        /// <summary>
        /// 検索時の除外固定一覧
        /// </summary>
        public List<Clude> Cludes { get; set; }

        /// <summary>
        /// コンストラクタ
        /// 初期状態としてMastersをベースにする
        /// </summary>
        public SearchRange(SearchCondition condition)
        {
            // TODO: Mastersのシングルトン化後、コンストラクタインジェクションにする

            // 武器
            if (condition.IsSpecificWeapon)
            {
                // 武器が指定されている場合、その武器のみを検索対象にする
                Weapons = new();
                Weapon? weapon = Masters.Weapons.Union(Masters.Artians).Where(w => w.Name == condition.WeaponName).FirstOrDefault();
                if (weapon != null)
                {
                    Weapons.Add(weapon);
                }
            }
            else if (condition.IsBestArtianSearch)
            {
                // 理論値検索が指定されている場合、関連するアーティア武器を洗い出して追加する
                Weapons = Masters.Weapons.Union(Masters.Artians).Union(condition.MakeRelatedArtians()).Where(w => w.WeaponType == condition.WeaponType).ToList();
            }
            else
            {
                // 通常
                Weapons = Masters.Weapons.Union(Masters.Artians).Where(w => w.WeaponType == condition.WeaponType).ToList();
            }

            // 護石
            if (condition.IsBestCharmSearch)
            {
                // 理論値護石検索が指定されている場合、関連する護石を洗い出して追加する
                Charms = Masters.Charms.Union(Masters.AdditionalCharms)
                    .Union(condition.MakeRelatedCharms(Masters.ShiningCharmCombos, Masters.ShiningCharmGroups)).ToList();
            }
            else
            {
                // 通常
                Charms = Masters.Charms.Union(Masters.AdditionalCharms).ToList();
            }

            // 他はMastersをベースにする
            Heads = Masters.Heads;
            Bodys = Masters.Bodys;
            Arms = Masters.Arms;
            Waists = Masters.Waists;
            Legs = Masters.Legs;
            Decos = Masters.Decos;
            Cludes = Masters.Cludes;
        }

    }
}
