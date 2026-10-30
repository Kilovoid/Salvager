using System;
using System.Collections.Generic;
using System.Text;

namespace Salvager.Abstractions
{
    public interface IClock
    {
        DateTime GetCurrentTime();
    }
}
