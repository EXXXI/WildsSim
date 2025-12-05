using Reactive.Bindings;
using SimModel.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WildsSim.ViewModels.BindableWrapper
{
    internal class BindableArtian : BindableEquipment
    {
        /// <summary>
        /// 武器種
        /// </summary>
        public ReactivePropertySlim<string> ArtianWeaponType { get; } = new();

        /// <summary>
        /// スキル一覧
        /// </summary>
        public ReactivePropertySlim<string> SkillDescription { get; } = new();

        /// <summary>
        /// アーティアを削除するコマンド
        /// </summary>
        public ReactiveCommand DeleteCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="original">オリジナルのアーティアデータ</param>
        /// <exception cref="ArgumentException">武器以外が渡された場合</exception>
        public BindableArtian(Weapon original) : base(original)
        {
            if (original.Kind != EquipKind.weapon)
            {
                throw new ArgumentException("武器以外の装備がアーティアとして登録されています");
            }
            ArtianWeaponType.Value = original.WeaponType.ToString();

            List<string> skillNames = new();
            foreach (var skill in original.Skills)
            {
                skillNames.Add(skill.Name);
            }
            SkillDescription.Value = string.Join(", ", skillNames);

            DeleteCommand.Subscribe(() => Delete());
        }

        /// <summary>
        /// アーティア削除
        /// </summary>
        private void Delete()
        {
            ArtianTabVM.DeleteArtian((Weapon)Original);
        }

        /// <summary>
        /// リストをまとめてバインド用クラスに変換
        /// </summary>
        /// <param name="list">変換前リスト</param>
        /// <returns></returns>
        static public ObservableCollection<BindableArtian> BeBindableList(List<Weapon> list)
        {
            ObservableCollection<BindableArtian> bindableList = new();
            foreach (var equip in list)
            {
                bindableList.Add(new BindableArtian(equip));
            }

            // 返却
            return bindableList;
        }
    }
}
