using Prism.Mvvm;
using Reactive.Bindings;
using SimModel.Service;
using System.Reactive.Linq;
using WildsSim.ViewModels.SubViews;

namespace WildsSim.ViewModels
{
    /// <summary>
    /// MainViewのViewModel
    /// 各タブのVMの設定を行う
    /// </summary>
    internal class MainViewModel : BindableBase
    {
        // TODO: 名称指定か何かにしたい

        /// <summary>
        /// スキル選択画面のタブIndex
        /// </summary>
        private const int SkillSelectorTabIndex = 0;

        /// <summary>
        /// 検索結果画面のタブIndex
        /// </summary>
        private const int SimulatorTabIndex = 1;

        /// <summary>
        /// MainViewインスタンス：子VMからの参照用
        /// </summary>
        static internal MainViewModel Instance { get; set; }

        /// <summary>
        /// ビジー判定
        /// </summary>
        public ReactivePropertySlim<bool> IsBusy { get; } = new(false);

        /// <summary>
        /// ビジー判定の反転
        /// </summary>
        public ReadOnlyReactivePropertySlim<bool> IsFree { get; private set; }

        /// <summary>
        /// プログレスバー進捗
        /// </summary>
        public ReactivePropertySlim<double> Progress { get; } = new();

        /// <summary>
        /// プログレスバー無限動作フラグ
        /// </summary>
        public ReactivePropertySlim<bool> IsIndeterminate { get; } = new();

        /// <summary>
        /// ステータスバー表示
        /// </summary>
        public ReactivePropertySlim<string> StatusBarText { get; } = new();

        /// <summary>
        /// シミュ本体
        /// </summary>
        internal Simulator Simulator { get; set; }

        /// <summary>
        /// スキル選択画面のVM
        /// </summary>
        public ReactivePropertySlim<SkillSelectTabViewModel> SkillSelectTabVM { get; } = new();

        /// <summary>
        /// 検索結果画面のVM
        /// </summary>
        public ReactivePropertySlim<SimulatorTabViewModel> SimulatorTabVM { get; } = new();

        /// <summary>
        /// 除外・固定画面のVM
        /// </summary>
        public ReactivePropertySlim<CludeTabViewModel> CludeTabVM { get; } = new();

        /// <summary>
        /// 護石画面のVM
        /// </summary>
        public ReactivePropertySlim<CharmTabViewModel> CharmTabVM { get; } = new();

        /// <summary>
        /// アーティア画面のVM
        /// </summary>
        public ReactivePropertySlim<ArtianTabViewModel> ArtianTabVM { get; } = new();

        /// <summary>
        /// 装飾品設定画面のVM
        /// </summary>
        public ReactivePropertySlim<DecoTabViewModel> DecoTabVM { get; } = new();

        /// <summary>
        /// マイセット画面のVM
        /// </summary>
        public ReactivePropertySlim<MySetTabViewModel> MySetTabVM { get; } = new();

        /// <summary>
        /// ライセンス画面のVM
        /// </summary>
        public ReactivePropertySlim<LicenseTabViewModel> LicenseTabVM { get; } = new();

        /// <summary>
        /// 選択しているタブのIndex
        /// </summary>
        public ReactivePropertySlim<int> SelectedTabIndex { get; set; } = new();

        /// <summary>
        /// 検索キャンセルコマンド
        /// </summary>
        public ReactiveCommand CancelCommand { get; private set; }


        /// <summary>
        /// コンストラクタ：起動時処理
        /// </summary>
        public MainViewModel()
        {
            // 子VMからの参照用にstaticにインスタンスを登録
            Instance = this;

            // シミュ本体のインスタンス化
            Simulator = new Simulator();
            Simulator.LoadData();

            // ビジー状態のプロパティを紐づけ
            IsFree = IsBusy.Select(x => !x).ToReadOnlyReactivePropertySlim();

            // 各タブのVMを設定
            SkillSelectTabVM.Value = new SkillSelectTabViewModel();
            SimulatorTabVM.Value = new SimulatorTabViewModel();
            CludeTabVM.Value = new CludeTabViewModel();
            CharmTabVM.Value = new CharmTabViewModel();
            ArtianTabVM.Value = new ArtianTabViewModel();
            DecoTabVM.Value = new DecoTabViewModel();
            MySetTabVM.Value = new MySetTabViewModel();
            LicenseTabVM.Value = new LicenseTabViewModel();

            // マスタファイル読み込み
            LoadMasters();

            // 処理中断
            CancelCommand = IsBusy.ToReactiveCommand().WithSubscribe(() => Cancel());

            // ログ表示
            StatusBarText.Value = "モンハンワイルズスキルシミュレータ for Windows";
        }

        /// <summary>
        /// 検索結果画面を表示
        /// </summary>
        internal void ShowSimulatorTab()
        {
            SelectedTabIndex.Value = SimulatorTabIndex;
        }

        /// <summary>
        /// スキル選択画面を表示
        /// </summary>
        internal void ShowSkillSelectorTab()
        {
            SelectedTabIndex.Value = SkillSelectorTabIndex;
        }


        /// <summary>
        /// マスタ情報を全てVMにロード
        /// </summary>
        internal void LoadMasters()
        {
            LoadEquips();
            MySetTabVM.Value.LoadMySets();
        }

        /// <summary>
        /// 装備関係のマスタ情報をVMにロード
        /// </summary>
        internal void LoadEquips()
        {
            // 除外固定画面用のVMの設定
            CludeTabVM.Value.LoadEquipsForClude();
            // 護石画面用のVMの設定
            CharmTabVM.Value.LoadEquipsForCharm();
            // アーティア画面用のVMの設定
            ArtianTabVM.Value.LoadEquipsForArtian();
            // スキル選択画面用のVMの設定
            SkillSelectTabVM.Value.LoadEquipsForArtian();
        }

        /// <summary>
        /// 処理中断
        /// </summary>
        private void Cancel()
        {
            Simulator.Cancel();
        }
    }
}
