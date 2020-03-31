using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Utils
{
    public class LogicValidation
    {
        public bool IsDataValid(object data)
        {
            var isValid = data != null;
            return isValid;
        }

        public bool ValidateDataCount(int counter)
        {
            int restrictionValue = 0;
            var isValid = counter > restrictionValue;
            return isValid;
        }

        public bool ValidateDataCountWithRestriction(int counter, int restrictionValue)
        {
            var isValid = counter > restrictionValue;
            return isValid;
        }
    }
}