using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AYA_UIS.Shared.Exceptions;

namespace Shared.Exceptions
{
    public class PromotionException : BaseException
    {
        public PromotionException(string message)
            : base(message, "PROMOTION_ERROR", 400)
        {
        }

        public PromotionException(string message, Exception innerException)
            : base(message, "PROMOTION_ERROR", 400, innerException)
        {
        }
    }
}
