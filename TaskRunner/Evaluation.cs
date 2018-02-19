using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;


namespace TaskRunner
{
    /// <summary>
    /// Task Runner - (c) 2017 - Raphael Stoeckli
    /// This program and its code is released under the MIT license
    /// -----------------------------------------------------------
    /// Class for handling of the evaluation of expressions
    /// </summary>
    public class Evaluation
    {
        /// <summary>
        /// Method for expression parsing
        /// </summary>
        /// <param name="expression">Raw expression to parse</param>
        /// <param name="displayOutput">If true, information about the executed Sub-Tasks is passed to the command shell</param>
        /// <returns>True, if the expression was evaluates successfully, otherwise false</returns>
        public static bool ParseCondition(string expression, bool displayOutput)
        {
            bool status = false;
            string expression2 = Evaluation.ParseParameters(expression);
            try
            {
                DataTable dt = new DataTable();
                var result = dt.Compute(expression2, "");
                status = (bool)result;
            }
            catch
            {
                if (displayOutput == true)
                {
                    Console.WriteLine("Error: The expression '" + expression + "' (translated to '" + expression2 + "') is invalid");
                    return false;
                }
            }
            return status;
        }

        /// <summary>
        /// Method to resolve parameters in an expression string and to correct the logical syntax into a valid format
        /// </summary>
        /// <param name="expression">Raw expression string</param>
        /// <returns>Resolved expression string</returns>
        private static string ParseParameters(string expression)
        {
            foreach (KeyValuePair<string, Parameter> param in Parameter.SystemParameters)
            {
               if (param.Value.ParameterType == Parameter.Types.DateTime)
               {
                   expression = expression.Replace(param.Key, param.Value.DateTimeValue.Ticks.ToString());
               }
               else if (param.Value.ParameterType == Parameter.Types.Number)
               {
                   expression = expression.Replace(param.Key, param.Value.NumericValue.ToString());
               }
               else if (param.Value.ParameterType == Parameter.Types.Boolean)
               {
                   expression = expression.Replace(param.Key, param.Value.BooleanValue.ToString());
               }
               else
               {
                   expression = expression.Replace(param.Key, param.Value.Value);
               }
            }
            foreach (KeyValuePair<string, Parameter> param in Parameter.UserParameters)
            {
                if (param.Value.ParameterType == Parameter.Types.DateTime)
                {
                    expression = expression.Replace(param.Key, param.Value.DateTimeValue.Ticks.ToString());
                }
                else if (param.Value.ParameterType == Parameter.Types.Number)
                {
                    expression = expression.Replace(param.Key, param.Value.NumericValue.ToString());
                }
                else if (param.Value.ParameterType == Parameter.Types.Boolean)
                {
                    expression = expression.Replace(param.Key, param.Value.BooleanValue.ToString());
                }
                else
                {
                    expression = expression.Replace(param.Key, param.Value.Value);
                }
            }
            expression = expression.Replace("!=", "<>");
            expression = expression.Replace("==", "=");
            expression = expression.Replace("&&", " and ");
            expression = expression.Replace("||", " or ");
            return expression;
        }


    }
}
