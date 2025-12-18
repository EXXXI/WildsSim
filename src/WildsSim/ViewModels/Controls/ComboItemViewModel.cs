using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WildsSim.ViewModels.Controls
{
    /// <summary>
    /// コンボボックスの選択肢用のViewModel
    /// </summary>
    class ComboItemViewModel<T> : ChildViewModelBase
    {
        /// <summary>
        /// 実際の値
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// 表示する値
        /// </summary>
        public string DispName { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="value"></param>
        /// <param name="dispName"></param>
        public ComboItemViewModel(T value, string dispName) 
        {
            Value = value;
            DispName = dispName;
        }
    }
}
