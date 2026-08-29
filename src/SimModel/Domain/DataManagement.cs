using SimModel.Config;
using SimModel.Model;
using System.Collections.Generic;
using System.Linq;

namespace SimModel.Domain
{
    /// <summary>
    /// データ管理クラス
    /// Mastersのデータと保存用実ファイルについて、同期を取りながら管理する
    /// </summary>
    public class DataManagement
    {
        /// <summary>
        /// ファイル操作クラスのインスタンス
        /// DIで注入される
        /// </summary>
        private readonly FileOperation _fileOperation;

        /// <summary>
        /// 護石関連操作クラスのインスタンス
        /// DIで注入される
        /// </summary>
        private readonly CharmAppraiser _charmAppraiser;

        /// <summary>
        /// ロジックの設定クラスのインスタンス
        /// DIで注入される
        /// </summary>
        private readonly LogicConfig _logicConfig;

        /// <summary>
        /// マスタ管理クラスのインスタンス
        /// DIで注入される
        /// </summary>
        private readonly Masters _masters;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="fileOperation"></param>
        public DataManagement(FileOperation fileOperation, CharmAppraiser charmAppraiser, LogicConfig logicConfig, Masters masters)
        {
            _fileOperation = fileOperation;
            _charmAppraiser = charmAppraiser;
            _logicConfig = logicConfig;
            _masters = masters;
        }

        /// <summary>
        /// 初期データ読み込み
        /// </summary>
        internal void LoadData()
        {
            // マスタデータ類の読み込み
            Masters.DefUpgrades = _fileOperation.LoadDefUpgradeCSV();
            Masters.Heads = _fileOperation.LoadHeadCSV(Masters.DefUpgrades);
            Masters.Bodys = _fileOperation.LoadBodyCSV(Masters.DefUpgrades);
            Masters.Arms = _fileOperation.LoadArmCSV(Masters.DefUpgrades);
            Masters.Waists = _fileOperation.LoadWaistCSV(Masters.DefUpgrades);
            Masters.Legs = _fileOperation.LoadLegCSV(Masters.DefUpgrades);
            Masters.Charms = _fileOperation.LoadCharmCSV();
            _masters.Decos = _fileOperation.LoadDecoCSV();
            Masters.Weapons = _fileOperation.LoadWeaponCSV();
            LoadSkill(); // 後処理が必要なためまとめて別メソッドに切り出し
            Masters.ShiningCharmCombos = _fileOperation.LoadAdditionalCharmComboCSV();
            Masters.ShiningCharmGroups = _fileOperation.LoadAdditionalCharmGroupCSV();

            // セーブデータ類の読み込み
            _fileOperation.MakeSaveFolder();
            LoadAdditionalCharm(); // 後処理が必要なためまとめて別メソッドに切り出し
            _masters.Artians = _fileOperation.LoadArtianCSV();
            _masters.Cludes = _fileOperation.LoadCludeCSV();
            _masters.RecentSkillNames = _fileOperation.LoadRecentSkillCSV();
            LoadMyCondition(); // 後処理が必要なためまとめて別メソッドに切り出し
            LoadMySet(); // 後処理が必要なためまとめて別メソッドに切り出し

            // 念のため全装備キャッシュをクリア
            _masters.ClearAllEquipmentsCache();
        }

        /// <summary>
        /// スキル読み込み関連処理
        /// </summary>
        private void LoadSkill()
        {
            var skills = _fileOperation.LoadSkillCSV();

            // どの防具・護石・武器にも存在しないスキルを除外
            // アーティア・追加護石がまだ読み込まれていないため、AllEquipmentsは使わない
            var equips = Masters.Weapons.Union(Masters.Heads).Union(Masters.Bodys).Union(Masters.Arms)
                .Union(Masters.Waists).Union(Masters.Legs).Union(Masters.Charms).Union(_masters.Decos);
            Masters.Skills = skills.Where(skill =>
                equips.Any(e => e.Skills.Any(s => s.Name == skill.Name)))
                .ToList();
        }

        /// <summary>
        /// 追加護石読み込み関連処理
        /// </summary>
        private void LoadAdditionalCharm()
        {
            _masters.AdditionalCharms = _fileOperation.LoadAdditionalCharmCSV();

            // 下位互換護石の計算
            CalcLowerCharm();
        }

        /// <summary>
        /// マイ検索条件読み込み関連処理
        /// </summary>
        private void LoadMyCondition()
        {
            _masters.MyConditions = _fileOperation.LoadMyConditionCSV();
            foreach (var cond in _masters.MyConditions)
            {
                if (cond.WeaponName != null)
                {
                    var artian = _masters.Artians.Where(a => a.Name == cond.WeaponName);
                    if (artian.Any())
                    {
                        cond.WeaponDispName = artian.First().DispName;
                    }
                }
            }
        }

        /// <summary>
        /// マイセット読み込み関連処理
        /// </summary>
        private void LoadMySet()
        {
            // マスタへ反映
            _masters.MySets = _fileOperation.LoadMySetCSV(_masters.AllEquipments);

            // マイセット利用状況の反映のため護石、アーティアを再書き込み
            _fileOperation.SaveAdditionalCharmCSV(_masters.AdditionalCharms, _masters.MySets);
            _fileOperation.SaveArtianCSV(_masters.Artians, _masters.MySets);
        }

        /// <summary>
        /// 除外設定を追加
        /// </summary>
        /// <param name="name">防具名</param>
        /// <returns>除外情報</returns>
        internal Clude? AddExclude(string name)
        {
            Equipment? equip = _masters.GetEquipByName(name);
            if ((equip == null) ||
                ((equip is Weapon weapon) && (weapon.WeaponType == WeaponType.指定なし)))
            {
                // スロット指定用の武器は除外しない
                return null;
            }
            // 仮想装備はMastersに含まれないため、equip == nullの時点で検出される
            //if (equip.IsVirtual)
            //{
            //    // 仮想装備は処理しない
            //    return null;
            //}
            return AddClude(equip.Name, CludeKind.exclude);
        }

        /// <summary>
        /// 固定設定を追加
        /// </summary>
        /// <param name="name">防具名</param>
        /// <returns>固定情報</returns>
        internal Clude? AddInclude(string name)
        {
            Equipment? equip = _masters.GetEquipByName(name);
            if ((equip == null) || 
                (equip.Kind == EquipKind.deco) ||
                (equip is Weapon))
            {
                // 装飾品と武器は固定しない
                return null;
            }
            // 仮想装備はMastersに含まれないため、equip == nullの時点で検出される
            //if (equip.IsVirtual)
            //{
            //    // 仮想装備は処理しない
            //    return null;
            //}

            // 同じ装備種類の固定装備があった場合、固定を解除する
            string? toDelete = null;
            foreach (var clude in _masters.Cludes)
            {
                if (clude.Kind == CludeKind.exclude)
                {
                    continue;
                }

                Equipment? oldEquip = _masters.GetEquipByName(clude.Name);
                if (oldEquip == null || oldEquip.Kind.Equals(equip.Kind))
                {
                    toDelete = clude.Name;
                }
            }
            if(toDelete != null)
            {
                DeleteClude(toDelete, false);
            }

            // 追加
            return AddClude(equip.Name, CludeKind.include);
        }

        // TODO: 護石・装飾品のレア度整備やアーティア・追加護石のレア度入力が必要なため保留
        ///// <summary>
        ///// 指定レア度以下を全て除外設定に追加
        ///// </summary>
        ///// <param name="rare">レア度</param>
        //internal void ExcludeByRare(int rare)
        //{
        //    var equips = Masters.Heads.Union(Masters.Bodys).Union(Masters.Arms).Union(Masters.Waists).Union(Masters.Legs);
        //    foreach (var equip in equips)
        //    {
        //        if (equip.Rare <= rare)
        //        {
        //            AddClude(equip.Name, CludeKind.exclude, false);
        //        }
        //    }

        //    // マスタへ反映
        //    _fileOperation.SaveCludeCSV(_masters.Cludes);
        //}

        /// <summary>
        /// 除外・固定の追加
        /// </summary>
        /// <param name="name">防具名</param>
        /// <param name="kind">除外or固定</param>
        /// <param name="doSave">trueの場合ファイルへの保存を実行(非指定時true、一括実行時のfalse指定を想定)</param>
        /// <returns>除外固定情報</returns>
        private Clude? AddClude(string name, CludeKind kind, bool doSave = true)
        {
            Clude? ret = null;

            bool existClude = false;
            foreach (var clude in _masters.Cludes)
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
                _masters.Cludes.Add(clude);
                ret = clude;
            }

            if (doSave)
            {
                // マスタへ反映
                _fileOperation.SaveCludeCSV(_masters.Cludes);
            }

            // 追加した設定
            return ret;
        }

        /// <summary>
        /// 除外・固定設定の削除
        /// </summary>
        /// <param name="name">防具名</param>
        /// <param name="doSave">trueの場合ファイルへの保存を実行(非指定時true、一括実行時のfalse指定を想定)</param>
        internal void DeleteClude(string name, bool doSave = true)
        {
            foreach (var clude in _masters.Cludes)
            {
                if (clude.Name.Equals(name))
                {
                    // 削除
                    _masters.Cludes.Remove(clude);
                    break;
                }
            }

            if (doSave)
            {
                // マスタへ反映
                _fileOperation.SaveCludeCSV(_masters.Cludes);
            }
        }

        /// <summary>
        /// 除外・固定設定の全削除
        /// </summary>
        internal void DeleteAllClude()
        {
            _masters.Cludes.Clear();

            // マスタへ反映
            _fileOperation.SaveCludeCSV(_masters.Cludes);
        }

        /// <summary>
        /// 防具の除外・固定設定の全削除
        /// </summary>
        internal void DeleteAllArmorClude()
        {
            // 武器と判別不能(データのない装備名)だけ抽出
            List<Clude> weaponCludes = new();
            foreach (var clude in _masters.Cludes)
            {
                Equipment? equip = _masters.GetEquipByName(clude.Name);
                if ((equip == null) || (equip is Weapon))
                {
                    weaponCludes.Add(clude);
                }
            }

            // 抽出したものと入れ替え
            _masters.Cludes.Clear();
            _masters.Cludes.AddRange(weaponCludes);

            // マスタへ反映
            _fileOperation.SaveCludeCSV(_masters.Cludes);
        }

        /// <summary>
        /// 武器の除外・固定設定の全削除
        /// </summary>
        internal void DeleteAllWeaponClude()
        {
            // 防具と判別不能(データのない装備名)だけ抽出
            List<Clude> armorCludes = new();
            foreach (var clude in _masters.Cludes)
            {
                Equipment? equip = _masters.GetEquipByName(clude.Name);
                if ((equip == null) || (equip is not Weapon))
                {
                    armorCludes.Add(clude);
                }
            }

            // 抽出したものと入れ替え
            _masters.Cludes.Clear();
            _masters.Cludes.AddRange(armorCludes);

            // マスタへ反映
            _fileOperation.SaveCludeCSV(_masters.Cludes);
        }

        /// <summary>
        /// マイセットの追加
        /// </summary>
        /// <param name="set">マイセット</param>
        /// <returns>追加したマイセット</returns>
        internal EquipSet? AddMySet(EquipSet set)
        {
            // null指定は無視
            if (set == null)
            {
                return null;
            }

            // 削除できる装備(護石・アーティア)について、マスタに存在しているかチェック
            // 装備無し(NameがEmpty)は登録可
            if (!string.IsNullOrEmpty(set.Charm.Name) &&
                !Masters.Charms.Union(_masters.AdditionalCharms).Any(c => c.Name.Equals(set.Charm.Name)))
            {
                return null;
            }
            if (!string.IsNullOrEmpty(set.Weapon.Name) &&
                !Masters.Weapons.Union(_masters.Artians).Any(c => c.Name.Equals(set.Weapon.Name)))
            {
                return null;
            }

            // 名前がないなら自動生成
            if (string.IsNullOrEmpty(set.Name))
            {
                set.Name = _logicConfig.DefaultMySetName;
            }

            // 追加
            _masters.MySets.Add(set);

            // マスタへ反映
            SaveMySet();

            return set;
        }

        /// <summary>
        /// マイセットの削除
        /// </summary>
        /// <param name="set">マイセット</param>
        /// <param name="doSave">trueの場合ファイルへの保存を実行(非指定時true、一括実行時のfalse指定を想定)</param>
        internal void DeleteMySet(EquipSet set, bool doSave = true)
        {
            // 削除
            bool done = _masters.MySets.Remove(set);

            // マスタへ反映
            if (done && doSave)
            {
                SaveMySet();            
            }
        }

        /// <summary>
        /// マイセットの順番入れ替え
        /// </summary>
        /// <param name="dropIndex">入れ替え元</param>
        /// <param name="targetIndex">入れ替え先</param>
        internal void MoveMySet(int dropIndex, int targetIndex)
        {
            // 引数チェック
            int setCount = _masters.MySets.Count;
            if (dropIndex < 0 || dropIndex >= setCount ||
                targetIndex < 0 || targetIndex >= setCount ||
                dropIndex == targetIndex)
            {
                return;
            }

            EquipSet set = _masters.MySets[dropIndex];
            _masters.MySets.RemoveAt(dropIndex);
            _masters.MySets.Insert(targetIndex, set);

            SaveMySet();
        }

        /// <summary>
        /// マイセットの名前変更
        /// </summary>
        /// <param name="name">変更する名前</param>
        /// <param name="set">マイセット</param>
        internal void ChangeNameOfMySet(string name, EquipSet set)
        {
            // 引数チェック
            if (string.IsNullOrEmpty(name) ||
                set == null ||
                set.Name == name ||
                !_masters.MySets.Contains(set))
            {
                return;
            }
            
            set.Name = name;
            SaveMySet();
        }

        /// <summary>
        /// マイセットの変更を保存
        /// </summary>
        private void SaveMySet()
        {
            // マスタへ反映
            _fileOperation.SaveMySetCSV(_masters.MySets);

            // マイセット利用状況の反映のため護石、アーティアを再書き込み
            _fileOperation.SaveAdditionalCharmCSV(_masters.AdditionalCharms, _masters.MySets);
            _fileOperation.SaveArtianCSV(_masters.Artians, _masters.MySets);
        }

        /// <summary>
        /// 最近使ったスキルの更新
        /// </summary>
        /// <param name="skills">検索したスキル</param>
        internal void UpdateRecentSkill(List<Skill> skills)
        {
            List<string> newNames = new();

            // 今回の検索条件をリストに追加
            // 直近の検索分は上限を超えていても保持する
            foreach (var skill in skills)
            {
                newNames.Add(skill.Name);
            }

            // 今までの検索条件をリストに追加
            foreach (var oldName in _masters.RecentSkillNames)
            {
                // 最大数に達していたらそこで終了
                if (_logicConfig.MaxRecentSkillCount <= newNames.Count)
                {
                    break;
                }
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
            }

            // 新しいリストに入れ替え
            _masters.RecentSkillNames.Clear();
            _masters.RecentSkillNames.AddRange(newNames);

            // マスタへ反映
            _fileOperation.SaveRecentSkillCSV(_masters.RecentSkillNames);
        }

        /// <summary>
        /// マイ検索条件の追加
        /// </summary>
        /// <param name="condition">検索条件</param>
        internal void AddMyCondition(SearchCondition condition)
        {
            // nullは無視
            if (condition == null)
            {
                return;
            }

            // 追加
            _masters.MyConditions.Add(condition);

            // マスタへ反映
            _fileOperation.SaveMyConditionCSV(_masters.MyConditions);
        }

        /// <summary>
        /// マイ検索条件の削除
        /// </summary>
        /// <param name="condition">検索条件</param>
        internal void DeleteMyCondition(SearchCondition condition)
        {
            // 削除
            bool done = _masters.MyConditions.Remove(condition);

            // マスタへ反映
            if (done)
            {
                _fileOperation.SaveMyConditionCSV(_masters.MyConditions);
            }
        }

        /// <summary>
        /// マイ検索条件の更新
        /// </summary>
        /// <param name="name">名前</param>
        /// <param name="condition">更新対象データ</param>
        internal void ChangeNameOfMyCondition(string name, SearchCondition condition)
        {
            // 引数チェック
            if (string.IsNullOrEmpty(name) ||
                condition == null ||
                condition.DispName == name ||
                !_masters.MyConditions.Contains(condition))
            {
                return;
            }

            condition.DispName = name;

            _fileOperation.SaveMyConditionCSV(_masters.MyConditions);
        }

        /// <summary>
        /// 装飾品の所持数の変更を保存
        /// </summary>
        /// <param name="deco">対象の装飾品</param>
        /// <param name="count">変更後の個数</param>
        internal void SaveDecoCount(Deco deco, int count)
        {
            // 引数チェック
            if (count < 0 ||
                deco == null ||
                !_masters.Decos.Contains(deco))
            {
                return;
            }


            deco.DecoCount = count;
            _fileOperation.SaveDecoCountJson(_masters.Decos);
        }

        /// <summary>
        /// 護石の追加
        /// </summary>
        /// <param name="charm">護石</param>
        internal void AddCharm(Equipment charm)
        {
            // 引数チェック
            if (charm == null ||
                _masters.AdditionalCharms.Contains(charm))
            {
                return;
            }

            // 追加
            _masters.AdditionalCharms.Add(charm);

            // 全装備キャッシュをクリア
            _masters.ClearAllEquipmentsCache();

            // 下位互換の再計算
            CalcLowerCharm();

            // マスタへ反映
            _fileOperation.SaveAdditionalCharmCSV(_masters.AdditionalCharms, _masters.MySets);
        }

        /// <summary>
        /// 護石の削除
        /// </summary>
        /// <param name="condition">検索条件</param>
        internal void DeleteCharm(Equipment charm)
        {
            // 引数チェック
            if (charm == null ||
                !_masters.AdditionalCharms.Contains(charm))
            {
                return;
            }

            // 除外・固定設定があったら削除
            DeleteClude(charm.Name);

            // この護石を使っているマイセットがあったら削除
            List<EquipSet> delMySets = new();
            foreach (var set in _masters.MySets)
            {
                if (set.Charm.Name != null && set.Charm.Name.Equals(charm.Name))
                {
                    delMySets.Add(set);
                }
            }
            foreach (var set in delMySets)
            {
                DeleteMySet(set, false);
            }

            // 削除
            _masters.AdditionalCharms.Remove(charm);

            // 全装備キャッシュをクリア
            _masters.ClearAllEquipmentsCache();

            // 下位互換の再計算
            CalcLowerCharm();

            // マスタへ反映
            if (delMySets.Count > 0)
            {
                // MySetに付随して護石も保存される
                SaveMySet();
            }
            else 
            { 
                _fileOperation.SaveAdditionalCharmCSV(_masters.AdditionalCharms, _masters.MySets);
            }
        }

        /// <summary>
        /// 護石の順番入れ替え
        /// </summary>
        /// <param name="dropIndex">入れ替え元</param>
        /// <param name="targetIndex">入れ替え先</param>
        internal void MoveCharm(int dropIndex, int targetIndex)
        {
            // 引数チェック
            int charmCount = _masters.AdditionalCharms.Count;
            if (dropIndex < 0 || dropIndex >= charmCount ||
                targetIndex < 0 || targetIndex >= charmCount ||
                dropIndex == targetIndex)
            {
                return;
            }

            Equipment charm = _masters.AdditionalCharms[dropIndex];
            _masters.AdditionalCharms.RemoveAt(dropIndex);
            _masters.AdditionalCharms.Insert(targetIndex, charm);

            // 全装備キャッシュをクリア
            _masters.ClearAllEquipmentsCache();

            _fileOperation.SaveAdditionalCharmCSV(_masters.AdditionalCharms, _masters.MySets);
        }

        /// <summary>
        /// アーティアの追加
        /// </summary>
        /// <param name="artian">アーティア</param>
        internal void AddArtian(Weapon artian)
        {
            // 引数チェック
            if (artian == null ||
                _masters.Artians.Contains(artian))
            {
                return;
            }

            // 追加
            _masters.Artians.Add(artian);

            // 全装備キャッシュをクリア
            _masters.ClearAllEquipmentsCache();

            // マスタへ反映
            _fileOperation.SaveArtianCSV(_masters.Artians, _masters.MySets);
        }

        /// <summary>
        /// アーティアの削除
        /// </summary>
        /// <param name="artian">アーティア</param>
        internal void DeleteArtian(Weapon artian)
        {
            // 引数チェック
            if (artian == null ||
                !_masters.Artians.Contains(artian))
            {
                return;
            }

            // 除外・固定設定があったら削除
            DeleteClude(artian.Name);

            // このアーティアを使っているマイセットがあったら削除
            List<EquipSet> delMySets = new();
            foreach (var set in _masters.MySets)
            {
                if (set.Weapon.Name != null && set.Weapon.Name.Equals(artian.Name))
                {
                    delMySets.Add(set);
                }
            }
            foreach (var set in delMySets)
            {
                DeleteMySet(set, false);
            }

            // 削除
            _masters.Artians.Remove(artian);

            // 全装備キャッシュをクリア
            _masters.ClearAllEquipmentsCache();

            // マスタへ反映
            if (delMySets.Count > 0)
            {
                // MySetに付随してアーティアも保存される
                SaveMySet();
            }
            else
            {
                _fileOperation.SaveArtianCSV(_masters.Artians, _masters.MySets);
            }
        }

        /// <summary>
        /// アーティアの順番入れ替え
        /// </summary>
        /// <param name="dropIndex">入れ替え元</param>
        /// <param name="targetIndex">入れ替え先</param>
        internal void MoveArtian(int dropIndex, int targetIndex)
        {
            // 引数チェック
            int charmCount = _masters.Artians.Count;
            if (dropIndex < 0 || dropIndex >= charmCount ||
                targetIndex < 0 || targetIndex >= charmCount ||
                dropIndex == targetIndex)
            {
                return;
            }

            Weapon artian = _masters.Artians[dropIndex];
            _masters.Artians.RemoveAt(dropIndex);
            _masters.Artians.Insert(targetIndex, artian);

            // 全装備キャッシュをクリア
            _masters.ClearAllEquipmentsCache();

            _fileOperation.SaveArtianCSV(_masters.Artians, _masters.MySets);
        }

        /// <summary>
        /// 護石の下位互換検出
        /// </summary>
        private void CalcLowerCharm()
        {
            if (!_logicConfig.UseCalcUpperCharm)
            {
                return;
            }

            foreach (var charm in _masters.AdditionalCharms)
            {
                charm.Upper = null;
                Equipment? upper = _charmAppraiser.HasUpperCharm(charm);
                if (upper != null)
                {
                    if (_charmAppraiser.IsLeftUpper(charm, upper))
                    {
                        charm.Upper = (upper, false);
                    }
                    else
                    {
                        charm.Upper = (upper, true);
                    }
                }
            }
        }
    }
}
