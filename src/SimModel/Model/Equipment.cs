using System.Collections.Generic;
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
        /// 性別制限
        /// </summary>
        public Sex Sex { get; set; }

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

        /// <summary>
        /// 初期防御力
        /// </summary>
        public int Mindef { get; set; }

        /// <summary>
        /// 最大防御力
        /// </summary>
        public int Maxdef { get; set; }

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
        /// 仮番号(除外固定画面用)
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
        public string DispName { 
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
                if (!Kind.Equals(EquipKind.deco) && !Kind.Equals(EquipKind.charm))
                {
                    sb.Append(',');
                    sb.Append(Slot1);
                    sb.Append('-');
                    sb.Append(Slot2);
                    sb.Append('-');
                    sb.Append(Slot3);
                    sb.Append('\n');
                    sb.Append("防御:");
                    sb.Append(Mindef);
                    sb.Append('→');
                    sb.Append(Maxdef);
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
                    if (!Kind.Equals(EquipKind.deco) && !Kind.Equals(EquipKind.charm))
                    {
                        sb.Append(',');
                        sb.Append(Slot1);
                        sb.Append('-');
                        sb.Append(Slot2);
                        sb.Append('-');
                        sb.Append(Slot3);
                    }
                }

                return sb.ToString();
            }
        }
    }
}
