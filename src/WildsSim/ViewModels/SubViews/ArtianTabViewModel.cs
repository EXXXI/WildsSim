using Reactive.Bindings;
using SimModel.Config;
using SimModel.Domain;
using SimModel.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WildsSim.Util;
using WildsSim.ViewModels.BindableWrapper;
using WildsSim.ViewModels.Controls;

namespace WildsSim.ViewModels.SubViews
{
    /// <summary>
    /// アーティア設定タブのVM
    /// </summary>
    internal class ArtianTabViewModel : ChildViewModelBase
    {
        /// <summary>
        /// 表示用アーティア一覧
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<BindableArtian>> Artians { get; } = new();

        /// <summary>
        /// 武器種の選択肢
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<string>> WeaponTypes { get; } = new();

        /// <summary>
        /// 選択中武器種
        /// </summary>
        public ReactivePropertySlim<string> SelectedWeaponType { get; } = new();

        /// <summary>
        /// 名前
        /// </summary>
        public ReactivePropertySlim<string> ArtianName { get; } = new(string.Empty);

        /// <summary>
        /// アーティア画面のスキル選択部品のVM
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<SkillSelectorViewModel>> ArtianSkillSelectorVMs { get; } = new();

        /// <summary>
        /// アーティア追加コマンド
        /// </summary>
        public ReactiveCommand AddArtianCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// ドラッグコマンド
        /// </summary>
        public ReactiveCommand RowChangedCommand { get; private set; } = new();


        public ArtianTabViewModel()
        {
            // 武器種の選択肢を生成し、画面に反映
            WeaponTypes.Value = new(Enum.GetNames(typeof(WeaponType)).Where(s => s != WeaponType.指定なし.ToString()));
            SelectedWeaponType.Value = WeaponTypes.Value[0];

            // アーティア画面のスキル選択部品準備
            ObservableCollection<SkillSelectorViewModel> artianSelectorVMs = new();
            artianSelectorVMs.Add(new SkillSelectorViewModel(SkillSelectorKind.ArtianGroup));
            artianSelectorVMs.Add(new SkillSelectorViewModel(SkillSelectorKind.ArtianSeries));
            ArtianSkillSelectorVMs.ChangeCollection(artianSelectorVMs);

            // コマンドを設定
            AddArtianCommand.Subscribe(_ => AddArtian());
            RowChangedCommand.Subscribe(indexpair => RowChanged(indexpair as (int, int)?));
        }

        /// <summary>
        /// アーティア追加
        /// </summary>
        internal void AddArtian()
        {
            Weapon artian = new Weapon();
            artian.InitArtian();
            artian.Name = Guid.NewGuid().ToString();
            artian.WeaponType = (WeaponType)Enum.Parse(typeof(WeaponType), SelectedWeaponType.Value);
            string dispName = string.Empty;
            if (string.IsNullOrWhiteSpace(ArtianName.Value))
            {
                for (int i = 1; string.IsNullOrEmpty(dispName); i++)
                {
                    string tryName = $"アーティア{artian.WeaponType}{i}";
                    if (Masters.Artians.Any(a => a.DispName == tryName))
                    {
                        // 既に存在する名前なので次へ
                        continue;
                    }
                    dispName = tryName;
                }
            }
            else
            {
                dispName = ArtianName.Value;
            }
            artian.DispName = dispName;
            List<Skill> skills = new List<Skill>();
            foreach (var vm in ArtianSkillSelectorVMs.Value)
            {
                if (Masters.Skills.Any(s => s.Name == vm.SkillName.Value))
                {
                    string skill = vm.SkillName.Value;
                    int level = vm.SkillLevel.Value;
                    skills.Add(new Skill(skill, level));
                }
            }
            artian.Skills = skills;

            // アーティア追加
            Simulator.AddArtian(artian);

            // 装備マスタをリロード
            MainVM.LoadEquips();

            // ログ表示
            SetStatusBar("アーティア追加完了：" + artian.DispName);
        }

        /// <summary>
        /// 順番入れ替え
        /// </summary>
        /// <param name="indexpair">(int dropIndex, int targetIndex)</param>
        private void RowChanged((int dropIndex, int targetIndex)? indexpair)
        {
            if (indexpair != null)
            {
                Artians.Value.Move(indexpair.Value.dropIndex, indexpair.Value.targetIndex);
                Simulator.MoveArtian(indexpair.Value.dropIndex, indexpair.Value.targetIndex);
            }
        }

        /// <summary>
        /// アーティア削除(BindableArtianから呼び出し)
        /// </summary>
        /// <param name="artian">アーティア</param>
        internal void DeleteArtian(Weapon artian)
        {
            if (artian == null)
            {
                // 装備無しなら何もせず終了
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                $"アーティア武器「{artian.DispName}」を削除します。\nよろしいですか？",
                "アーティア武器削除",
                MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            // この護石を使っているマイセットがあったら再度確認する
            if (Masters.MySets.Where(set => artian.Name.Equals(set.Weapon.Name)).Any())
            {
                MessageBoxResult setConfirm = MessageBox.Show(
                    $"アーティア武器「 {artian.DispName}」を利用しているマイセットが存在します。\n本当に削除してよろしいですか？\n(該当のマイセットも同時に削除されます。)",
                    "アーティア武器削除",
                    MessageBoxButton.YesNo);
                if (setConfirm != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            // アーティア武器削除
            Simulator.DeleteArtian(artian);

            // マスタをリロード
            // マイセットが変更になる可能性があるためそちらもリロード
            MySetTabVM.LoadMySets();
            MainVM.LoadEquips();

            // ログ表示
            SetStatusBar("アーティア削除完了：" + artian.DispName);
        }


        /// <summary>
        /// 装備関係のマスタ情報をVMにロード
        /// </summary>
        internal void LoadEquipsForArtian()
        {
            // 護石画面用のVMの設定
            ObservableCollection<BindableArtian> artianList = BindableArtian.BeBindableList(Masters.Artians);
            Artians.ChangeCollection(artianList);
        }
    }
}
