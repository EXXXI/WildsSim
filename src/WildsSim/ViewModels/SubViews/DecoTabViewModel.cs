using Reactive.Bindings;
using SimModel.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WildsSim.Util;
using WildsSim.ViewModels.Controls;

namespace WildsSim.ViewModels.SubViews
{
    /// <summary>
    /// 装飾品設定画面のVM
    /// </summary>
    internal class DecoTabViewModel : ChildViewModelBase
    {
        /// <summary>
        /// 装飾品所持数変更部品のカテゴリ毎コンテナ
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<DecoCountSelectorContainerViewModel>> Containers { get; } = new();

        /// <summary>
        /// 装飾品名フィルタ用入力
        /// </summary>
        public ReactivePropertySlim<string> FilterName { get; } = new(string.Empty);

        /// <summary>
        /// スキル名フィルタ用入力
        /// </summary>
        public ReactivePropertySlim<string> FilterSkillName { get; } = new(string.Empty);

        /// <summary>
        /// フィルタクリアコマンド
        /// </summary>
        public ReactiveCommand ClearFilterCommand { get; } = new();

        /// <summary>
        /// フィルタ適用コマンド
        /// </summary>
        public ReactiveCommand ApplyFilterCommand { get; } = new();

        /// <summary>
        /// 全装飾品0指定コマンド
        /// </summary>
        public ReactiveCommand SetAll0Command { get; } = new();

        /// <summary>
        /// 全装飾品7指定コマンド
        /// </summary>
        public ReactiveCommand SetAll7Command { get; } = new();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public DecoTabViewModel()
        {
            // VMをロード
            LoadContainers();

            // コマンドを設定
            ClearFilterCommand.Subscribe(() => ClearFilter());
            ApplyFilterCommand.Subscribe(() => ApplyFilter());
            SetAll0Command.Subscribe(() => SetAll(0));
            SetAll7Command.Subscribe(() => SetAll(7));
        }

        /// <summary>
        /// 全装飾品の個数を変更する
        /// </summary>
        /// <param name="count">変更する値</param>
        private void SetAll(int count)
        {
            // 一旦フィルタをリセット
            LoadContainers();

            // 個数を反映
            foreach (var container in Containers.Value)
            {
                container.SetAllCount(count);
            }
        }

        /// <summary>
        /// フィルタクリア
        /// </summary>
        private void ClearFilter()
        {
            LoadContainers();
        }

        /// <summary>
        /// フィルタ適用
        /// </summary>
        private void ApplyFilter()
        {
            LoadContainers(FilterName.Value, FilterSkillName.Value);
        }

        /// <summary>
        /// 装飾品個数変更部品をリロード
        /// </summary>
        /// <param name="filterName">装飾品名でフィルタする場合その文字列</param>
        /// <param name="filterSkillName">スキル名でフィルタする場合その文字列</param>
        private void LoadContainers(string filterName = "", string filterSkillName = "")
        {
            // 表示対象
            var decos = Masters.Decos;

            // 名称フィルタ適用
            if (!string.IsNullOrWhiteSpace(filterName))
            {
                decos = decos.Where(d => d.DispName.Contains(filterName)).ToList();
            }

            // スキル名フィルタ適用
            if (!string.IsNullOrWhiteSpace(filterSkillName))
            {
                decos = decos.Where(d => d.Skills.Where(s => s.Name.Contains(filterSkillName)).Any()).ToList();
            }

            // 表示
            var groups = decos.GroupBy(d => d.DecoCateory);
            ObservableCollection<DecoCountSelectorContainerViewModel> containers = new();
            foreach (var group in groups)
            {
                containers.Add(new DecoCountSelectorContainerViewModel(group.Key, group));
            }
            Containers.ChangeCollection(containers);
        }
    }
}
