using SimModel.Config;
using SimModel.Model;
using System.Collections.Generic;
using System.Linq;

namespace SimModel.Domain
{
    /// <summary>
    /// データ管理クラス
    /// </summary>
    static internal class DataManagement
    {
        /// <summary>
        /// 最近使ったスキルの記憶容量
        /// </summary>
        static public int MaxRecentSkillCount { get; } = LogicConfig.Instance.MaxRecentSkillCount;

        /// <summary>
        /// 除外設定を追加
        /// </summary>
        /// <param name="name">防具名</param>
        /// <returns>除外情報</returns>
        static internal Clude? AddExclude(string name)
        {
            Equipment? equip = Masters.GetEquipByName(name);
            if (equip == null)
            {
                return null;
            }
            return AddClude(equip.Name, CludeKind.exclude);
        }

        /// <summary>
        /// 固定設定を追加
        /// </summary>
        /// <param name="name">防具名</param>
        /// <returns>固定情報</returns>
        static internal Clude? AddInclude(string name)
        {
            Equipment? equip = Masters.GetEquipByName(name);
            if (equip == null || equip.Kind == EquipKind.deco)
            {
                // 装飾品は固定しない
                return null;
            }

            // 同じ装備種類の固定装備があった場合、固定を解除する
            string? toDelete = null;
            foreach (var clude in Masters.Cludes)
            {
                if (clude.Kind == CludeKind.exclude)
                {
                    continue;
                }

                Equipment? oldEquip = Masters.GetEquipByName(clude.Name);
                if (oldEquip == null || oldEquip.Kind.Equals(equip.Kind))
                {
                    toDelete = clude.Name;
                }
            }
            if(toDelete != null)
            {
                DeleteClude(toDelete);
            }

            // 追加
            return AddClude(equip.Name, CludeKind.include);
        }

        /// <summary>
        /// 指定レア度以下を全て除外設定に追加
        /// </summary>
        /// <param name="rare">レア度</param>
        static internal void ExcludeByRare(int rare)
        {
            var equips = Masters.Heads.Union(Masters.Bodys).Union(Masters.Arms).Union(Masters.Waists).Union(Masters.Legs);
            foreach (var equip in equips)
            {
                if (equip.Rare <= rare)
                {
                    AddClude(equip.Name, CludeKind.exclude);
                }
            }
        }

        /// <summary>
        /// 除外・固定の追加
        /// </summary>
        /// <param name="name">防具名</param>
        /// <param name="kind">除外or固定</param>
        /// <returns>除外固定情報</returns>
        static private Clude? AddClude(string name, CludeKind kind)
        {
            Clude? ret = null;

            bool existClude = false;
            foreach (var clude in Masters.Cludes)
            {
                if (clude.Name.Equals(name))
                {
                    // 既に設定がある場合は上書き
                    clude.Kind = kind;
                    existClude = true;
                    ret = clude;
                }
            }
            if (!existClude)
            {
                // 設定がない場合は新規作成
                Clude clude = new();
                clude.Name = name;
                clude.Kind = kind;
                // 追加
                Masters.Cludes.Add(clude);
                ret = clude;
            }

            // マスタへ反映
            FileOperation.SaveCludeCSV();

            // 追加した設定
            return ret;
        }

        /// <summary>
        /// 除外・固定設定の削除
        /// </summary>
        /// <param name="name">防具名</param>
        static internal void DeleteClude(string name)
        {
            foreach (var clude in Masters.Cludes)
            {
                if (clude.Name.Equals(name))
                {
                    // 削除
                    Masters.Cludes.Remove(clude);
                    break;
                }
            }

            // マスタへ反映
            FileOperation.SaveCludeCSV();
        }

        /// <summary>
        /// 除外・固定設定の全削除
        /// </summary>
        static internal void DeleteAllClude()
        {
            Masters.Cludes.Clear();

            // マスタへ反映
            FileOperation.SaveCludeCSV();
        }

        /// <summary>
        /// マイセットの追加
        /// </summary>
        /// <param name="set">マイセット</param>
        /// <returns>追加したマイセット</returns>
        static internal EquipSet AddMySet(EquipSet set)
        {
            // 追加
            Masters.MySets.Add(set);

            // マスタへ反映
            FileOperation.SaveMySetCSV();

            return set;
        }

        /// <summary>
        /// マイセットの削除
        /// </summary>
        /// <param name="set">マイセット</param>
        static internal void DeleteMySet(EquipSet set)
        {
            // 削除
            Masters.MySets.Remove(set);

            // マスタへ反映
            FileOperation.SaveMySetCSV();
        }

        /// <summary>
        /// マイセットの変更を保存
        /// </summary>
        static internal void SaveMySet()
        {
            // マスタへ反映
            FileOperation.SaveMySetCSV();
        }

        /// <summary>
        /// マイセットを再読み込み
        /// </summary>
        static internal void LoadMySet()
        {
            // マスタへ反映
            FileOperation.LoadMySetCSV();
        }

        /// <summary>
        /// 最近使ったスキルの更新
        /// </summary>
        /// <param name="skills">検索したスキル</param>
        internal static void UpdateRecentSkill(List<Skill> skills)
        {
            List<string> newNames = new List<string>();

            // 今回の検索条件をリストに追加
            foreach (var skill in skills)
            {
                newNames.Add(skill.Name);
            }

            // 今までの検索条件をリストに追加
            foreach (var oldName in Masters.RecentSkillNames)
            {
                bool isDuplicate = false;
                foreach (var newName in newNames)
                {
                    if (newName.Equals(oldName))
                    {
                        isDuplicate = true;
                        break;
                    }
                }
                if (!isDuplicate)
                {
                    newNames.Add(oldName);
                }

                // 最大数に達したらそこで終了
                if (MaxRecentSkillCount <= newNames.Count)
                {
                    break;
                }
            }

            // 新しいリストに入れ替え
            Masters.RecentSkillNames = newNames;

            // マスタへ反映
            FileOperation.SaveRecentSkillCSV();
        }

        /// <summary>
        /// マイ検索条件の追加
        /// </summary>
        /// <param name="condition">検索条件</param>
        internal static void AddMyCondition(SearchCondition condition)
        {
            // 追加
            Masters.MyConditions.Add(condition);

            // マスタへ反映
            FileOperation.SaveMyConditionCSV();
        }

        /// <summary>
        /// マイ検索条件の削除
        /// </summary>
        /// <param name="condition">検索条件</param>
        internal static void DeleteMyCondition(SearchCondition condition)
        {
            // 削除
            Masters.MyConditions.Remove(condition);

            // マスタへ反映
            FileOperation.SaveMyConditionCSV();
        }

        /// <summary>
        /// マイ検索条件の更新
        /// </summary>
        /// <param name="newCondition">新データ</param>
        internal static void UpdateMyCondition(SearchCondition newCondition)
        {
            foreach (var condition in Masters.MyConditions)
            {
                if (condition.ID == newCondition.ID)
                {
                    condition.DispName = newCondition.DispName;
                    condition.WeaponSlot1 = newCondition.WeaponSlot1;
                    condition.WeaponSlot2 = newCondition.WeaponSlot2;
                    condition.WeaponSlot3 = newCondition.WeaponSlot3;
                    condition.Def = newCondition.Def;
                    condition.Fire = newCondition.Fire;
                    condition.Water = newCondition.Water;
                    condition.Thunder = newCondition.Thunder;
                    condition.Ice = newCondition.Ice;
                    condition.Dragon = newCondition.Dragon;
                    condition.Sex = newCondition.Sex;
                    condition.Skills = newCondition.Skills;

                    // マスタへ反映
                    FileOperation.SaveMyConditionCSV();

                    return;
                }
            }

            // 万一更新先が見つからなかった場合は新規登録
            AddMyCondition(newCondition);
        }

        /// <summary>
        /// 装飾品の所持数の変更を保存
        /// </summary>
        /// <param name="deco">対象の装飾品</param>
        /// <param name="count">変更後の個数</param>
        internal static void SaveDecoCount(Deco deco, int count)
        {
            deco.DecoCount = count;
            FileOperation.SaveDecoCountJson();
        }
    }
}
