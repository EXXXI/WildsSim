using Reactive.Bindings;
using SimModel.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace WildsSim.ViewModels.BindableWrapper
{
    /// <summary>
    /// バインド用装備セット
    /// </summary>
    internal class BindableEquipSet : ChildViewModelBase
    {
        /// <summary>
        /// 武器装備
        /// </summary>
        public ReactivePropertySlim<BindableEquipment> Weapon { get; } = new();

        /// <summary>
        /// 頭装備
        /// </summary>
        public ReactivePropertySlim<BindableEquipment> Head { get; } = new();

        /// <summary>
        /// 胴装備
        /// </summary>
        public ReactivePropertySlim<BindableEquipment> Body { get; } = new();

        /// <summary>
        /// 腕装備
        /// </summary>
        public ReactivePropertySlim<BindableEquipment> Arm { get; } = new();

        /// <summary>
        /// 腰装備
        /// </summary>
        public ReactivePropertySlim<BindableEquipment> Waist { get; } = new();

        /// <summary>
        /// 足装備
        /// </summary>
        public ReactivePropertySlim<BindableEquipment> Leg { get; } = new();

        /// <summary>
        /// 護石
        /// </summary>
        public ReactivePropertySlim<BindableEquipment> Charm { get; } = new();

        /// <summary>
        /// マイセット用名前
        /// </summary>
        public ReactivePropertySlim<string> Name { get; } = new();

        /// <summary>
        /// 最大防御力
        /// </summary>
        public ReactivePropertySlim<int> Maxdef { get; } = new();

        /// <summary>
        /// 火耐性
        /// </summary>
        public ReactivePropertySlim<int> Fire { get; } = new();

        /// <summary>
        /// 水耐性
        /// </summary>
        public ReactivePropertySlim<int> Water { get; } = new();

        /// <summary>
        /// 雷耐性
        /// </summary>
        public ReactivePropertySlim<int> Thunder { get; } = new();

        /// <summary>
        /// 氷耐性
        /// </summary>
        public ReactivePropertySlim<int> Ice { get; } = new();

        /// <summary>
        /// 龍耐性
        /// </summary>
        public ReactivePropertySlim<int> Dragon { get; } = new();

        /// <summary>
        /// 装飾品のCSV表記 3行
        /// </summary>
        public ReactivePropertySlim<string> DecoNameCSV { get; } = new();

        /// <summary>
        /// スキルのCSV形式 3行
        /// </summary>
        public ReactivePropertySlim<string> SkillsDisp { get; } = new();

        /// <summary>
        /// 説明
        /// </summary>
        public ReactivePropertySlim<string> Description { get; } = new();

        /// <summary>
        /// オリジナル
        /// </summary>
        public EquipSet Original { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="set"></param>
        public BindableEquipSet(EquipSet set)
        {
            Weapon.Value = new BindableEquipment(set.Weapon);
            Head.Value = new BindableEquipment(set.Head);
            Body.Value = new BindableEquipment(set.Body);
            Arm.Value = new BindableEquipment(set.Arm);
            Waist.Value = new BindableEquipment(set.Waist);
            Leg.Value = new BindableEquipment(set.Leg);
            Charm.Value = new BindableEquipment(set.Charm);
            Name.Value = set.Name;
            Maxdef.Value = set.Maxdef;
            Fire.Value = set.Fire;
            Water.Value = set.Water;
            Thunder.Value = set.Thunder;
            Ice.Value = set.Ice;
            Dragon.Value = set.Dragon;
            DecoNameCSV.Value = set.DecoNameCSVMultiLine;
            SkillsDisp.Value = set.SkillsDispMultiLine;
            Description.Value = set.Description;
            Original = set;

            // 名前変更時に保存と再読み込みの処理を実施
            // 初回生成分はSkip
            Name.Skip(1).Subscribe(_ => ChangeName());
        }

        /// <summary>
        /// マイセット名前変更
        /// </summary>
        private void ChangeName()
        {
            MySetTabVM.ChangeName(Name.Value);
        }

        /// <summary>
        /// リストをまとめてバインド用クラスに変換
        /// </summary>
        /// <param name="list">変換前リスト</param>
        /// <returns></returns>
        static public ObservableCollection<BindableEquipSet> BeBindableList(List<EquipSet> list)
        {
            ObservableCollection<BindableEquipSet> bindableList = new ObservableCollection<BindableEquipSet>();
            foreach (var set in list)
            {
                bindableList.Add(new BindableEquipSet(set));
            }
            return bindableList;
        }
    }
}
