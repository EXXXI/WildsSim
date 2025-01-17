using Google.OrTools.LinearSolver;
using SimModel.Config;
using SimModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Windows.Input;

namespace SimModel.Domain
{
    /// <summary>
    /// 検索を実施するクラス
    /// </summary>
    internal class Searcher : IDisposable
    {
        // 制約式・変数の名称
        const string HeadRowName = "head";
        const string BodyRowName = "body";
        const string ArmRowName = "arm";
        const string WaistRowName = "waist";
        const string LegRowName = "leg";
        const string CharmRowName = "charm";
        const string Slot1RowName = "slot1";
        const string Slot2RowName = "slot2";
        const string Slot3RowName = "slot3";
        const string Slot4RowName = "slot4";
        const string SexRowName = "sex";
        const string DefRowName = "def";
        const string FireRowName = "fire";
        const string WaterRowName = "water";
        const string ThunderRowName = "thunder";
        const string IceRowName = "ice";
        const string DragonRowName = "dragon";
        const string SkillRowPrefix = "skill_";
        const string SetRowPrefix = "set_";
        const string CludeRowPrefix = "clude_";
        const string EquipColPrefix = "equip_";

        /// <summary>
        /// 検索条件
        /// </summary>
        public SearchCondition Condition { get; set; }

        /// <summary>
        /// ソルバ
        /// </summary>
        public Solver SimSolver { get; set; }

        /// <summary>
        /// 変数の辞書
        /// </summary>
        public Dictionary<string, Variable> Variables { get; set; } = new();

        /// <summary>
        /// 制約式の辞書
        /// </summary>
        public Dictionary<string, Constraint> Constraints { get; set; } = new();

        /// <summary>
        /// 検索結果
        /// </summary>
        public List<EquipSet> ResultSets { get; set; }

        /// <summary>
        /// 中断フラグ
        /// </summary>
        public bool IsCanceling { get; set; } = false;

        /// <summary>
        /// 検索対象の頭一覧
        /// </summary>
        private List<Equipment> Heads { get; set; }

        /// <summary>
        /// 検索対象の胴一覧
        /// </summary>
        private List<Equipment> Bodys { get; set; }

        /// <summary>
        /// 検索対象の腕一覧
        /// </summary>
        private List<Equipment> Arms { get; set; }

        /// <summary>
        /// 検索対象の腰一覧
        /// </summary>
        private List<Equipment> Waists { get; set; }

        /// <summary>
        /// 検索対象の足一覧
        /// </summary>
        private List<Equipment> Legs { get; set; }

        /// <summary>
        /// コンストラクタ：検索条件を指定する
        /// </summary>
        /// <param name="condition"></param>
        public Searcher(SearchCondition condition)
        {
            Condition = condition;
            ResultSets = new List<EquipSet>();

            Heads = Masters.Heads;
            Bodys = Masters.Bodys;
            Arms = Masters.Arms;
            Waists = Masters.Waists;
            Legs = Masters.Legs;

            SimSolver = Solver.CreateSolver("SCIP");

            // 変数設定
            SetVariables();

            // 制約式設定
            SetConstraints();

            // 目的関数設定(防御力)
            SetObjective();

            // 係数設定(防具データ)
            SetDatas();
        }

        /// <summary>
        /// 検索
        /// </summary>
        /// <param name="limit">頑張り度</param>
        /// <returns>全件検索完了した場合true</returns>
        public bool ExecSearch(int limit)
        {
            // 目標検索件数
            int target = ResultSets.Count + limit;

            while (ResultSets.Count < target)
            {
                // 計算
                var result = SimSolver.Solve();
                if (!result.Equals(Solver.ResultStatus.OPTIMAL))
                {
                    // もう結果がヒットしない場合終了
                    return true;
                }

                // 計算結果整理
                EquipSet? set = MakeSet();
                if (set == null)
                {
                    // TODO: 計算結果の空データ、何故発生する？
                    // 空データが出現したら終了
                    return true;
                }

                // 次回検索時用
                // 検索済み結果除外の制約式
                List<Equipment> equips = set.ExistingEquipsWithOutDecos();
                string key = SetRowPrefix + set.GlpkRowName;
                var newConstraint = SimSolver.MakeConstraint(0.0, equips.Count - 1, key);
                Constraints.Add(key, newConstraint);
                // 検索済み結果除外の係数設定
                foreach (var equip in equips)
                {
                    // 各装備に対応する係数を1とする
                    newConstraint.SetCoefficient(Variables[EquipColPrefix + equip.Name], 1);
                }

                // 中断確認
                if (IsCanceling)
                {
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// 変数設定
        /// </summary>
        private void SetVariables()
        {
            // 各装備は0個以上で整数
            var equips = Heads.Union(Bodys).Union(Arms).Union(Waists).Union(Legs)
                .Union(Masters.Charms).Union(Masters.Decos);
            foreach (var equip in equips)
            {
                string key = EquipColPrefix + equip.Name;
                Variable value;
                if (equip is Deco deco)
                {
                    // 装飾品は所持数を上限とする
                    value = SimSolver.MakeIntVar(0.0, deco.DecoCount, key);
                }
                else
                {
                    value = SimSolver.MakeIntVar(0.0, 1.0, key);
                }
                Variables.Add(key, value);
            }
        }

        /// <summary>
        /// 制約式設定
        /// </summary>
        private void SetConstraints()
        {
            // 各部位に装着できる防具は1つまで
            Constraints.Add(HeadRowName, SimSolver.MakeConstraint(0, 1, HeadRowName));
            Constraints.Add(BodyRowName, SimSolver.MakeConstraint(0, 1, BodyRowName));
            Constraints.Add(ArmRowName, SimSolver.MakeConstraint(0, 1, ArmRowName));
            Constraints.Add(WaistRowName, SimSolver.MakeConstraint(0, 1, WaistRowName));
            Constraints.Add(LegRowName, SimSolver.MakeConstraint(0, 1, LegRowName));
            Constraints.Add(CharmRowName, SimSolver.MakeConstraint(0, 1, CharmRowName));

            // 武器スロ計算
            int[] slotCond = SlotCalc(Condition.WeaponSlot1, Condition.WeaponSlot2, Condition.WeaponSlot3);

            // 残りスロット数は0以上
            Constraints.Add(Slot1RowName, SimSolver.MakeConstraint(0.0 - slotCond[0], double.PositiveInfinity, Slot1RowName));
            Constraints.Add(Slot2RowName, SimSolver.MakeConstraint(0.0 - slotCond[1], double.PositiveInfinity, Slot2RowName));
            Constraints.Add(Slot3RowName, SimSolver.MakeConstraint(0.0 - slotCond[2], double.PositiveInfinity, Slot3RowName));
            Constraints.Add(Slot4RowName, SimSolver.MakeConstraint(0.0 - slotCond[3], double.PositiveInfinity, Slot4RowName));

            // 性別(自分と違う方を除外する)
            Constraints.Add(SexRowName, SimSolver.MakeConstraint(0, 0, SexRowName));

            // 防御・耐性
            Constraints.Add(DefRowName, SimSolver.MakeConstraint(Condition.Def ?? 0.0, double.PositiveInfinity, DefRowName));
            Constraints.Add(FireRowName, SimSolver.MakeConstraint(Condition.Fire ?? double.NegativeInfinity, double.PositiveInfinity, FireRowName));
            Constraints.Add(WaterRowName, SimSolver.MakeConstraint(Condition.Water ?? double.NegativeInfinity, double.PositiveInfinity, WaterRowName));
            Constraints.Add(ThunderRowName, SimSolver.MakeConstraint(Condition.Thunder ?? double.NegativeInfinity, double.PositiveInfinity, ThunderRowName));
            Constraints.Add(IceRowName, SimSolver.MakeConstraint(Condition.Ice ?? double.NegativeInfinity, double.PositiveInfinity, IceRowName));
            Constraints.Add(DragonRowName, SimSolver.MakeConstraint(Condition.Dragon ?? double.NegativeInfinity, double.PositiveInfinity, DragonRowName));

            // スキル
            foreach (var skill in Condition.Skills)
            {
                if (skill.IsFixed)
                {
                    string key = SkillRowPrefix + skill.Name;
                    Constraints.Add(key, SimSolver.MakeConstraint(skill.Level, skill.Level, key));
                }
                else
                {
                    string key = SkillRowPrefix + skill.Name;
                    Constraints.Add(key, SimSolver.MakeConstraint(skill.Level, double.PositiveInfinity, key));
                }
            }

            // 除外固定装備設定
            foreach (var clude in Masters.Cludes)
            {
                int fix = 0;
                if (clude.Kind.Equals(CludeKind.include))
                {
                    fix = 1;
                }
                string key = CludeRowPrefix + clude.Name;
                Constraints.Add(key, SimSolver.MakeConstraint(fix, fix, key));
            }
        }

        /// <summary>
        /// 目的関数設定(防御力)
        /// </summary>
        /// <param name="solver">ソルバ</param>
        /// <param name="x">変数の配列</param>
        private void SetObjective()
        {
            Objective objective = SimSolver.Objective();

            // 各装備の防御力が、目的関数における各装備の項の係数となる
            var equips = Heads.Union(Bodys).Union(Arms).Union(Waists).Union(Legs)
                .Union(Masters.Charms).Union(Masters.Decos);
            foreach (var equip in equips)
            {
                string key = EquipColPrefix + equip.Name;
                objective.SetCoefficient(Variables[key], Score(equip));
            }
            objective.SetMaximization();
        }

        /// <summary>
        /// 装備の評価スコアを返す
        /// </summary>
        /// <param name="equip">装備</param>
        /// <returns>スコア</returns>
        private int Score(Equipment equip)
        {
            int slot1 = 0;
            int slot2 = 0;
            int slot3 = 0;

            if (equip.Kind != EquipKind.deco)
            {
                slot1 = equip.Slot1;
                slot2 = equip.Slot2;
                slot3 = equip.Slot3;
            }
            else
            {
                slot1 = -equip.Slot1;
            }

            int score = 0;

            // 防御力
            score += equip.Maxdef;

            // スロット数
            score *= 20;
            score += Math.Sign(slot1);
            score += Math.Sign(slot2);
            score += Math.Sign(slot3);

            // スロット大きさ
            score *= 80;
            score += slot1 + slot2 + slot3;

            return score;
        }

        /// <summary>
        /// 係数設定(防具データ)
        /// </summary>
        private void SetDatas()
        {
            // 防具データ
            var equips = Heads.Union(Bodys).Union(Arms).Union(Waists).Union(Legs)
                .Union(Masters.Charms).Union(Masters.Decos);
            foreach (var equip in equips)
            {
                string key = EquipColPrefix + equip.Name;
                SetEquipData(Variables[key], equip);
            }

            // 除外固定データ
            foreach (var clude in Masters.Cludes)
            {
                Constraints[CludeRowPrefix + clude.Name].SetCoefficient(Variables[EquipColPrefix + clude.Name], 1);
            }
        }

        /// <summary>
        /// 装備のデータを係数として登録
        /// </summary>
        /// <param name="xvar">変数</param>
        /// <param name="equip">装備</param>
        private void SetEquipData(Variable xvar, Equipment equip)
        {
            // 部位情報
            string kindRowName = string.Empty;
            bool isDecoOrGSkill = false;
            switch (equip.Kind)
            {
                case EquipKind.head:
                    kindRowName = HeadRowName;
                    break;
                case EquipKind.body:
                    kindRowName = BodyRowName;
                    break;
                case EquipKind.arm:
                    kindRowName = ArmRowName;
                    break;
                case EquipKind.waist:
                    kindRowName = WaistRowName;
                    break;
                case EquipKind.leg:
                    kindRowName = LegRowName;
                    break;
                case EquipKind.charm:
                    kindRowName = CharmRowName;
                    break;
                default:
                    isDecoOrGSkill
                        = true;
                    break;
            }
            if (!isDecoOrGSkill)
            {
                Constraints[kindRowName].SetCoefficient(xvar, 1);
            }

            // スロット情報
            int[] slotCond = SlotCalc(equip.Slot1, equip.Slot2, equip.Slot3);
            if (isDecoOrGSkill)
            {
                for (int i = 0; i < slotCond.Length; i++)
                {
                    slotCond[i] = slotCond[i] * -1;
                }
            }
            Constraints[Slot1RowName].SetCoefficient(xvar, slotCond[0]);
            Constraints[Slot2RowName].SetCoefficient(xvar, slotCond[1]);
            Constraints[Slot3RowName].SetCoefficient(xvar, slotCond[2]);
            Constraints[Slot4RowName].SetCoefficient(xvar, slotCond[3]);

            // 性別情報(自分と違う方を除外する)
            if (!equip.Sex.Equals(Sex.all) && !equip.Sex.Equals(Condition.Sex))
            {
                Constraints[SexRowName].SetCoefficient(xvar, 1);
            }

            // 防御・耐性情報
            Constraints[DefRowName].SetCoefficient(xvar, equip.Maxdef);
            Constraints[FireRowName].SetCoefficient(xvar, equip.Fire);
            Constraints[WaterRowName].SetCoefficient(xvar, equip.Water);
            Constraints[ThunderRowName].SetCoefficient(xvar, equip.Thunder);
            Constraints[IceRowName].SetCoefficient(xvar, equip.Ice);
            Constraints[DragonRowName].SetCoefficient(xvar, equip.Dragon);

            // スキル情報
            foreach (var condSkill in Condition.Skills)
            {
                foreach (var equipSkill in equip.Skills)
                {
                    if (equipSkill.Name.Equals(condSkill.Name))
                    {
                        Constraints[SkillRowPrefix + condSkill.Name].SetCoefficient(xvar, equipSkill.Level);
                    }
                }
            }
        }

        /// <summary>
        /// 計算結果整理
        /// </summary>
        /// <returns>成功時EquipSet、失敗時null</returns>
        private EquipSet? MakeSet()
        {
            EquipSet equipSet = new();
            bool hasData = false;
            foreach (var keyValuePair in Variables)
            {
                Variable variable = keyValuePair.Value;
                if (variable.SolutionValue() > 0)
                {
                    // 装備名
                    string name = keyValuePair.Key.Replace(EquipColPrefix, string.Empty);

                    // 存在チェック
                    Equipment? equip = Masters.GetEquipByName(name);
                    if (equip == null || string.IsNullOrWhiteSpace(equip.Name))
                    {
                        // 存在しない装備名の場合無視
                        continue;
                    }
                    hasData = true;

                    // 装備種類確認
                    switch (equip.Kind)
                    {
                        case EquipKind.head:
                            equipSet.Head = equip;
                            break;
                        case EquipKind.body:
                            equipSet.Body = equip;
                            break;
                        case EquipKind.arm:
                            equipSet.Arm = equip;
                            break;
                        case EquipKind.waist:
                            equipSet.Waist = equip;
                            break;
                        case EquipKind.leg:
                            equipSet.Leg = equip;
                            break;
                        case EquipKind.deco:
                            for (int j = 0; j < variable.SolutionValue(); j++)
                            {
                                // 装飾品は個数を確認し、その数追加
                                equipSet.Decos.Add(equip);
                            }
                            break;
                        case EquipKind.charm:
                            equipSet.Charm = equip;
                            break;
                        default:
                            break;
                    }
                }
            }

            if (hasData)
            {
                // 装備セットにスロット情報を付加
                equipSet.WeaponSlot1 = Condition.WeaponSlot1;
                equipSet.WeaponSlot2 = Condition.WeaponSlot2;
                equipSet.WeaponSlot3 = Condition.WeaponSlot3;

                // 重複する結果(今回の結果に無駄な装備を加えたもの)が既に見つかっていた場合、それを削除
                RemoveDuplicateSet(equipSet);

                // 装飾品をソート
                equipSet.SortDecos();

                // 検索結果に追加
                ResultSets.Add(equipSet);

                // 成功
                return equipSet;
            }

            // 失敗
            return null;
        }

        /// <summary>
        /// 重複する結果(今回の結果に無駄な装備を加えたもの)が既に見つかっていた場合、それを削除
        /// </summary>
        /// <param name="newSet">新しい検索結果</param>
        private void RemoveDuplicateSet(EquipSet newSet)
        {
            List<EquipSet> removeList = new();
            foreach (var set in ResultSets)
            {
                if (!IsDuplicateEquipName(newSet.Head.Name, set.Head.Name))
                {
                    continue;
                }
                if (!IsDuplicateEquipName(newSet.Body.Name, set.Body.Name))
                {
                    continue;
                }
                if (!IsDuplicateEquipName(newSet.Arm.Name, set.Arm.Name))
                {
                    continue;
                }
                if (!IsDuplicateEquipName(newSet.Waist.Name, set.Waist.Name))
                {
                    continue;
                }
                if (!IsDuplicateEquipName(newSet.Leg.Name, set.Leg.Name))
                {
                    continue;
                }
                if (!IsDuplicateEquipName(newSet.Charm.Name, set.Charm.Name))
                {
                    continue;
                }

                // 全ての部位で重複判定を満たしたため削除
                removeList.Add(set);
            }

            foreach (var set in removeList)
            {
                ResultSets.Remove(set);
            }
        }

        /// <summary>
        /// 重複判定
        /// </summary>
        /// <param name="newName">新セットの防具名</param>
        /// <param name="oldName">旧セットの防具名</param>
        /// <returns></returns>
        private bool IsDuplicateEquipName(string newName, string oldName)
        {
            return string.IsNullOrWhiteSpace(newName) || newName.Equals(oldName);
        }

        /// <summary>
        /// GLPK用のスロット計算
        /// 例：3-1-1→1スロ以下2個2スロ以下2個3スロ以下3個
        /// </summary>
        /// <param name="slot1">スロット1</param>
        /// <param name="slot2">スロット2</param>
        /// <param name="slot3">スロット3</param>
        /// <returns>GLPK用のスロット値</returns>
        private int[] SlotCalc(int slot1, int slot2, int slot3)
        {
            int[] slotCond = new int[4];
            for (int i = 0; i < slot1; i++)
            {
                slotCond[i]++;
            }
            for (int i = 0; i < slot2; i++)
            {
                slotCond[i]++;
            }
            for (int i = 0; i < slot3; i++)
            {
                slotCond[i]++;
            }
            return slotCond;
        }

        #region Dispose関連

        /// <summary>
        /// disposeフラグ
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing">disposeフラグ</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    SimSolver.Dispose();
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// ファイナライザ
        /// </summary>
        ~Searcher()
        {
            Dispose(false);
        }

        #endregion
    }
}
