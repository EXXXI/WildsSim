using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using WildsSim.Config;
using WildsSim.Util;
using WildsSim.ViewModels.Controls;
using SimModel.Domain;
using SimModel.Model;
using SimModel.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace WildsSim.ViewModels.SubViews
{
    /// <summary>
    /// スキル選択画面VM
    /// </summary>
    internal class SkillSelectTabViewModel : ChildViewModelBase
    {
        // TODO: 名称指定か何かにしたい
        /// <summary>
        /// 追加スキル検索のサブタブIndex
        /// </summary>
        const int ExSkillTabIndex = 1;

        /// <summary>
        /// 武器指定方式のスロットのみ指定の選択肢
        /// </summary>
        const string SlotOnlyString = "武器はスロットのみ指定";

        /// <summary>
        /// 武器指定方式の武器検索の選択肢
        /// </summary>
        const string CalcWeaponString = "武器も計算に含める";

        /// <summary>
        /// 武器指定なしの選択肢
        /// </summary>
        const string SearchWeaponString = "指定しない(全武器から検索する)";


        /// <summary>
        /// スロットの最大の大きさ
        /// </summary>
        private int MaxSlotSize { get; } = ViewConfig.Instance.MaxSlotSize;

        /// <summary>
        /// デフォルトの頑張り度
        /// </summary>
        private string DefaultLimit { get; } = ViewConfig.Instance.DefaultLimit;

        /// <summary>
        /// 追加スキル検索結果用VM
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<SkillAdderViewModel>> ExtraSkillVMs { get; } = new();

        /// <summary>
        /// 最近使ったスキル用VM
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<SkillAdderViewModel>> RecentSkillVMs { get; } = new();

        /// <summary>
        /// スキルカテゴリ表示部品のVM
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<SkillLevelSelectorContainerViewModel>> SkillContainerVMs { get; } = new();

        /// <summary>
        /// マイ検索条件表示部品のVM
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<MyConditionRowViewModel>> MyConditionVMs { get; } = new();

        // TODO:要変更
        /// <summary>
        /// 武器スロ指定
        /// </summary>
        public ReactivePropertySlim<string> WeaponSlots { get; } = new();

        /// <summary>
        /// 防御力指定
        /// </summary>
        public ReactivePropertySlim<string> Def { get; } = new(string.Empty);

        /// <summary>
        /// 火耐性指定
        /// </summary>
        public ReactivePropertySlim<string> Fire { get; } = new(string.Empty);

        /// <summary>
        /// 水耐性指定
        /// </summary>
        public ReactivePropertySlim<string> Water { get; } = new(string.Empty);

        /// <summary>
        /// 雷耐性指定
        /// </summary>
        public ReactivePropertySlim<string> Thunder { get; } = new(string.Empty);

        /// <summary>
        /// 氷耐性指定
        /// </summary>
        public ReactivePropertySlim<string> Ice { get; } = new(string.Empty);

        /// <summary>
        /// 龍耐性指定
        /// </summary>
        public ReactivePropertySlim<string> Dragon { get; } = new(string.Empty);

        /// <summary>
        /// 頑張り度(検索件数)
        /// </summary>
        public ReactivePropertySlim<string> Limit { get; } = new();

        /// <summary>
        /// 選択中タブのIndex
        /// </summary>
        public ReactivePropertySlim<int> SelectedTabIndex { get; } = new();

        // TODO: 要変更
        /// <summary>
        /// スロット選択の選択肢
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<string>> SlotMaster { get; } = new();

        /// <summary>
        /// 武器指定方式の選択肢
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<string>> CalcWeaponMaster { get; } = new();

        /// <summary>
        /// 武器指定方式の選択中選択肢
        /// </summary>
        public ReactivePropertySlim<string> CalcWeapon { get; } = new();

        /// <summary>
        /// 武器を計算に入れるフラグ
        /// </summary>
        public ReactivePropertySlim<bool> IsCalcWeapon { get; } = new();

        /// <summary>
        /// 武器はスロットのみ指定するフラグ
        /// </summary>
        public ReactivePropertySlim<bool> IsSlotOnly { get; } = new();

        /// <summary>
        /// スロット武器選択の選択肢
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<string>> SlotWeapons { get; } = new();

        /// <summary>
        /// 選択中スロット武器
        /// </summary>
        public ReactivePropertySlim<string> SelectedSlotWeapon { get; } = new();

        /// <summary>
        /// 武器種の選択肢
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<string>> WeaponTypes { get; } = new();

        /// <summary>
        /// 選択中武器種
        /// </summary>
        public ReactivePropertySlim<string> SelectedWeaponType { get; } = new();

        /// <summary>
        /// 武器の選択肢
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<string>> Weapons { get; } = new();

        /// <summary>
        /// 選択中武器
        /// </summary>
        public ReactivePropertySlim<string> SelectedWeapon { get; } = new();

        /// <summary>
        /// 最低攻撃力
        /// </summary>
        public ReactivePropertySlim<string> MinAttack { get; } = new();

        /// <summary>
        /// 検索コマンド
        /// </summary>
        public AsyncReactiveCommand SearchCommand { get; private set; }

        /// <summary>
        /// 追加スキル検索コマンド
        /// </summary>
        public AsyncReactiveCommand SearchExtraSkillCommand { get; private set; }

        /// <summary>
        /// 検索条件クリアコマンド
        /// </summary>
        public ReactiveCommand ClearAllCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// マイ検索条件追加コマンド
        /// </summary>
        public ReactiveCommand AddMyConditionCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SkillSelectTabViewModel()
        {

            // スキル選択部品を配置
            SkillContainerVMs.ChangeCollection(new ObservableCollection<SkillLevelSelectorContainerViewModel>(
                Masters.Skills
                    .GroupBy(s => s.Category)
                    .Select(g => new SkillLevelSelectorContainerViewModel(g.Key, g))
            ));

            // 武器指定方式の選択肢を生成し、画面に反映
            CalcWeaponMaster.Value = new(){ SlotOnlyString, CalcWeaponString };
            CalcWeapon.Value = SlotOnlyString;

            // スロット武器選択の選択肢を生成し、画面に反映
            SlotWeapons.Value = new(Masters.Weapons.
                Where(w => w.WeaponType == WeaponType.指定なし).
                Select(w => w.Name).
                ToList());
            SelectedSlotWeapon.Value = SlotWeapons.Value[0];

            // 武器種の選択肢を生成し、画面に反映
            WeaponTypes.Value = new(Enum.GetNames(typeof(WeaponType)).Where(s => s != WeaponType.指定なし.ToString()));
            SelectedWeaponType.Value = WeaponTypes.Value[0];

            // スロットの選択肢を生成し、画面に反映
            ObservableCollection<string> slots = new();
            for (int i = 0; i <= MaxSlotSize; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    for (int k = 0; k <= j; k++)
                    {
                        slots.Add(string.Join('-', i, j, k));
                    }
                }
            }
            SlotMaster.Value = slots;
            WeaponSlots.Value = "0-0-0";

            // 頑張り度を設定
            Limit.Value = DefaultLimit;

            // 最近使ったスキル読み込み
            LoadRecentSkills();

            // マイ検索条件
            LoadMyCondition();

            // コマンドを設定
            ReadOnlyReactivePropertySlim<bool> isFree = MainVM.IsFree;
            SearchCommand = isFree.ToAsyncReactiveCommand().WithSubscribe(async () => await Search()).AddTo(Disposable);
            SearchExtraSkillCommand = isFree.ToAsyncReactiveCommand().WithSubscribe(async () => await SearchExtraSkill()).AddTo(Disposable);
            ClearAllCommand.Subscribe(_ => ClearSearchCondition());
            AddMyConditionCommand.Subscribe(_ => AddMyCondition());
            CalcWeapon.Subscribe(_ => ChangeIsCalcWeapon());
            SelectedWeaponType.Subscribe(_ => ChangeWeapons());
        }

        /// <summary>
        /// 検索
        /// </summary>
        /// <returns>Task</returns>
        private async Task Search()
        {
            // 頑張り度を整理
            int searchLimit = ParseUtil.Parse(Limit.Value, int.Parse(DefaultLimit));

            // 検索条件を整理
            SearchCondition condition = MakeCondition();

            // 開始ログ表示
            SetStatusBar("検索開始・・・");

            // ビジーフラグ
            IsBusy.Value = true;
            MainVM.IsIndeterminate.Value = true;

            // 検索
            List<EquipSet> result = await Task.Run(() => Simulator.Search(condition, searchLimit));

            // ビジーフラグ解除
            IsBusy.Value = false;
            MainVM.IsIndeterminate.Value = false;

            // 最近使ったスキル再読み込み
            LoadRecentSkills();

            // 完了ログ表示
            if (Simulator.IsCanceling)
            {
                SetStatusBar("処理中断：中断時の検索状態を表示します");
            }
            else
            {
                SetStatusBar($"検索完了：{result.Count}件");
            }

            // 検索結果画面に結果を表示
            SimulatorTabVM.ShowSearchResult(result, Simulator.IsCanceling || !Simulator.IsSearchedAll, searchLimit);
            MainVM.ShowSimulatorTab();
        }

        /// <summary>
        /// 追加スキル検索
        /// </summary>
        /// <returns>Task</returns>
        private async Task SearchExtraSkill()
        {
            // 開始ログ表示
            SetStatusBar("追加スキル検索開始・・・");

            // ビジーフラグ
            IsBusy.Value = true;

            // 追加スキル検索
            SearchCondition condition = MakeCondition();
            List<Skill> result = await Task.Run(() => Simulator.SearchExtraSkill(condition, MainVM.Progress));
            MainVM.Progress.Value = 0;

            // 追加スキル表示用VMをセット
            var groups = result.GroupBy(skill => skill.Name);
            ExtraSkillVMs.ChangeCollection(new ObservableCollection<SkillAdderViewModel>(
                groups.Select(group => new SkillAdderViewModel(group.Key, group.Select(skill => skill.Level)))));

            // ビジーフラグ解除
            IsBusy.Value = false;

            // ログ表示
            if (Simulator.IsCanceling)
            {
                SetStatusBar("処理中断：中断時の検索状態を表示します");
            }
            else
            {
                SetStatusBar("追加スキル検索完了");
            }

            // サブタブを追加スキル検索結果に
            SelectedTabIndex.Value = ExSkillTabIndex;
        }

        /// <summary>
        /// 検索条件リセット
        /// </summary>
        private void ClearSearchCondition()
        {
            MessageBoxResult result = MessageBox.Show(
                $"選択中の検索条件を全てリセットします。\nよろしいですか？",
                "検索条件リセット",
                MessageBoxButton.YesNo);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            foreach (var vm in SkillContainerVMs.Value)
            {
                vm.ClearAll();
            }
            WeaponSlots.Value = "0-0-0";
            Def.Value = string.Empty;
            Fire.Value = string.Empty;
            Water.Value = string.Empty;
            Thunder.Value = string.Empty;
            Ice.Value = string.Empty;
            Dragon.Value = string.Empty;
        }

        /// <summary>
        /// マイ検索条件の(再)読み込み
        /// </summary>
        public void LoadMyCondition()
        {
            List<SearchCondition> conditions = Masters.MyConditions;
            MyConditionVMs.ChangeCollection(new ObservableCollection<MyConditionRowViewModel>(
                Masters.MyConditions.Select(condition => new MyConditionRowViewModel(condition))
            ));
        }

        /// <summary>
        /// 最近使ったスキルの(再)読み込み
        /// </summary>
        private void LoadRecentSkills()
        {
            var recentSkills = Masters.RecentSkillNames.Join(
                Masters.Skills, r => r, s => s.Name,
                (r, s) => new
                {
                    Name = s.Name,
                    Range = Enumerable.Range(1, s.Level)
                });

            RecentSkillVMs.ChangeCollection(new ObservableCollection<SkillAdderViewModel>(
                recentSkills.Select(skill => new SkillAdderViewModel(skill.Name,skill.Range))));
        }

        /// <summary>
        /// スキル選択に引数指定のスキルを適用
        /// </summary>
        /// <param name="name">スキル名</param>
        /// <param name="level">レベル</param>
        internal void AddSkill(string name, int level)
        {
            var isSuccess = SkillContainerVMs.Value
                .Select(vm => vm.TryAddSkill(name, level))
                .Contains(true);
        }

        /// <summary>
        /// 引数指定のマイセットのスキルを検索条件に反映
        /// </summary>
        /// <param name="mySet">マイセット</param>
        internal void InputMySetCondition(EquipSet? mySet)
        {
            if (mySet == null)
            {
                // マイセットが空の場合何もせず終了
                return;
            }

            // 各スキル選択部品に適用を試みる
            foreach (var vm in SkillContainerVMs.Value)
            {
                vm.ClearAll();
                vm.TryAddSkill(mySet.Skills);
            }

            // スロット情報反映
            WeaponSlots.Value = mySet.WeaponSlotDisp;
        }

        /// <summary>
        /// マイ検索条件をスキル選択へ適用
        /// </summary>
        /// <param name="condition">マイ検索条件</param>
        internal void ApplyMyCondition(SearchCondition condition)
        {
            // スキル
            foreach (var vm in SkillContainerVMs.Value)
            {
                vm.ClearAll();
                vm.TryAddSkill(condition.Skills);
            }

            // スロット情報反映
            WeaponSlots.Value = condition.WeaponSlot1 + "-" + condition.WeaponSlot2 + "-" + condition.WeaponSlot3;

            // 防御力・耐性を反映
            Def.Value = condition.Def?.ToString() ?? string.Empty;
            Fire.Value = condition.Fire?.ToString() ?? string.Empty;
            Water.Value = condition.Water?.ToString() ?? string.Empty;
            Thunder.Value = condition.Thunder?.ToString() ?? string.Empty;
            Ice.Value = condition.Ice?.ToString() ?? string.Empty;
            Dragon.Value = condition.Dragon?.ToString() ?? string.Empty;

            // ログ表示
            SetStatusBar($"マイ検索条件反映完了：{condition.DispName}");
        }

        /// <summary>
        /// マイ検索条件を追加
        /// </summary>
        private void AddMyCondition()
        {
            SearchCondition condition = MakeCondition();
            string condName = string.Empty;
            bool hasSameName = true;
            for (int i = 1; hasSameName; i++)
            {
                condName = "検索条件" + i;
                hasSameName = Masters.MyConditions.Any(cond => cond.DispName == condName);
            }
            condition.DispName = condName;
            Simulator.AddMyCondition(condition);
            LoadMyCondition();

            // ログ表示
            SetStatusBar($"マイ検索条件登録完了：{condition.DispName}");
        }

        /// <summary>
        /// 武器指定方式の選択肢を元に、表示を切り替える
        /// </summary>
        private void ChangeIsCalcWeapon()
        {
            if (CalcWeapon.Value == SlotOnlyString)
            {
                IsCalcWeapon.Value = false;
                IsSlotOnly.Value = true;
            }
            else
            {
                IsCalcWeapon.Value = true;
                IsSlotOnly.Value = false;
            }
        }

        /// <summary>
        /// 武器種の選択をもとに、武器一覧を切り替える
        /// </summary>
        private void ChangeWeapons()
        {
            string selected = SelectedWeaponType.Value;
            ObservableCollection<string> weapons = new();
            weapons.Add(SearchWeaponString);
            weapons.AddRange(Masters.Weapons.Where(w => w.WeaponType.ToString() == selected).Select(w => w.Name).ToList());
            Weapons.Value = weapons;
            SelectedWeapon.Value = weapons[0];
        }

        /// <summary>
        /// 検索条件インスタンスを作成
        /// </summary>
        /// <returns>検索条件</returns>
        private SearchCondition MakeCondition()
        {
            SearchCondition condition = new();

            // スキル条件を整理
            condition.Skills = SkillContainerVMs.Value
                .SelectMany(vm => vm.SelectedSkills())
                .ToList();

            // 武器スロ条件を整理
            string[] splited = WeaponSlots.Value.Split('-');
            condition.WeaponSlot1 = int.Parse(splited[0]);
            condition.WeaponSlot2 = int.Parse(splited[1]);
            condition.WeaponSlot3 = int.Parse(splited[2]);

            // 防御力・耐性を整理
            condition.Def = ParseOrNull(Def.Value);
            condition.Fire = ParseOrNull(Fire.Value);
            condition.Water = ParseOrNull(Water.Value);
            condition.Thunder = ParseOrNull(Thunder.Value);
            condition.Ice = ParseOrNull(Ice.Value);
            condition.Dragon = ParseOrNull(Dragon.Value);

            // 名前・ID
            condition.ID = Guid.NewGuid().ToString();
            condition.DispName = "検索条件";

            return condition;
        }

        /// <summary>
        /// int.Parseを実施
        /// </summary>
        /// <param name="param">Parsestring</param>
        /// <returns>Parseしたint　変換できなかった場合null</returns>
        private int? ParseOrNull(string param)
        {
            if (int.TryParse(param, out int result))
            {
                return result;
            }
            return null;
        }
    }
}
