using SimModel.Model;
using SimModel.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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

        private static string GenericDecoName(int type, int size)
        {
            return $"_{GenericTypeName(type)}汎珠【{GenericSizeName(size)}】";
        }

        private static string GenericTypeName(int type)
        {
            return type switch
            {
                0 => "防",
                1 => "攻",
                2 => "両",
                _ => throw new ArgumentException("Invalid type")
            };
        }
        private static string GenericSizeName(int size)
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
        internal bool IsLeftUpper(Equipment left, Equipment right, bool useDecos = true)
        {
            // 右防具に汎用装飾品を詰めて、左防具でそれを再現できたら上位互換であると判定する

            //検索
            Searcher searcher = MakeCharmSearcher(right, [left], useDecos);
            searcher.ExecSearch(1);

            // 1件でもヒットすれば上位互換
            bool result = (searcher.ResultSets.Count > 0);

            searcher.Dispose();
            return result;

        }

        /// <summary>
        /// 上位互換の護石を検索
        /// 護石比較用
        /// </summary>
        /// <param name="charm"></param>
        /// <returns></returns>
        internal Equipment? HasUpperCharm(Equipment charm, bool useDecos = true)
        {
            // 護石に汎用装飾品を詰めて、他の護石でそれを再現できたら上位互換であると判定する

            Searcher searcher = MakeCharmSearcher(charm, Masters.AdditionalCharms.Except([charm]).ToList(), useDecos);
            searcher.ExecSearch(1);

            Equipment? UpperCharm = null;
            // 目的関数で空きスロット数、スロットサイズを優先しているので、1件目が最優
            // 上位互換があれば上位互換、なければ同等の護石、それもなければ結果なしになる
            if (searcher.ResultSets.Count > 0)
            {
                UpperCharm = searcher.ResultSets[0].Charm;
            }

            searcher.Dispose();
            return UpperCharm;

        }

        /// <summary>
        /// 上位互換護石検索用のSearcherを作成
        /// </summary>
        /// <param name="baseCharm">検索するスキル・スロットのもととなる護石</param>
        /// <param name="charmRange">検索範囲</param>
        /// <param name="useDecos">装飾品を考慮する場合true</param>
        /// <returns></returns>
        private Searcher MakeCharmSearcher(Equipment baseCharm, List<Equipment> charmRange, bool useDecos = true)
        {
            // 汎用装飾品リストが未作成の場合は作成する
            if (GenericDecos == null)
            {
                MakeGenericDecos();
            }

            SearchCondition condition = new()
            {
                Skills = new(baseCharm.Skills),
                HasAllDecos = true
            };

            for (int type = 0; type <= 2; type++)
            {
                for (int size = 1; size <= 4; size++)
                {
                    int count = 0;
                    if (baseCharm.Slot1 == size && baseCharm.SlotType1 == type)
                    {
                        count++;
                    }
                    if (baseCharm.Slot2 == size && baseCharm.SlotType2 == type)
                    {
                        count++;
                    }
                    if (baseCharm.Slot3 == size && baseCharm.SlotType3 == type)
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
            range.Charms = charmRange;
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
            return searcher;
        }
    }
}
