using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AYA_UIS.Core.Domain.Contracts;

namespace Services.Implementatios
{
    public class GpaCalculator : IGpaCalculator
    {
        public decimal Calculate(IEnumerable<decimal> grades)
        {
            if (!grades.Any())
                return 0;

            var average = grades.Average();

        
            return Math.Round((average / 100m) * 4m, 2);
        }
    }
}
