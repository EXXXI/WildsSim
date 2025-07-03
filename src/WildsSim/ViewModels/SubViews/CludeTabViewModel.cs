using Reactive.Bindings;
using WildsSim.Util;
using WildsSim.ViewModels.Controls;
using SimModel.Model;
using SimModel.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Linq.Expressions;
using Google.OrTools.ConstraintSolver;
using Google.Protobuf.Collections;

namespace WildsSim.ViewModels.SubViews
{
    /// <summary>
    /// 除外固定設定タブのVM
    /// </summary>
    class CludeTabViewModel : ChildViewModelBase
    {
        /// <summary>
        /// 除外・固定画面の防具表形式表示の各行のVM
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<CludeGridArmorRowViewModel>> ShowingArmors { get; } = new();

        /// <summary>
        /// 除外・固定画面の武器表形式表示の各行のVM
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<CludeGridWeaponRowViewModel>> ShowingWeapons { get; } = new();

        /// <summary>
        /// フィルタ用名前入力欄
        /// </summary>
        public ReactivePropertySlim<string> FilterName { get; } = new();

        /// <summary>
        /// フィルタ用設定済絞り込みフラグ
        /// </summary>
        public ReactivePropertySlim<bool> IsCludeOnlyFilter { get; } = new(false);

        /// <summary>
        /// 除外固定をすべて解除するコマンド
        /// </summary>
        public ReactiveCommand DeleteAllCludeCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// 防具の除外固定をすべて解除するコマンド
        /// </summary>
        public ReactiveCommand DeleteAllArmorCludeCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// 武器の除外固定をすべて解除するコマンド
        /// </summary>
        public ReactiveCommand DeleteAllWeaponCludeCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// フィルタをクリアするコマンド
        /// </summary>
        public ReactiveCommand ClearFilterCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// フィルタを適用するコマンド
        /// </summary>
        public ReactiveCommand ApplyFilterCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CludeTabViewModel()
        {
            // コマンドを設定
            DeleteAllCludeCommand.Subscribe(_ => DeleteAllClude());
            DeleteAllArmorCludeCommand.Subscribe(_ => DeleteAllArmorClude());
            DeleteAllWeaponCludeCommand.Subscribe(_ => DeleteAllWeaponClude());
            ClearFilterCommand.Subscribe(_ => ClearFilter());
            ApplyFilterCommand.Subscribe(_ => ApplyFilter());
        }

        /// <summary>
        /// フィルタを適用
        /// </summary>
        private void ApplyFilter()
        {
            LoadGridData(FilterName.Value, IsCludeOnlyFilter.Value);
        }

        /// <summary>
        /// フィルタを解除
        /// </summary>
        private void ClearFilter()
        {
            LoadGridData();
        }

        /// <summary>
        /// 除外装備設定
        /// </summary>
        /// <param name="equip">装備</param>
        internal void AddExclude(Equipment equip)
        {
            AddExclude(equip.Name, equip.DispName);
        }

        /// <summary>
        /// 除外装備設定
        /// </summary>
        /// <param name="trueName">物理名</param>
        /// <param name="dispName">表示名</param>
        internal void AddExclude(string trueName, string dispName)
        {
            if (string.IsNullOrEmpty(trueName) ||
                Masters.GetEquipByName(trueName).Name != trueName)
            {
                // 装備無しなら何もせず終了
                return;
            }

            // 除外
            Clude? done = Simulator.AddExclude(trueName);

            // 表の該当部分の更新
            CludeGridCellViewModel? target = 
                ShowingArmors.Value.Select(vm => vm.FindByName(trueName))
                .Union(ShowingWeapons.Value.Select(vm => vm.FindByName(trueName)))
                .Where(vm => vm != null)
                .FirstOrDefault();
            target?.ChangeIncludeSilent(false);
            target?.ChangeExcludeSilent(true);

            // ログ表示
            if (done != null)
            {
                SetStatusBar("除外登録完了：" + dispName);
            }
            else
            {
                SetStatusBar(dispName + "は除外できません");
            }
        }

        /// <summary>
        /// 固定装備設定
        /// </summary>
        /// <param name="equip">装備</param>
        internal void AddInclude(Equipment equip)
        {
            AddInclude(equip.Name, equip.DispName);
        }

        /// <summary>
        /// 固定装備設定
        /// </summary>
        /// <param name="trueName">物理名</param>
        /// <param name="dispName">表示名</param>
        internal void AddInclude(string trueName, string dispName)
        {
            if (string.IsNullOrEmpty(trueName) ||
                Masters.GetEquipByName(trueName).Name != trueName)
            {
                // 装備無しなら何もせず終了
                return;
            }

            // 固定
            Clude? done = Simulator.AddInclude(trueName);

            // 表の該当部分の更新
            // 武器は固定しないので処理外
            CludeGridCellViewModel? target = ShowingArmors.Value
                .Select(vm => vm.FindByName(trueName))
                .Where(vm => vm != null)
                .FirstOrDefault();
            target?.ChangeExcludeSilent(false);
            target?.ChangeIncludeSilent(true);

            // 同じ部位の別の固定表示を解除
            EquipKind kind = target?.BaseEquip?.Kind ?? EquipKind.error;
            CludeGridCellViewModel? otherInclude = ShowingArmors.Value
                .Select(vm => vm.FindByKind(kind))
                .Where(vm => vm != null && vm.IsInclude.Value == true && vm.BaseEquip?.Name != target?.BaseEquip?.Name)
                .FirstOrDefault();
            otherInclude?.ChangeIncludeSilent(false);
            otherInclude?.ChangeExcludeSilent(false);

            // ログ表示
            if (done != null)
            {
                SetStatusBar("固定登録完了：" + dispName);
            }
            else
            {
                SetStatusBar(dispName + "は固定できません");
            }
        }

        /// <summary>
        /// 除外・固定の解除
        /// </summary>
        /// <param name="equip">装備</param>
        internal void DeleteClude(Equipment equip)
        {
            DeleteClude(equip.Name, equip.DispName);
        }

        /// <summary>
        /// 除外・固定の解除
        /// </summary>
        /// <param name="trueName">物理名</param>
        /// <param name="dispName">表示名</param>
        internal void DeleteClude(string trueName, string dispName)
        {
            if (string.IsNullOrEmpty(trueName))
            {
                // 装備無しなら何もせず終了
                return;
            }

            // 解除
            Simulator.DeleteClude(trueName);

            // 表の該当部分の更新
            CludeGridCellViewModel? target =
                ShowingArmors.Value.Select(vm => vm.FindByName(trueName))
                .Union(ShowingWeapons.Value.Select(vm => vm.FindByName(trueName)))
                .Where(vm => vm != null)
                .FirstOrDefault();
            target?.ChangeIncludeSilent(false);
            target?.ChangeExcludeSilent(false);

            // ログ表示
            SetStatusBar("除外・固定解除完了：" + dispName);
        }

        /// <summary>
        /// 除外・固定の全解除
        /// </summary>
        private void DeleteAllClude()
        {
            // 解除
            Simulator.DeleteAllClude();

            // 除外固定のマスタをまとめてリロード
            LoadGridData();

            // ログ表示
            SetStatusBar("固定・除外の全解除完了");
        }

        /// <summary>
        /// 防具の除外・固定の全解除
        /// </summary>
        private void DeleteAllArmorClude()
        {
            // 解除
            Simulator.DeleteAllArmorClude();

            // 除外固定のマスタをまとめてリロード
            LoadGridData();

            // ログ表示
            SetStatusBar("固定・除外の全解除完了");
        }

        /// <summary>
        /// 武器の除外・固定の全解除
        /// </summary>
        private void DeleteAllWeaponClude()
        {
            // 解除
            Simulator.DeleteAllWeaponClude();

            // 除外固定のマスタをまとめてリロード
            LoadGridData();

            // ログ表示
            SetStatusBar("固定・除外の全解除完了");
        }

        /// <summary>
        /// 装備のマスタ情報をVMにロード
        /// </summary>
        internal void LoadEquipsForClude()
        {
            LoadGridData();
        }

        /// <summary>
        /// 除外固定の表をリロードする
        /// </summary>
        /// <param name="filterName">文字列でフィルタする場合その文字列</param>
        /// <param name="cludeOnly">設定されているもののみに絞る場合true</param>
        private void LoadGridData(string filterName = "", bool cludeOnly = false)
        {
            LoadArmorGridData(filterName, cludeOnly);
            LoadWeaponGridData(filterName, cludeOnly);
        }

        /// <summary>
        /// 除外固定の表をリロードする(防具)
        /// </summary>
        /// <param name="filterName">文字列でフィルタする場合その文字列</param>
        /// <param name="cludeOnly">設定されているもののみに絞る場合true</param>
        private void LoadArmorGridData(string filterName, bool cludeOnly)
        {
            // 表示対象
            var heads = Masters.Heads;
            var bodys = Masters.Bodys;
            var arms = Masters.Arms;
            var waists = Masters.Waists;
            var legs = Masters.Legs;
            var charms = Masters.Charms.Union(Masters.AdditionalCharms).ToList();
            var decos = Masters.Decos;

            // 簡潔化のためにリスト化(護石と装飾品は特殊処理があるので別枠)
            var armors = new List<Equipment>[]
            {
                heads, bodys, arms, waists, legs
            };

            // 名称フィルタ
            if (!string.IsNullOrWhiteSpace(filterName))
            {
                for (int i = 0; i < armors.Length; i++)
                {
                    armors[i] = armors[i].Where(x => x.DispName.Contains(filterName)).ToList();
                }
                charms = charms.Where(x => x.DispName.Contains(filterName)).ToList();
                decos = decos.Where(x => x.DispName.Contains(filterName)).ToList();
            }

            // 設定フィルタ
            if (cludeOnly)
            {
                for (int i = 0; i < armors.Length; i++)
                {
                    armors[i] = armors[i].Where(x => Masters.Cludes.Where(c => c.Name == x.Name).Any()).ToList();
                }
                charms = charms.Where(x => Masters.Cludes.Where(c => c.Name == x.Name).Any()).ToList();
                decos = decos.Where(x => Masters.Cludes.Where(c => c.Name == x.Name).Any()).ToList();

            }

            // 一覧用の行データ格納場所
            ObservableCollection<CludeGridArmorRowViewModel> rows = new();

            // 存在する仮番号をチェック
            IEnumerable<int> rowNos = new List<int>();
            foreach (var equips in armors)
            {
                rowNos = rowNos.Union(equips.Select(equip => equip.RowNo));
            }

            List<int> rowNoList = rowNos.ToList();
            rowNoList.Sort();

            // 仮番号ごとに行データを作成
            foreach (var rowNo in rowNoList)
            {
                List<IEnumerable<Equipment>> rowNoArmors = new();
                int max = 0;
                foreach (var equips in armors)
                {
                    var rowNoEquips = equips.Where(equip => equip.RowNo == rowNo);
                    rowNoArmors.Add(rowNoEquips);
                    max = Math.Max(max, rowNoEquips.Count());
                }

                // 同じ仮番号があったらその分行を増やす
                for (int i = 0; i < max; i++)
                {
                    CludeGridArmorRowViewModel row = new();
                    var targetReactiveProperties = new ReactivePropertySlim<CludeGridCellViewModel>[]
                    {
                        row.Head, row.Body, row.Arm, row.Waist, row.Leg
                    };
                    for (int j = 0; j < rowNoArmors.Count; j++)
                    {
                        targetReactiveProperties[j].Value = new CludeGridCellViewModel(rowNoArmors[j].ElementAtOrDefault(i));
                    }
                    // 護石と装飾品は単に順番に並べる
                    row.Charm.Value = new CludeGridCellViewModel(charms.ElementAtOrDefault(rows.Count));
                    row.Deco.Value = new CludeGridCellViewModel(decos.ElementAtOrDefault(rows.Count));
                    rows.Add(row);
                }
            }

            // 防具が終わっても護石と装飾品がまだある場合は追加する
            while (rows.Count < Math.Max(charms.Count, decos.Count))
            {
                CludeGridArmorRowViewModel row = new();
                row.Head.Value = new CludeGridCellViewModel(null);
                row.Body.Value = new CludeGridCellViewModel(null);
                row.Arm.Value = new CludeGridCellViewModel(null);
                row.Waist.Value = new CludeGridCellViewModel(null);
                row.Leg.Value = new CludeGridCellViewModel(null);
                row.Charm.Value = new CludeGridCellViewModel(charms.ElementAtOrDefault(rows.Count));
                row.Deco.Value = new CludeGridCellViewModel(decos.ElementAtOrDefault(rows.Count));
                rows.Add(row);
            }

            // 表のリフレッシュ
            ShowingArmors.ChangeCollection(rows);
        }

        /// <summary>
        /// 除外固定の表をリロードする(武器)
        /// </summary>
        /// <param name="filterName">文字列でフィルタする場合その文字列</param>
        /// <param name="cludeOnly">設定されているもののみに絞る場合true</param>
        private void LoadWeaponGridData(string filterName, bool cludeOnly)
        {
            // 表示対象
            var greatSwords = Masters.Weapons.Where(w => w.WeaponType == WeaponType.大剣).ToList();
            var longSwords = Masters.Weapons.Where(w => w.WeaponType == WeaponType.太刀).ToList();
            var swordAndShields = Masters.Weapons.Where(w => w.WeaponType == WeaponType.片手剣).ToList();
            var dualBladeses = Masters.Weapons.Where(w => w.WeaponType == WeaponType.双剣).ToList();
            var lances = Masters.Weapons.Where(w => w.WeaponType == WeaponType.ランス).ToList();
            var gunlances = Masters.Weapons.Where(w => w.WeaponType == WeaponType.ガンランス).ToList();
            var hammers = Masters.Weapons.Where(w => w.WeaponType == WeaponType.ハンマー).ToList();
            var huntingHorns = Masters.Weapons.Where(w => w.WeaponType == WeaponType.狩猟笛).ToList();
            var switchAxes = Masters.Weapons.Where(w => w.WeaponType == WeaponType.スラッシュアックス).ToList();
            var chargeBlades = Masters.Weapons.Where(w => w.WeaponType == WeaponType.チャージアックス).ToList();
            var insectGlaives = Masters.Weapons.Where(w => w.WeaponType == WeaponType.操虫棍).ToList();
            var lightBowguns = Masters.Weapons.Where(w => w.WeaponType == WeaponType.ライトボウガン).ToList();
            var heavyBowguns = Masters.Weapons.Where(w => w.WeaponType == WeaponType.ヘビィボウガン).ToList();
            var bows = Masters.Weapons.Where(w => w.WeaponType == WeaponType.弓).ToList();

            // 簡潔化のためにリスト化
            var weaponLists = new List<Weapon>[]
            {
                greatSwords, longSwords, swordAndShields, dualBladeses, lances, gunlances, hammers, huntingHorns, switchAxes, chargeBlades, insectGlaives, lightBowguns, heavyBowguns, bows
            };

            // 名称フィルタ
            if (!string.IsNullOrWhiteSpace(filterName))
            {
                for (int i = 0; i < weaponLists.Length; i++)
                {
                    weaponLists[i] = weaponLists[i].Where(x => x.DispName.Contains(filterName)).ToList();
                }
            }

            // 設定フィルタ
            if (cludeOnly)
            {
                for (int i = 0; i < weaponLists.Length; i++)
                {
                    weaponLists[i] = weaponLists[i].Where(x => Masters.Cludes.Where(c => c.Name == x.Name).Any()).ToList();
                }
            }

            // 一覧用の行データ格納場所
            ObservableCollection<CludeGridWeaponRowViewModel> rows = new();

            // 存在する仮番号をチェック
            IEnumerable<int> rowNos = new List<int>();
            foreach (var weaponList in weaponLists)
            {
                rowNos = rowNos.Union(weaponList.Select(w => w.RowNo));
            }

            List<int> rowNoList = rowNos.ToList();
            rowNoList.Sort();

            // 仮番号ごとに行データを作成
            foreach (var rowNo in rowNoList)
            {
                List<IEnumerable<Weapon>> rowNoWeapons = new();
                int max = 0;
                foreach (var weaponList in weaponLists)
                {
                    var rowNoEquips = weaponList.Where(w => w.RowNo == rowNo);
                    rowNoWeapons.Add(rowNoEquips);
                    max = Math.Max(max, rowNoEquips.Count());
                }

                // 同じ仮番号があったらその分行を増やす
                for (int i = 0; i < max; i++)
                {
                    CludeGridWeaponRowViewModel row = new();
                    var targetReactiveProperties = new ReactivePropertySlim<CludeGridCellViewModel>[]
                    {
                        row.GreatSword, row.LongSword, row.SwordAndShield, row.DualBlades, row.Lance, row.Gunlance, row.Hammer,
                        row.HuntingHorn, row.SwitchAxe, row.ChargeBlade, row.InsectGlaive, row.LightBowgun, row.HeavyBowgun, row.Bow
                    };
                    for (int j = 0; j < rowNoWeapons.Count; j++)
                    {
                        targetReactiveProperties[j].Value = new CludeGridCellViewModel(rowNoWeapons[j].ElementAtOrDefault(i));
                    }
                    rows.Add(row);
                }
            }

            // 表のリフレッシュ
            ShowingWeapons.ChangeCollection(rows);
        }
    }
}
