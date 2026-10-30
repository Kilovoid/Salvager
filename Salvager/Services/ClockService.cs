using Salvager.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Salvager.Services
{
    public class ClockService : IClock
    {
        public DateTime GetCurrentTime() => DateTime.Now;
    }
}
