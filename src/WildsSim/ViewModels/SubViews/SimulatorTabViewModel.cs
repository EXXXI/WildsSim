using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SimModel.Model;
using SimModel.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WildsSim.Util;
using WildsSim.ViewModels.BindableWrapper;
using WildsSim.ViewModels.Controls;

namespace WildsSim.ViewModels.SubViews
{
    /// <summary>
    /// 検索結果タブのVM
    /// </summary>
    class SimulatorTabViewModel : ChildViewModelBase
    {
        /// <summary>
        /// 検索結果一覧
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<BindableEquipSet>> SearchResult { get; } = new();

        /// <summary>
        /// 検索結果の選択行
        /// </summary>
        public ReactivePropertySlim<BindableEquipSet?> DetailSet { get; } = new();

        /// <summary>
        /// 装備詳細の各行のVM
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<EquipRowViewModel>> EquipRowVMs { get; } = new();

        /// <summary>
        /// 頑張り度(前回の検索の値を保存)
        /// </summary>
        public int Limit { get; set; }

        /// <summary>
        /// もっと検索可能フラグ
        /// </summary>
        public ReactivePropertySlim<bool> IsRemaining { get; } = new(false);

        /// <summary>
        /// 護石検索可能フラグ
        /// </summary>
        public ReactivePropertySlim<bool> IsNoResult { get; } = new(false);

        /// <summary>
        /// もっと検索コマンド
        /// </summary>
        public AsyncReactiveCommand SearchMoreCommand { get; private set; }

        /// <summary>
        /// 護石検索コマンド
        /// </summary>
        public AsyncReactiveCommand SearchCharmCommand { get; private set; }

        /// <summary>
        /// マイセット追加コマンド
        /// </summary>
        public ReactiveCommand AddMySetCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// 防具を除外するコマンド
        /// </summary>
        public ReactiveCommand ExcludeCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// 防具を固定するコマンド
        /// </summary>
        public ReactiveCommand IncludeCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SimulatorTabViewModel()
        {
            // シミュ画面の検索結果と装備詳細を紐づけ
            DetailSet.Subscribe(set => {
                if (set != null)
                {
                    EquipRowVMs.ChangeCollection(EquipRowViewModel.SetToEquipRows(set.Original));
                }
            });

            // コマンドを設定
            ReadOnlyReactivePropertySlim<bool> isFree = MainVM.IsFree;
            SearchMoreCommand = isFree.ToAsyncReactiveCommand().WithSubscribe(async () => await SearchMore()).AddTo(Disposable);
            SearchCharmCommand = isFree.ToAsyncReactiveCommand().WithSubscribe(async () => await SearchCharm()).AddTo(Disposable);
            AddMySetCommand.Subscribe(() => AddMySet());
            ExcludeCommand.Subscribe(x => Exclude(x as BindableEquipment));
            IncludeCommand.Subscribe(x => Include(x as BindableEquipment));
        }

        /// <summary>
        /// 検索結果表示
        /// </summary>
        /// <param name="result">検索結果</param>
        /// <param name="remain">続きの有無(もっと検索の可否)</param>
        /// <param name="limit">頑張り度</param>
        internal void ShowSearchResult(List<EquipSet> result, bool remain, int limit)
        {
            SearchResult.ChangeCollection(BindableEquipSet.BeBindableList(result));
            DetailSet.Value = SearchResult.Value.Count > 0 ? SearchResult.Value[0] : null;
            IsRemaining.Value = remain;
            IsNoResult.Value = !remain && result.Count == 0;
            Limit = limit;
        }

        /// <summary>
        /// もっと検索
        /// </summary>
        /// <returns></returns>
        async private Task SearchMore()
        {
            // ステータスバー
            SetStatusBar($"もっと検索開始・・・");

            // ビジーフラグ
            IsBusy.Value = true;
            MainVM.IsIndeterminate.Value = true;

            // もっと検索
            List<EquipSet> result = await Task.Run(() => Simulator.SearchMore(Limit));
            SearchResult.ChangeCollection(BindableEquipSet.BeBindableList(result));

            // ビジーフラグ解除
            IsBusy.Value = false;
            MainVM.IsIndeterminate.Value = false;

            // もっと検索可否を設定
            IsRemaining.Value = Simulator.IsCanceling || !Simulator.IsSearchedAll;

            // ログ表示
            if (Simulator.IsCanceling)
            {
                SetStatusBar("処理中断：中断時の検索状態を表示します");
            }
            else
            {
                SetStatusBar($"もっと検索完了：{result.Count}件");
            }
        }

        /// <summary>
        /// もっと検索
        /// </summary>
        /// <returns></returns>
        async private Task SearchCharm()
        {
            // ステータスバー
            SetStatusBar($"護石検索開始・・・");

            // ビジーフラグ
            IsBusy.Value = true;

            // 追加スキル検索
            List<Equipment> result = await Task.Run(() => Simulator.SearchCharm(MainVM.Progress));
            MainVM.Progress.Value = 0;
            List<EquipSet> showDatas = result.Select(charm => new EquipSet() { Charm = charm}).ToList();
            SearchResult.ChangeCollection(BindableEquipSet.BeBindableList(showDatas));

            // ビジーフラグ解除
            IsBusy.Value = false;

            // ログ表示
            if (Simulator.IsCanceling)
            {
                SetStatusBar("処理中断：中断時の検索状態を表示します");
            }
            else
            {
                SetStatusBar("護石検索完了");
            }
        }

        /// <summary>
        /// 装備除外
        /// </summary>
        /// <param name="equip">除外対象</param>
        private void Exclude(BindableEquipment? equip)
        {
            if (equip != null)
            {
                CludeTabVM.AddExclude(equip.Original);
            }
        }

        /// <summary>
        /// 装備固定
        /// </summary>
        /// <param name="equip">固定対象</param>
        private void Include(BindableEquipment? equip)
        {
            if (equip != null)
            {
                CludeTabVM.AddInclude(equip.Original);
            }
        }

        /// <summary>
        /// マイセットを追加
        /// </summary>
        private void AddMySet()
        {
            EquipSet? set = DetailSet.Value?.Original;
            if (set == null)
            {
                // 詳細画面が空の状態で実行したなら何もせず終了
                return;
            }

            // 追加
            EquipSet? added = Simulator.AddMySet(set);

            if (added == null)
            {
                // 追加できなかった場合は何もせず終了
                return;
            }

            // マイセットマスタのリロード
            MySetTabVM.LoadMySets();

            // ログ表示
            SetStatusBar("マイセット登録完了：" + set.Name);
        }
    }
}
