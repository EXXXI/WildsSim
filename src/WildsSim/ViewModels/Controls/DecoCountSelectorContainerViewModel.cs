using Reactive.Bindings;
using SimModel.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WildsSim.Util;

namespace WildsSim.ViewModels.Controls
{
    /// <summary>
    /// 装飾品所持数変更部品のカテゴリごとのコンテナ
    /// </summary>
    internal class DecoCountSelectorContainerViewModel : ChildViewModelBase
    {
        /// <summary>
        /// 装飾品所持数変更部品
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<DecoCountSelectorViewModel>> SelectorVMs { get; set; } = new();

        /// <summary>
        /// カテゴリ名
        /// </summary>
        public ReactivePropertySlim<string> Category { get; set; } = new();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="category">カテゴリ名</param>
        /// <param name="decos">装飾品</param>
        public DecoCountSelectorContainerViewModel(string category, IEnumerable<Deco> decos)
        {
            Category.Value = category;

            ObservableCollection<DecoCountSelectorViewModel> vms = new();
            foreach (var deco in decos)
            {
                vms.Add(new DecoCountSelectorViewModel(deco));
            }
            SelectorVMs.ChangeCollection(vms);
        }

        /// <summary>
        /// 配下の装飾品所持数をすべて変更
        /// </summary>
        /// <param name="count">変更する値</param>
        public void SetAllCount(int count)
        {
            foreach (var vm in SelectorVMs.Value)
            {
                vm.DecoCount.Value = count;
            }
        }
    }
}
