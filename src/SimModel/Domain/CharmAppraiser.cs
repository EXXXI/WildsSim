using SimModel.Model;
using SimModel.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimModel.Domain
{
    public class CharmAppraiser
    {
        private List<Deco>? GenericDecos { get; set; }

        private readonly SearcherFactory _searcherFactory;
        public CharmAppraiser(SearcherFactory searcherFactory)
        {
            _searcherFactory = searcherFactory;
        }

        private void MakeGenericDecos()
        {
            List<Deco> decos = new();
            for (int type = 0; type <= 2; type++)
            {
                for (int size = 1; size <= 4; size++)
                {
                    Deco deco = new Deco()
                    {
                        Name = GenericDecoName(type, size),
                        SlotType1 = type,
                        Slot1 = size,
                        Skills = new List<Skill>() { new Skill(GenericDecoName(type,size), 1) }
                    };
                    decos.Add(deco);
                }
            }

            GenericDecos = decos;
        }

        private string GenericDecoName(int type, int size)
        {
            return $"_{GenericTypeName(type)}汎珠【{GenericSizeName(size)}】";
        }

        private string GenericTypeName(int type)
        {
            return type switch
            {
                0 => "防",
                1 => "攻",
                2 => "両",
                _ => throw new ArgumentException("Invalid type")
            };
        }
        private string GenericSizeName(int size)
        {
            return size switch
            {
                1 => "１",
                2 => "２",
                3 => "３",
                4 => "４",
                _ => throw new ArgumentException("Invalid type")
            };
        }


        /// <summary>
        /// 第一引数の防具が第二引数の防具の上位互換の場合true
        /// 護石比較用
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="decos">装飾品を考慮する場合装飾品リストを渡す</param>
        /// <returns></returns>
        internal bool IsLeftUpper(Equipment left, Equipment right, bool useDecos)
        {
            // 右防具に汎用装飾品を詰めて、左防具でそれを再現できたら上位互換であると判定する

            // 汎用装飾品リストが未作成の場合は作成する
            if (GenericDecos == null)
            {
                MakeGenericDecos();
            }

            SearchCondition condition = new()
            {
                Skills = new(right.Skills),
                HasAllDecos = true
            };

            for (int type = 0; type <= 2; type++)
            {
                for (int size = 1; size <= 4; size++)
                {
                    int count = 0;
                    if (right.Slot1 == size && right.SlotType1 == type)
                    {
                        count++;
                    }
                    if (right.Slot2 == size && right.SlotType2 == type)
                    {
                        count++;
                    }
                    if (right.Slot3 == size && right.SlotType3 == type)
                    {
                        count++;
                    }

                    if (count > 0)
                    {
                        condition.Skills.Add(new Skill(GenericDecoName(type, size), count));
                    }
                }
            }

            SearchRange range = new(condition);
            range.Weapons = new List<Weapon>();
            range.Heads = new List<Equipment>();
            range.Bodys = new List<Equipment>();
            range.Arms = new List<Equipment>();
            range.Waists = new List<Equipment>();
            range.Legs = new List<Equipment>();
            range.Charms = [ left ];
            if (useDecos)
            {
                range.Decos = GenericDecos.Union(Masters.Decos).ToList();
            }
            else
            {
                range.Decos = GenericDecos;
            }
            range.Cludes = new List<Clude>();

            Searcher searcher = _searcherFactory.Create(condition, range);
            searcher.ExecSearch(1);

            // 1件でもヒットすれば上位互換
            bool result = (searcher.ResultSets.Count > 0);
            
            searcher.Dispose();
            return result;

        }

    }
}
