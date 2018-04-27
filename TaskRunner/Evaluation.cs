using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;


namespace TaskRunner
{
    /// <summary>
    /// Task Runner - (c) 2018 - Raphael Stoeckli
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
        /// <remarks>Dates stored in parameters will be interpreted as ticks (long: 100 nanoseconds per tick). If strings are to compare, the terms must be enclosed by single quotes (')</remarks>
        public static bool ParseCondition(string expression, bool displayOutput)
        {
            if (string.IsNullOrEmpty(expression)) { return false; }
            string expression2 = Evaluation.ParseParameters(expression);
            expression2 = Evaluation.NormalizeExpression(expression2);
            try
            {
                DataTable dt = new DataTable();
                var result = dt.Compute(expression2, "");
                return (bool)result;
            }
            catch (Exception e)
            {
                if (displayOutput == true)
                {
                    Console.WriteLine("Error: The expression '" + expression + "' (translated to '" + expression2 + "') is invalid");  
                }
                return false;
            }
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
                   expression = expression.Replace(param.Key, param.Value.NumericValue.ToString(CultureInfo.InvariantCulture));
               }
               else if (param.Value.ParameterType == Parameter.Types.Boolean)
               {
                   expression = expression.Replace(param.Key, param.Value.BooleanValue.ToString());
               }
               else
               {
                   expression = expression.Replace(param.Key,"'" + param.Value.Value + "'");
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
                    expression = expression.Replace(param.Key, param.Value.NumericValue.ToString(CultureInfo.InvariantCulture));
                }
                else if (param.Value.ParameterType == Parameter.Types.Boolean)
                {
                    expression = expression.Replace(param.Key, param.Value.BooleanValue.ToString());
                }
                else
                {
                    expression = expression.Replace(param.Key, "'" + param.Value.Value + "'");
                }
            }

            return expression;
        }

        private static string NormalizeExpression(string expression)
        {
            Regex rx = new Regex("('.*?')");
            MatchCollection match = rx.Matches(expression);
            List<string> tokens = new List<string>();
            foreach (Match item in match)
            {
                tokens.Add(item.Value);
            }

            expression = Regex.Replace(expression, "('.*?')", "\x0");
            expression = expression.Replace("!=", "<>");
            expression = expression.Replace("==", "=");
            expression = expression.Replace("&&", " and ");
            expression = expression.Replace("||", " or ");
            expression = expression.Replace("!", " not "); // single exclamation mark
            string[] tokens2 = expression.Split('\x0');
            StringBuilder sb = new StringBuilder();
           
            foreach (string token in tokens2)
            {
                sb.Append(token);
                if (tokens.Count > 0)
                {
                    sb.Append(tokens[0]);
                    tokens.RemoveAt(0);
                }
            }

            return sb.ToString();
        }


    }
}
