using System;
using System.Collections.Generic;
using System.Text;

namespace Salvager.Services
{
    internal class RealLogger : ILogger
    {
        public void Log(string message) => App.Log(message);
    }
}
