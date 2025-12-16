using SimModel.Config;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace SimModel.Model
{
    /// <summary>
    /// 装備
    /// </summary>
    public class Equipment
    {
        /// <summary>
        /// 管理用装備名
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// レア度
        /// </summary>
        public int Rare { get; set; }

        /// <summary>
        /// スロット1つ目
        /// </summary>
        public int Slot1 { get; set; }

        /// <summary>
        /// スロット2つ目
        /// </summary>
        public int Slot2 { get; set; }

        /// <summary>
        /// スロット3つ目
        /// </summary>
        public int Slot3 { get; set; }

        private int? transcendingSlot1 = null;
        private int? transcendingSlot2 = null;
        private int? transcendingSlot3 = null;
        /// <summary>
        /// 限界突破スロット1つ目
        /// </summary>
        public int TranscendingSlot1
        {
            get
            {
                if (transcendingSlot1 == null)
                {
                    CalcTranscendingSlots();
                }
                return transcendingSlot1.Value;
            }
        }

        /// <summary>
        /// 限界突破スロット2つ目
        /// </summary>
        public int TranscendingSlot2
        {
            get
            {
                if (transcendingSlot2 == null)
                {
                    CalcTranscendingSlots();
                }
                return transcendingSlot2.Value;
            }
        }

        /// <summary>
        /// 限界突破スロット3つ目
        /// </summary>
        public int TranscendingSlot3
        {
            get
            {
                if (transcendingSlot3 == null)
                {
                    CalcTranscendingSlots();
                }
                return transcendingSlot3.Value;
            }
        }

        /// <summary>
        /// スロットタイプ(0:防御スキルのみ,1:攻撃スキルのみ,2:両方可)1つ目
        /// </summary>
        public int SlotType1 { get; set; } = 0;

        /// <summary>
        /// スロットタイプ(0:防御スキルのみ,1:攻撃スキルのみ,2:両方可)2つ目
        /// </summary>
        public int SlotType2 { get; set; } = 0;

        /// <summary>
        /// スロットタイプ(0:防御スキルのみ,1:攻撃スキルのみ,2:両方可)3つ目
        /// </summary>
        public int SlotType3 { get; set; } = 0;

        /// <summary>
        /// 初期防御力
        /// </summary>
        public int Mindef { get; set; }

        /// <summary>
        /// 最大防御力
        /// </summary>
        public int Maxdef { get; set; }

        /// <summary>
        /// 限界突破防御力
        /// </summary>
        public int TranscendingDef { get; set; }

        /// <summary>
        /// 火耐性
        /// </summary>
        public int Fire { get; set; }

        /// <summary>
        /// 水耐性
        /// </summary>
        public int Water { get; set; }

        /// <summary>
        /// 雷耐性
        /// </summary>
        public int Thunder { get; set; }

        /// <summary>
        /// 氷耐性
        /// </summary>
        public int Ice { get; set; }

        /// <summary>
        /// 龍耐性
        /// </summary>
        public int Dragon { get; set; }

        /// <summary>
        /// ワンセット防具フラグ
        /// </summary>
        public bool IsOneSet { get; set; } = false;

        /// <summary>
        /// 仮番号(除外固定画面・ワンセット防具用)
        /// </summary>
        public int RowNo { get; set; } = int.MaxValue;

        /// <summary>
        /// スキル
        /// </summary>
        public List<Skill> Skills { get; set; } = new();

        /// <summary>
        /// 装備種類
        /// </summary>
        public EquipKind Kind { get; set; }

        /// <summary>
        /// 仮想装備フラグ(理論値護石・アーティア)
        /// </summary>
        public bool IsVirtual { get; set; } = false;

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public Equipment()
        {

        }

        /// <summary>
        /// 装備種類指定コンストラクタ
        /// </summary>
        /// <param name="kind"></param>
        public Equipment(EquipKind kind)
        {
            Kind = kind;
        }

        /// <summary>
        /// 表示用装備名の本体
        /// </summary>
        private string? dispName = null;
        /// <summary>
        /// 表示用装備名(特殊処理が必要な場合、保持してそれを利用)
        /// </summary>
        public string DispName
        {
            get
            {
                return string.IsNullOrWhiteSpace(dispName) ? Name : dispName;
            }
            set
            {
                dispName = value;
            }
        }

        // TODO: 現状、DispNameと同等。必要があれば3行程度に情報を付加
        /// <summary>
        /// 一覧での詳細表示用
        /// </summary>
        public string DetailDispName
        {
            get
            {
                return DispName;
            }
        }

        /// <summary>
        /// 装備の説明
        /// </summary>
        public string Description
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Name))
                {
                    return string.Empty;
                }

                StringBuilder sb = new StringBuilder();
                sb.Append(DispName);
                if (!Kind.Equals(EquipKind.deco))
                {
                    sb.Append(',');
                    sb.Append(Slot1);
                    sb.Append('-');
                    sb.Append(Slot2);
                    sb.Append('-');
                    sb.Append(Slot3);
                    if (IsTranscendingSlotTarget)
                    {
                        sb.Append('→');
                        sb.Append(TranscendingSlot1);
                        sb.Append('-');
                        sb.Append(TranscendingSlot2);
                        sb.Append('-');
                        sb.Append(TranscendingSlot3);
                    }
                    sb.Append('\n');
                    sb.Append("防御:");
                    sb.Append(Mindef);
                    sb.Append('→');
                    sb.Append(Maxdef);
                    sb.Append('→');
                    sb.Append(TranscendingDef);
                    sb.Append(',');
                    sb.Append("火:");
                    sb.Append(Fire);
                    sb.Append(',');
                    sb.Append("水:");
                    sb.Append(Water);
                    sb.Append(',');
                    sb.Append("雷:");
                    sb.Append(Thunder);
                    sb.Append(',');
                    sb.Append("氷:");
                    sb.Append(Ice);
                    sb.Append(',');
                    sb.Append("龍:");
                    sb.Append(Dragon);
                }
                foreach (var skill in Skills)
                {
                    sb.Append('\n');
                    sb.Append(skill.Description);
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// 装備の簡易説明(名前とスロットのみ)
        /// </summary>
        public string SimpleDescription
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(Kind.StrWithColon());
                if (!string.IsNullOrWhiteSpace(Name))
                {
                    sb.Append(DispName);
                    if (!Kind.Equals(EquipKind.deco))
                    {
                        sb.Append(',');
                        sb.Append(Slot1);
                        sb.Append('-');
                        sb.Append(Slot2);
                        sb.Append('-');
                        sb.Append(Slot3);
                        if (IsTranscendingSlotTarget)
                        {
                            sb.Append('→');
                            sb.Append(TranscendingSlot1);
                            sb.Append('-');
                            sb.Append(TranscendingSlot2);
                            sb.Append('-');
                            sb.Append(TranscendingSlot3);
                        }
                    }
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// 護石の表示名を設定する
        /// </summary>
        public void SetCharmDispName()
        {
            if (Kind != EquipKind.charm)
            {
                // 護石以外は処理しない
                return;
            }

            StringBuilder dispName = new();
            if (IsVirtual)
            {
                dispName.Append("(仮) ");
            }
            foreach (var skill in Skills)
            {
                dispName.Append(skill.Name);
                if (skill.Level > 0)
                {
                    dispName.Append($"Lv{skill.Level}");
                }
                dispName.Append(", ");
            }
            dispName.Append(Slot1);
            if (Slot1 != 0)
            {
                switch (SlotType1)
                {
                    case 0: // 防御スキルのみ
                        dispName.Append("(防)");
                        break;
                    case 1: // 武器スキルのみ
                        dispName.Append("(武)");
                        break;
                    case 2: // 両対応
                        dispName.Append("(両)");
                        break;
                    default:
                        break;
                }
            }
            dispName.Append("-");
            dispName.Append(Slot2);
            if (Slot2 != 0)
            {
                switch (SlotType2)
                {
                    case 0: // 防御スキルのみ
                        dispName.Append("(防)");
                        break;
                    case 1: // 武器スキルのみ
                        dispName.Append("(武)");
                        break;
                    case 2: // 両対応
                        dispName.Append("(両)");
                        break;
                    default:
                        break;
                }
            }
            dispName.Append("-");
            dispName.Append(Slot3);
            if (Slot3 != 0)
            {
                switch (SlotType3)
                {
                    case 0: // 防御スキルのみ
                        dispName.Append("(防)");
                        break;
                    case 1: // 武器スキルのみ
                        dispName.Append("(武)");
                        break;
                    case 2: // 両対応
                        dispName.Append("(両)");
                        break;
                    default:
                        break;
                }
            }
            DispName = dispName.ToString();
        }


        /// <summary>
        /// 限界突破スロット計算
        /// </summary>
        private void CalcTranscendingSlots()
        {
            if (!IsTranscendingSlotTarget)
            {
                // 防具5部位以外や、レア5,6以外は処理しない
                transcendingSlot1 = Slot1;
                transcendingSlot2 = Slot2;
                transcendingSlot3 = Slot3;
                return;
            }

            // レア5は全スロットを+1(計+3)、レア6はスロット1,2を+1(計+2)。元々Lv3のスロットは変化しない
            transcendingSlot1 = Math.Min(Slot1 + 1, 3);
            transcendingSlot2 = Math.Min(Slot2 + 1, 3);
            transcendingSlot3 = (Rare == 6) ? Slot3 : Math.Min(Slot3 + 1, 3);
            return;
        }

        /// <summary>
        /// スロット限界突破対象防具かどうか
        /// </summary>
        private bool IsTranscendingSlotTarget
        {
            get
            {
                // 5部位のどれか、かつ、レア5～6
                return (Kind == EquipKind.head ||
                        Kind == EquipKind.body ||
                        Kind == EquipKind.arm ||
                        Kind == EquipKind.waist ||
                        Kind == EquipKind.leg) &&
                       (Rare == 5 || Rare == 6);
            }
        }
    }
}
