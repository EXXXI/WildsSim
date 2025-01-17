using Reactive.Bindings;
using SimModel.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WildsSim.ViewModels.Controls
{
    /// <summary>
    /// 装飾品所持数変更部品
    /// </summary>
    internal class DecoCountSelectorViewModel : ChildViewModelBase
    {
        /// <summary>
        /// 表示名
        /// </summary>
        public ReactivePropertySlim<string> DispName { get; set; } = new();

        /// <summary>
        /// 所持数
        /// </summary>
        public ReactivePropertySlim<int> DecoCount { get; set; } = new();

        /// <summary>
        /// 所持数の選択肢リスト
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<int>> DecoCountList { get; set; } = new();

        /// <summary>
        /// 対応する装飾品データ
        /// </summary>
        private Deco BaseDeco { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="deco"></param>
        public DecoCountSelectorViewModel(Deco deco)
        {
            BaseDeco = deco;
            DispName.Value = deco.DispName;
            DecoCount.Value = deco.DecoCount;
            DecoCountList.Value = new() { 0, 1, 2, 3, 4, 5, 6, 7};

            DecoCount.Subscribe(_ => SaveDecoCount());
        }

        /// <summary>
        /// 所持数の変更を保存
        /// </summary>
        private void SaveDecoCount()
        {
            Simulator.SaveDecoCount(BaseDeco, DecoCount.Value);
        }
    }
}
