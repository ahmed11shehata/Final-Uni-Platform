using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AYA_UIS.Core.Domain.Contracts
{
    public interface IGpaCalculator
    {
        decimal Calculate(IEnumerable<decimal> grades);

    }
}
