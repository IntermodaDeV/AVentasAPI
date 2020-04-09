using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace AventasApi.Utils
{
    public class LogicValidation
    {
        public string CounterToSplit(string counter, int position)
        {
            string[] data = counter.Split('-');
            var isValid = data.Count() < 3;
            if (isValid)
            {
                return " ";
            }
            return data[position];
        }

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

        public bool AreModelsValids(object firstModel, object secondModel)
        {
            var isValid = firstModel == null || secondModel == null;
            return isValid;
        }

        public bool AreModelDistinct(object firstModel, object secondModel)
        {
            var isValid = firstModel.GetType() != secondModel.GetType();
            return isValid;
        }
    }
}