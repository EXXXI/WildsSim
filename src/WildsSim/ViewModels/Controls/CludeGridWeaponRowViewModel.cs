using Reactive.Bindings;
using SimModel.Model;
using System.Linq;

namespace WildsSim.ViewModels.Controls
{
    /// <summary>
    /// 除外固定一覧表用の各行のVM
    /// </summary>
    internal class CludeGridWeaponRowViewModel : ChildViewModelBase
    {
        /// <summary>
        /// 大剣
        /// </summary>
        public ReactivePropertySlim<CludeGridCellViewModel> GreatSword { get; } = new();

        /// <summary>
        /// 太刀
        /// </summary>
        public ReactivePropertySlim<CludeGridCellViewModel> LongSword { get; } = new();

        /// <summary>
        /// 片手剣
        /// </summary>
        public ReactivePropertySlim<CludeGridCellViewModel> SwordAndShield { get; } = new();

        /// <summary>
        /// 双剣
        /// </summary>
        public ReactivePropertySlim<CludeGridCellViewModel> DualBlades { get; } = new();

        /// <summary>
        /// ランス
        /// </summary>
        public ReactivePropertySlim<CludeGridCellViewModel> Lance { get; } = new();

        /// <summary>
        /// ガンランス
        /// </summary>
        public ReactivePropertySlim<CludeGridCellViewModel> Gunlance { get; } = new();

        /// <summary>
        /// ハンマー
        /// </summary>
        public ReactivePropertySlim<CludeGridCellViewModel> Hammer { get; } = new();

        /// <summary>
        /// 狩猟笛
        /// </summary>
        public ReactivePropertySlim<CludeGridCellViewModel> HuntingHorn { get; } = new();

        /// <summary>
        /// スラッシュアックス
        /// </summary>
        public ReactivePropertySlim<CludeGridCellViewModel> SwitchAxe { get; } = new();

        /// <summary>
        /// チャージアックス
        /// </summary>
        public ReactivePropertySlim<CludeGridCellViewModel> ChargeBlade { get; } = new();

        /// <summary>
        /// 操虫棍
        /// </summary>
        public ReactivePropertySlim<CludeGridCellViewModel> InsectGlaive { get; } = new();

        /// <summary>
        /// ライトボウガン
        /// </summary>
        public ReactivePropertySlim<CludeGridCellViewModel> LightBowgun { get; } = new();

        /// <summary>
        /// ヘビィボウガン
        /// </summary>
        public ReactivePropertySlim<CludeGridCellViewModel> HeavyBowgun { get; } = new();

        /// <summary>
        /// 弓
        /// </summary>
        public ReactivePropertySlim<CludeGridCellViewModel> Bow { get; } = new();

        /// <summary>
        /// 指定した名称の装備があればVMを返す
        /// </summary>
        /// <param name="name">装備名</param>
        /// <returns>指定した名称の装備があればVM、なければnull</returns>
        public CludeGridCellViewModel? FindByName(string name)
        {
            var weapons = new ReactivePropertySlim<CludeGridCellViewModel>[]
            {
                GreatSword, LongSword, SwordAndShield, DualBlades, Lance, Gunlance, Hammer, HuntingHorn, SwitchAxe, ChargeBlade, InsectGlaive, LightBowgun, HeavyBowgun, Bow
            };
            return weapons.Where(p => p.Value.BaseEquip?.Name == name).FirstOrDefault()?.Value;
        }
    }
}
