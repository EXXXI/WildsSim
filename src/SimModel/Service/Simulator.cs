using SimModel.Config;
using SimModel.Domain;
using SimModel.Model;
using System.Collections.Generic;
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
        private Searcher Searcher { get; set; }

        /// <summary>
        /// 全件検索完了フラグ
        /// </summary>
        public bool IsSearchedAll { get; set; }

        /// <summary>
        /// 中断フラグ
        /// </summary>
        public bool IsCanceling { get; private set; } = false;

        /// <summary>
        /// データ読み込み
        /// </summary>
        public void LoadData()
        {
            // マスタデータ類の読み込み
            FileOperation.LoadSkillCSV();
            FileOperation.LoadHeadCSV();
            FileOperation.LoadBodyCSV();
            FileOperation.LoadArmCSV();
            FileOperation.LoadWaistCSV();
            FileOperation.LoadLegCSV();
            FileOperation.LoadCludeCSV();
            FileOperation.LoadDecoCSV();
            FileOperation.LoadWeaponCSV();

            // セーブデータ類の読み込み
            FileOperation.MakeSaveFolder();
            FileOperation.LoadCharmCSV();
            FileOperation.LoadRecentSkillCSV();
            FileOperation.LoadMyConditionCSV();
            FileOperation.LoadMySetCSV();
        }

        /// <summary>
        /// 新規検索
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <param name="limit">頑張り度</param>
        /// <returns>検索結果</returns>
        public List<EquipSet> Search(SearchCondition condition, int limit)
        {
            ResetIsCanceling();

            // 検索
            if (Searcher != null)
            {
                Searcher.Dispose();
            }
            Searcher = new Searcher(condition);
            IsSearchedAll = Searcher.ExecSearch(limit);

            // 最近使ったスキル更新
            UpdateRecentSkill(condition.Skills);

            return Searcher.ResultSets;
        }

        /// <summary>
        /// 条件そのまま追加検索
        /// </summary>
        /// <param name="limit">頑張り度</param>
        /// <returns>検索結果</returns>
        public List<EquipSet> SearchMore(int limit)
        {
            ResetIsCanceling();

            // まだ検索がされていない場合、0件で返す
            if (Searcher == null)
            {
                return new List<EquipSet>();
            }

            IsSearchedAll = Searcher.ExecSearch(limit);

            return Searcher.ResultSets;
        }

        /// <summary>
        /// 追加スキル検索
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns>検索結果</returns>
        public List<Skill> SearchExtraSkill(SearchCondition condition, Reactive.Bindings.ReactivePropertySlim<double>? progress = null)
        {
            ResetIsCanceling();

            List<Skill> exSkills = new();

            // プログレスバー
            if (progress != null)
            {
                progress.Value = 0.0;
            }

            // 全スキル全レベルを走査
            Parallel.ForEach(Masters.Skills,
                new ParallelOptions { 
                    MaxDegreeOfParallelism = LogicConfig.Instance.MaxDegreeOfParallelism 
                },
                () => new List<Skill>(),
                (skill, loop, subResult) =>
                {
                    // 中断チェック
                    // TODO: もし時間がかかるようならCancelToken等でちゃんとループを終了させること
                    if (IsCanceling)
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
                            using Searcher exSearcher = new Searcher(exCondition);
                            exSearcher.ExecSearch(1);

                            // 1件でもヒットすれば追加スキル一覧に追加
                            if (exSearcher.ResultSets.Count > 0)
                            {
                                subResult.Add(exSkill);
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

            return sortedSkills;
        }

        /// <summary>
        /// 除外装備登録
        /// </summary>
        /// <param name="name">対象装備名</param>
        /// <returns>追加できた場合その設定、追加できなかった場合null</returns>
        public Clude? AddExclude(string name)
        {
            return DataManagement.AddExclude(name);
        }

        /// <summary>
        /// 固定装備登録
        /// </summary>
        /// <param name="name">対象装備名</param>
        /// <returns>追加できた場合その設定、追加できなかった場合null</returns>
        public Clude? AddInclude(string name)
        {
            return DataManagement.AddInclude(name);
        }

        /// <summary>
        /// 除外・固定解除
        /// </summary>
        /// <param name="name">対象装備名</param>
        public void DeleteClude(string name)
        {
            DataManagement.DeleteClude(name);
        }

        /// <summary>
        /// 除外・固定全解除
        /// </summary>
        public void DeleteAllClude()
        {
            DataManagement.DeleteAllClude();
        }

        /// <summary>
        /// 防具の除外・固定全解除
        /// </summary>
        public void DeleteAllArmorClude()
        {
            DataManagement.DeleteAllArmorClude();
        }

        /// <summary>
        /// 武器の除外・固定全解除
        /// </summary>
        public void DeleteAllWeaponClude()
        {
            DataManagement.DeleteAllWeaponClude();
        }

        // TODO: 現状未使用
        /// <summary>
        /// 指定レア度以下を全除外
        /// </summary>
        /// <param name="rare">レア度</param>
        public void ExcludeByRare(int rare)
        {
            DataManagement.ExcludeByRare(rare);
        }

        /// <summary>
        /// マイセット登録
        /// </summary>
        /// <param name="set">マイセット</param>
        /// <returns>登録セット</returns>
        public EquipSet AddMySet(EquipSet set)
        {
            return DataManagement.AddMySet(set);
        }

        /// <summary>
        /// マイセット削除
        /// </summary>
        /// <param name="set">削除対象</param>
        public void DeleteMySet(EquipSet set)
        {
            DataManagement.DeleteMySet(set);
        }

        /// <summary>
        /// マイセット更新
        /// </summary>
        public void SaveMySet()
        {
            DataManagement.SaveMySet();
        }

        /// <summary>
        /// マイセット再読み込み
        /// </summary>
        public void LoadMySet()
        {
            DataManagement.LoadMySet();
        }

        /// <summary>
        /// 最近使ったスキル更新
        /// </summary>
        /// <param name="skills">検索で使ったスキル</param>
        public void UpdateRecentSkill(List<Skill> skills)
        {
            DataManagement.UpdateRecentSkill(skills);
        }

        /// <summary>
        /// 中断フラグをオン
        /// </summary>
        public void Cancel()
        {
            IsCanceling = true;
            if (Searcher != null)
            {
                Searcher.IsCanceling = true;
            }
        }

        /// <summary>
        /// 中断フラグをリセット
        /// </summary>
        public void ResetIsCanceling()
        {
            IsCanceling = false;
            if (Searcher != null)
            {
                Searcher.IsCanceling = false;
            }
        }

        /// <summary>
        /// マイ検索条件登録
        /// </summary>
        /// <param name="condition">登録対象</param>
        public void AddMyCondition(SearchCondition condition)
        {
            DataManagement.AddMyCondition(condition);
        }

        /// <summary>
        /// マイ検索条件削除
        /// </summary>
        /// <param name="condition">削除対象</param>
        public void DeleteMyCondition(SearchCondition condition)
        {
            DataManagement.DeleteMyCondition(condition);
        }

        /// <summary>
        /// マイ検索条件更新
        /// </summary>
        /// <param name="condition">更新対象</param>
        public void UpdateMyCondition(SearchCondition condition)
        {
            DataManagement.UpdateMyCondition(condition);
        }

        /// <summary>
        /// 装飾品の所持数変更を保存
        /// </summary>
        /// <param name="deco">対象の装飾品</param>
        /// <param name="count">変更する値</param>
        public void SaveDecoCount(Deco deco, int count)
        {
            DataManagement.SaveDecoCount(deco, count);
        }
    }
}
