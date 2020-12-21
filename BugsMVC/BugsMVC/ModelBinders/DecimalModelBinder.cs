using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugsMVC.ModelBinderClasses
{
    public class DecimalModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            ValueProviderResult valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            ModelState modelState = new ModelState { Value = valueResult };
            object actualValue = null;
            try
            {
                if (!string.IsNullOrEmpty(valueResult.AttemptedValue))
                {
                    System.Globalization.CultureInfo customCulture = CultureInfo.CurrentCulture;
                    if (CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator != ".")
                    {
                        customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
                        customCulture.NumberFormat.NumberDecimalSeparator = ".";
                        customCulture.NumberFormat.NumberGroupSeparator = ",";
                    }

                    actualValue = Convert.ToDecimal(valueResult.AttemptedValue, customCulture);
                }                
            }
            catch (FormatException e)
            {
                modelState.Errors.Add(e);
            }

            bindingContext.ModelState.Add(bindingContext.ModelName, modelState);
            return actualValue;
        }
    }
}