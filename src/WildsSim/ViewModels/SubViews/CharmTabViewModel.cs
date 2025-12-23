using Reactive.Bindings;
using SimModel.Config;
using SimModel.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WildsSim.Config;
using WildsSim.Util;
using WildsSim.ViewModels.BindableWrapper;
using WildsSim.ViewModels.Controls;

namespace WildsSim.ViewModels.SubViews
{
    internal class CharmTabViewModel : ChildViewModelBase
    {
        /// <summary>
        /// 追加護石のスキル個数
        /// </summary>
        private int MaxCharmSkillCount { get; } = LogicConfig.Instance.MaxCharmSkillCount;

        /// <summary>
        /// スロットの最大の大きさ
        /// </summary>
        private int MaxSlotSize { get; } = ViewConfig.Instance.MaxSlotSize;

        /// <summary>
        /// 護石画面のスキル選択部品のVM
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<SkillSelectorViewModel>> CharmSkillSelectorVMs { get; } = new();

        /// <summary>
        /// 表示用護石一覧
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<BindableCharm>> Charms { get; } = new();

        /// <summary>
        /// スロット1
        /// </summary>
        public ReactivePropertySlim<string> Slot1 { get; } = new();

        /// <summary>
        /// スロット2
        /// </summary>
        public ReactivePropertySlim<string> Slot2 { get; } = new();

        /// <summary>
        /// スロット3
        /// </summary>
        public ReactivePropertySlim<string> Slot3 { get; } = new();

        /// <summary>
        /// スロット1タイプ
        /// </summary>
        public ReactivePropertySlim<string> SlotType1 { get; } = new();

        /// <summary>
        /// スロット2タイプ
        /// </summary>
        public ReactivePropertySlim<string> SlotType2 { get; } = new();

        /// <summary>
        /// スロット3タイプ
        /// </summary>
        public ReactivePropertySlim<string> SlotType3 { get; } = new();

        /// <summary>
        /// スロットサイズの選択肢
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<string>> SlotSizeMaster { get; } = new();

        /// <summary>
        /// スロット種類の選択肢
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<string>> SlotTypeMaster { get; } = new();

        /// <summary>
        /// 護石追加コマンド
        /// </summary>
        public ReactiveCommand AddCharmCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// スロット1クリアコマンド
        /// </summary>
        public ReactiveCommand ClearSlot1Command { get; } = new ReactiveCommand();

        /// <summary>
        /// スロット2クリアコマンド
        /// </summary>
        public ReactiveCommand ClearSlot2Command { get; } = new ReactiveCommand();

        /// <summary>
        /// スロット3クリアコマンド
        /// </summary>
        public ReactiveCommand ClearSlot3Command { get; } = new ReactiveCommand();

        /// <summary>
        /// ドラッグコマンド
        /// </summary>
        public ReactiveCommand RowChangedCommand { get; private set; } = new();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CharmTabViewModel()
        {
            // 護石画面のスキル選択部品準備
            ObservableCollection<SkillSelectorViewModel> charmSelectorVMs = new();
            for (int i = 0; i < MaxCharmSkillCount; i++)
            {
                charmSelectorVMs.Add(new SkillSelectorViewModel());
            }
            CharmSkillSelectorVMs.ChangeCollection(charmSelectorVMs);

            // スロットの選択肢を生成し、画面に反映
            ObservableCollection<string> slots = new();
            for (int i = 0; i <= MaxSlotSize; i++)
            {
                slots.Add(i.ToString());
            }
            SlotSizeMaster.Value = slots;
            Slot1.Value = "0";
            Slot2.Value = "0";
            Slot3.Value = "0";
            SlotTypeMaster.Value = ["防具用", "武器用"];
            SlotType1.Value = "防具用";
            SlotType2.Value = "防具用";
            SlotType3.Value = "防具用";

            // コマンドを設定
            AddCharmCommand.Subscribe(_ => AddCharm());
            RowChangedCommand.Subscribe(indexpair => RowChanged(indexpair as (int, int)?));
            ClearSlot1Command.Subscribe(_ => ClearSlot(1));
            ClearSlot2Command.Subscribe(_ => ClearSlot(2));
            ClearSlot3Command.Subscribe(_ => ClearSlot(3));
        }

        /// <summary>
        /// 護石追加
        /// </summary>
        internal void AddCharm()
        {
            Equipment charm = new();
            charm.Name = Guid.NewGuid().ToString();
            charm.Kind = EquipKind.charm;

            // スキルを整理
            List<Skill> skills = new();
            foreach (var vm in CharmSkillSelectorVMs.Value)
            {
                if (Masters.IsSkillName(vm.SkillName.Value))
                {
                    skills.Add(new Skill(vm.SkillName.Value, vm.SkillLevel.Value));
                }
            }
            charm.Skills = skills;

            // スロットを整理
            charm.Slot1 = int.Parse(Slot1.Value);
            charm.Slot2 = int.Parse(Slot2.Value);
            charm.Slot3 = int.Parse(Slot3.Value);
            switch (SlotType1.Value)
            {
                case "防具用":
                    charm.SlotType1 = 0;
                    break;
                case "武器用":
                    charm.SlotType1 = 1;
                    break;
                case "両対応":
                    charm.SlotType1 = 2;
                    break;
                default:
                    break;
            }
            switch (SlotType2.Value)
            {
                case "防具用":
                    charm.SlotType2 = 0;
                    break;
                case "武器用":
                    charm.SlotType2 = 1;
                    break;
                case "両対応":
                    charm.SlotType2 = 2;
                    break;
                default:
                    break;
            }
            switch (SlotType3.Value)
            {
                case "防具用":
                    charm.SlotType3 = 0;
                    break;
                case "武器用":
                    charm.SlotType3 = 1;
                    break;
                case "両対応":
                    charm.SlotType3 = 2;
                    break;
                default:
                    break;
            }

            // 護石の表示名を設定
            charm.SetCharmDispName();

            // 護石追加
            Simulator.AddCharm(charm);

            // 装備マスタをリロード
            MainVM.LoadEquips();

            // ログ表示
            SetStatusBar("護石追加完了：" + charm.DispName);
        }

        /// <summary>
        /// 順番入れ替え
        /// </summary>
        /// <param name="indexpair">(int dropIndex, int targetIndex)</param>
        private void RowChanged((int dropIndex, int targetIndex)? indexpair)
        {
            if (indexpair != null && indexpair.Value.dropIndex >= 0 && indexpair.Value.targetIndex >= 0)
            {
                Charms.Value.Move(indexpair.Value.dropIndex, indexpair.Value.targetIndex);
                Simulator.MoveCharm(indexpair.Value.dropIndex, indexpair.Value.targetIndex);
            }
        }

        /// <summary>
        /// 護石削除(BindableCharmから呼び出し)
        /// </summary>
        /// <param name="charm">護石</param>
        internal void DeleteCharm(Equipment charm)
        {
            if (charm == null)
            {
                // 装備無しなら何もせず終了
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                $"護石「{charm.DispName}」を削除します。\nよろしいですか？",
                "護石削除",
                MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            // この護石を使っているマイセットがあったら再度確認する
            if (Masters.MySets.Where(set => charm.Name.Equals(set.Charm.Name)).Any())
            {
                MessageBoxResult setConfirm = MessageBox.Show(
                    $"護石「{charm.DispName}」を利用しているマイセットが存在します。\n本当に削除してよろしいですか？\n(該当のマイセットも同時に削除されます。)",
                    "護石削除",
                    MessageBoxButton.YesNo);
                if (setConfirm != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            // 護石削除
            Simulator.DeleteCharm(charm);

            // マスタをリロード
            // マイセットが変更になる可能性があるためそちらもリロード
            MySetTabVM.LoadMySets();
            MainVM.LoadEquips();

            // ログ表示
            SetStatusBar("護石削除完了：" + charm.DispName);
        }

        /// <summary>
        /// スロット入力をクリア
        /// </summary>
        /// <param name="no">クリア対象</param>
        private void ClearSlot(int no)
        {
            switch (no)
            {
                case 1:
                    Slot1.Value = "0";
                    SlotType1.Value = "防具用";
                    break;
                case 2:
                    Slot2.Value = "0";
                    SlotType2.Value = "防具用";
                    break;
                case 3:
                    Slot3.Value = "0";
                    SlotType3.Value = "防具用";
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 装備関係のマスタ情報をVMにロード
        /// </summary>
        internal void LoadEquipsForCharm()
        {
            // 護石画面用のVMの設定
            ObservableCollection<BindableCharm> charmList = BindableCharm.BeBindableList(Masters.AdditionalCharms);
            Charms.ChangeCollection(charmList);
        }
    }
}
