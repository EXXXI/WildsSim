using System;

namespace SimModel.ExceptionClass
{
    /// <summary>
    /// このシミュレーターとして想定内の例外を表す例外クラス
    /// </summary>
    public class SimulatorException : Exception
    {   
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public SimulatorException(string message, System.Exception innerException) : base(message, innerException)
        {
        }
    }
}
