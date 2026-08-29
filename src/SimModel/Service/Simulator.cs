using NLog;
using SimModel.Config;
using SimModel.Domain;
using SimModel.Model;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SimModel.Service
{
    /// <summary>
    /// シミュ本体
    /// </summary>
    public class Simulator
    {
        /// <summary>
        /// 検索インスタンス
        /// </summary>
        private Searcher? Searcher { get; set; }

        /// <summary>
        /// 全件検索完了フラグ
        /// </summary>
        public bool IsSearchedAll { get; set; }

        /// <summary>
        /// 中断フラグ
        /// </summary>
        public bool IsCanceling { get; private set; } = false;

        /// <summary>
        /// 理論値護石での検索中か否か
        /// </summary>
        public bool IsBestCharmSearch { get { return Searcher?.Condition?.IsBestCharmSearch ?? false; } }

        /// <summary>
        /// 理論値アーティアでの検索中か否か
        /// </summary>
        public bool IsBestArtianSearch { get { return Searcher?.Condition?.IsBestArtianSearch ?? false; } }

        /// <summary>
        /// ログ出力用
        /// </summary>
        static private Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// データ管理クラスのインスタンス
        /// DIで注入される
        /// </summary>
        private readonly DataManagement _dataManagement;

        /// <summary>
        /// 護石関連計算クラスのインスタンス
        /// DIで注入される
        /// </summary>
        private readonly CharmAppraiser _charmAppraiser;

        /// <summary>
        /// マスタ管理クラスのインスタンス
        /// DIで注入される
        /// </summary>
        private readonly Masters _masters;

        /// <summary>
        /// ロジックの設定クラスのインスタンス
        /// DIで注入される
        /// </summary>
        private readonly LogicConfig _logicConfig;

        public Simulator(DataManagement dataManagement, CharmAppraiser charmAppraiser, LogicConfig logicConfig, Masters masters)
        {
            _dataManagement = dataManagement;
            _dataManagement.LoadData();
            _charmAppraiser = charmAppraiser;
            _logicConfig = logicConfig;
            _masters = masters;
        }

        /// <summary>
        /// 新規検索
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <param name="limit">頑張り度</param>
        /// <returns>検索結果</returns>
        public async Task<List<EquipSet>> Search(SearchCondition condition, int limit, CancellationToken? token = null)
        {
            ResetIsCanceling();

            // 検索
            Searcher?.Dispose();
            SearchRange range = new(condition, _masters);
            Searcher = new(condition, range);
            IsSearchedAll = await Searcher.ExecSearch(limit, token);

            // 最近使ったスキル更新
            UpdateRecentSkill(condition.Skills);

            // 中断時はフラグを立てる
            if (token?.IsCancellationRequested ?? false)
            {
                IsCanceling = true;
            }

            return Searcher.ResultSets;
        }

        /// <summary>
        /// 条件そのまま追加検索
        /// </summary>
        /// <param name="limit">頑張り度</param>
        /// <returns>検索結果</returns>
        public async Task<List<EquipSet>> SearchMore(int limit, CancellationToken? token = null)
        {
            ResetIsCanceling();

            // まだ検索がされていない場合、0件で返す
            if (Searcher == null)
            {
                return new List<EquipSet>();
            }

            IsSearchedAll = await Searcher.ExecSearch(limit, token);

            // 中断時はフラグを立てる
            if (token?.IsCancellationRequested ?? false)
            {
                IsCanceling = true;
            }

            return Searcher.ResultSets;
        }

        /// <summary>
        /// 追加スキル検索
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns>検索結果</returns>
        public async Task<List<Skill>> SearchExtraSkill(SearchCondition condition, Reactive.Bindings.ReactivePropertySlim<double>? progress = null, CancellationToken? token = null)
        {
            ResetIsCanceling();

            List<Skill> exSkills = new();

            // プログレスバー
            if (progress != null)
            {
                progress.Value = 0.0;
            }

            // 検索範囲は変更しないのでここで1つだけ生成
            SearchRange range = new(condition, _masters);

            // 全スキル全レベルを走査
            Parallel.ForEach(Masters.Skills,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = _logicConfig.MaxDegreeOfParallelism
                },
                () => new List<Skill>(),
                (skill, loop, subResult) => 
                {
                    // 中断チェック
                    if (token?.IsCancellationRequested ?? false)
                    {
                        return subResult;
                    }

                    for (int i = 1; i <= skill.Level; i++)
                    {
                        // 検索条件をコピー
                        SearchCondition exCondition = new(condition);

                        // スキルを検索条件に追加
                        Skill exSkill = new(skill.Name, i);
                        bool isNewSkill = exCondition.AddSkill(new Skill(skill.Name, i));

                        // 新規スキルor既存だが上位Lvのスキルの場合のみ検索を実行
                        if (isNewSkill)
                        {
                            // 頑張り度1で検索
                            using Searcher exSearcher = new(exCondition, range);
                            exSearcher.ExecSearch(1).GetAwaiter().GetResult();

                            // 1件でもヒットすれば追加スキル一覧に追加
                            if (exSearcher.ResultSets.Count > 0)
                            {
                                subResult.Add(exSkill);
                            }
                            else
                            {
                                // ヒットしなかった場合、上位Lvは確認不要
                                break;
                            }
                        }
                    }

                    // プログレスバー
                    if (progress != null)
                    {
                        lock (progress)
                        {
                            progress.Value += 1.0 / Masters.Skills.Count;
                        }

                    }

                    return subResult;
                },
                (finalResult) =>
                {
                    lock (exSkills)
                    {
                        exSkills.AddRange(finalResult);
                    }
                }
            );

            // skill.csv順にソート
            List<Skill> sortedSkills = new();
            foreach (var skill in Masters.Skills)
            {
                foreach (var result in exSkills)
                {
                    if (skill.Name == result.Name)
                    {
                        sortedSkills.Add(result);
                    }
                }
            }

            // 中断時はフラグを立てる
            if (token?.IsCancellationRequested ?? false)
            {
                IsCanceling = true;
            }

            return sortedSkills;
        }

        /// <summary>
        /// 護石検索
        /// </summary>
        /// <returns>検索結果</returns>
        public async Task<List<EquipSet>> SearchCharm(Reactive.Bindings.ReactivePropertySlim<double>? progress = null, CancellationToken? token = null)
        {
            ResetIsCanceling();

            // まだ検索がされていない場合、0件で返す
            // 想定していない状況であり、このコードが実行されたら呼び出し側のバグ
            if (Searcher == null)
            {
                logger.Warn("通常の検索を行う前に護石検索が実行されました。");
                return new List<EquipSet>();
            }

            // プログレスバー
            if (progress != null)
            {
                progress.Value = 0.0;
            }
            // 護石検索の進捗は80%までで、残り20%は下位互換護石の除外処理に使う
            double mainProgressRate = 0.8;

            // 検索対象の護石をリストアップ
            SearchCondition condition = Searcher.Condition;
            List<Equipment> targetCharms = condition.MakeRelatedCharms(Masters.ShiningCharmCombos, Masters.ShiningCharmGroups);

            // 走査
            List<EquipSet> resultSets = new();
            Parallel.ForEach(targetCharms,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = _logicConfig.MaxDegreeOfParallelism
                },
                () => new List<EquipSet>(),
                (targetCharm, loop, subResult) =>
                {
                    // 中断チェック
                    if (token?.IsCancellationRequested ?? false)
                    {
                        return subResult;
                    }

                    // 検索条件をコピー
                    SearchCondition exCondition = new(Searcher.Condition);

                    // 護石を固定
                    SearchRange range = new(exCondition, _masters);
                    range.Charms = new List<Equipment> { targetCharm };
                    range.Cludes = _masters.Cludes; // 検索範囲の護石は理想護石であり、護石の除外固定はそのまま渡しても無視されるので問題なし

                    // 頑張り度1で検索
                    using Searcher exSearcher = new(exCondition, range);
                    exSearcher.ExecSearch(1).GetAwaiter().GetResult();

                    // 1件でもヒットすれば結果に追加
                    if (exSearcher.ResultSets.Count > 0)
                    {
                        subResult.Add(exSearcher.ResultSets[0]);
                    }

                    // プログレスバー
                    if (progress != null)
                    {
                        lock (progress)
                        {
                            progress.Value += mainProgressRate / targetCharms.Count;
                        }

                    }

                    return subResult;
                },
                (finalResult) =>
                {
                    lock (resultSets)
                    {
                        resultSets.AddRange(finalResult);
                    }
                }
            );

            // (中断時用プログレスバー補正)
            if (progress != null)
            {
                progress.Value = mainProgressRate;
            }

            // 下位互換の護石で済む場合削除
            // TODO: HasUpperCharmで少し高速化できそう。検索本体に比べて恩恵が少なく、処理も複雑になりそうなので保留中
            List<EquipSet> filtered = new();
            foreach (var left in resultSets)
            {
                bool hasLower = false;
                foreach (var right in resultSets)
                {
                    // 同じ護石は除外
                    if (left == right)
                    {
                        continue;
                    }
                    // 下位互換の護石があるか確認
                    if (_charmAppraiser.IsLeftUpper(left.Charm, right.Charm, false))
                    {
                        hasLower = true;
                        break;
                    }
                }
                if (!hasLower)
                {
                    filtered.Add(left);
                }

                // プログレスバー
                if (progress != null)
                {
                        progress.Value += (1 - mainProgressRate) / resultSets.Count;
                }

            }

            // 中断時はフラグを立てる
            if (token?.IsCancellationRequested ?? false)
            {
                IsCanceling = true;
            }

            return filtered;
        }


        /// <summary>
        /// 除外装備登録
        /// </summary>
        /// <param name="name">対象装備名</param>
        /// <returns>追加できた場合その設定、追加できなかった場合null</returns>
        public Clude? AddExclude(string name)
        {
            return _dataManagement.AddExclude(name);
        }

        /// <summary>
        /// 固定装備登録
        /// </summary>
        /// <param name="name">対象装備名</param>
        /// <returns>追加できた場合その設定、追加できなかった場合null</returns>
        public Clude? AddInclude(string name)
        {
            return _dataManagement.AddInclude(name);
        }

        /// <summary>
        /// 除外・固定解除
        /// </summary>
        /// <param name="name">対象装備名</param>
        public void DeleteClude(string name)
        {
            _dataManagement.DeleteClude(name);
        }

        /// <summary>
        /// 除外・固定全解除
        /// </summary>
        public void DeleteAllClude()
        {
            _dataManagement.DeleteAllClude();
        }

        /// <summary>
        /// 防具の除外・固定全解除
        /// </summary>
        public void DeleteAllArmorClude()
        {
            _dataManagement.DeleteAllArmorClude();
        }

        /// <summary>
        /// 武器の除外・固定全解除
        /// </summary>
        public void DeleteAllWeaponClude()
        {
            _dataManagement.DeleteAllWeaponClude();
        }

        // TODO: 護石・装飾品のレア度整備やアーティア・追加護石のレア度入力が必要なため保留
        ///// <summary>
        ///// 指定レア度以下を全除外
        ///// </summary>
        ///// <param name="rare">レア度</param>
        //public void ExcludeByRare(int rare)
        //{
        //    _dataManagement.ExcludeByRare(rare);
        //}

        /// <summary>
        /// マイセット登録
        /// </summary>
        /// <param name="set">マイセット</param>
        /// <returns>登録セット</returns>
        public EquipSet? AddMySet(EquipSet set)
        {
            return _dataManagement.AddMySet(set);
        }

        /// <summary>
        /// マイセット削除
        /// </summary>
        /// <param name="set">削除対象</param>
        public void DeleteMySet(EquipSet set)
        {
            _dataManagement.DeleteMySet(set);
        }

        /// <summary>
        /// マイセット更新
        /// </summary>
        public void ChangeNameOfMySet(string name, EquipSet set)
        {
            _dataManagement.ChangeNameOfMySet(name, set);
        }

        /// <summary>
        /// マイセットの順番入れ替え
        /// </summary>
        /// <param name="dropIndex">入れ替え元</param>
        /// <param name="targetIndex">入れ替え先</param>
        public void MoveMySet(int dropIndex, int targetIndex)
        {
            _dataManagement.MoveMySet(dropIndex, targetIndex);
        }

        /// <summary>
        /// 最近使ったスキル更新
        /// </summary>
        /// <param name="skills">検索で使ったスキル</param>
        private void UpdateRecentSkill(List<Skill> skills)
        {
            _dataManagement.UpdateRecentSkill(skills);
        }

        /// <summary>
        /// マイ検索条件登録
        /// </summary>
        /// <param name="condition">登録対象</param>
        public void AddMyCondition(SearchCondition condition)
        {
            _dataManagement.AddMyCondition(condition);
        }

        /// <summary>
        /// マイ検索条件削除
        /// </summary>
        /// <param name="condition">削除対象</param>
        public void DeleteMyCondition(SearchCondition condition)
        {
            _dataManagement.DeleteMyCondition(condition);
        }

        /// <summary>
        /// マイ検索条件の名前更新
        /// </summary>
        /// <param name="name">名前</param>
        /// <param name="condition">更新対象</param>
        public void ChangeNameOfMyCondition(string name, SearchCondition condition)
        {
            _dataManagement.ChangeNameOfMyCondition(name, condition);
        }

        /// <summary>
        /// 装飾品の所持数変更を保存
        /// </summary>
        /// <param name="deco">対象の装飾品</param>
        /// <param name="count">変更する値</param>
        public void SaveDecoCount(Deco deco, int count)
        {
            _dataManagement.SaveDecoCount(deco, count);
        }

        /// <summary>
        /// 護石登録
        /// </summary>
        /// <param name="charm">登録対象</param>
        public void AddCharm(Equipment charm)
        {
            _dataManagement.AddCharm(charm);
        }

        /// <summary>
        /// 護石削除
        /// </summary>
        /// <param name="condition">削除対象</param>
        public void DeleteCharm(Equipment charm)
        {
            _dataManagement.DeleteCharm(charm);
        }

        /// <summary>
        /// 護石の順番入れ替え
        /// </summary>
        /// <param name="dropIndex">入れ替え元</param>
        /// <param name="targetIndex">入れ替え先</param>
        public void MoveCharm(int dropIndex, int targetIndex)
        {
            _dataManagement.MoveCharm(dropIndex, targetIndex);
        }

        /// <summary>
        /// アーティア登録
        /// </summary>
        /// <param name="artian">登録対象</param>
        public void AddArtian(Weapon artian)
        {
            _dataManagement.AddArtian(artian);
        }

        /// <summary>
        /// アーティア削除
        /// </summary>
        /// <param name="artian">削除対象</param>
        public void DeleteArtian(Weapon artian)
        {
            _dataManagement.DeleteArtian(artian);
        }

        /// <summary>
        /// アーティアの順番入れ替え
        /// </summary>
        /// <param name="dropIndex">入れ替え元</param>
        /// <param name="targetIndex">入れ替え先</param>
        public void MoveArtian(int dropIndex, int targetIndex)
        {
            _dataManagement.MoveArtian(dropIndex, targetIndex);
        }

        /// <summary>
        /// 中断フラグをリセット
        /// </summary>
        private void ResetIsCanceling()
        {
            IsCanceling = false;
        }
    }
}
