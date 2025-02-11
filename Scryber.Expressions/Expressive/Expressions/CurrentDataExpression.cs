﻿using System;
using System.Collections.Generic;

namespace Scryber.Expressive.Expressions
{
    public class CurrentDataExpression : IExpression
    {

        public const string CurrentDataVariableName = "[CurrentData]";

        private Context context;

        public CurrentDataExpression(Context context)
        {
            this.context = context;
        }

       
        #region IExpression Members

        /// <inheritdoc />
        public object Evaluate(IDictionary<string, object> variables)
        {
            object value;
            if (variables.TryGetValue(CurrentDataVariableName, out value))
                return value;
            else
                return null;
        }

        #endregion
    }
}
