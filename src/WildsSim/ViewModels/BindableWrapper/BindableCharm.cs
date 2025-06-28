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
    internal class BindableCharm : BindableEquipment
    {
        /// <summary>
        /// 護石を削除するコマンド
        /// </summary>
        public ReactiveCommand DeleteCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="original">オリジナルの護石データ</param>
        /// <exception cref="ArgumentException">護石以外が渡された場合</exception>
        public BindableCharm(Equipment original) : base(original)
        {
            if (original.Kind != EquipKind.charm)
            {
                throw new ArgumentException("護石以外の装備が護石として登録されています");
            }

            DeleteCommand.Subscribe(() => Delete());
        }

        /// <summary>
        /// 護石削除
        /// </summary>
        private void Delete()
        {
            CharmTabVM.DeleteCharm(Original);
        }

        /// <summary>
        /// リストをまとめてバインド用クラスに変換
        /// </summary>
        /// <param name="list">変換前リスト</param>
        /// <returns></returns>
        static public ObservableCollection<BindableCharm> BeBindableList(List<Equipment> list)
        {
            ObservableCollection<BindableCharm> bindableList = new();
            foreach (var equip in list)
            {
                bindableList.Add(new BindableCharm(equip));
            }

            // 返却
            return bindableList;
        }
    }
}
