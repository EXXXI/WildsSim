using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimModel.ExceptionClass
{
    /// <summary>
    /// このシミュレーターとして想定内の例外を表す例外クラス
    /// </summary>
    public class SimulatorException : Exception
    {

        public SimulatorException(string message) : base(message)
        {
        }

        public SimulatorException(string message, System.Exception innerException) : base(message, innerException)
        {
        }
    }
}
