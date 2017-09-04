using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskRunner
{

    public class Parameter
    {
        private static Dictionary<string, Parameter> parameters;

        public static Dictionary<string, Parameter> Parameters
        {
            get
            {
                if (Parameter.parameters == null)
                {
                    Parameter.parameters = new Dictionary<string, Parameter>();
                }
                return Parameter.parameters; 
            }
            set { Parameter.parameters = value; }
        }

        public enum Types
        {
            String,
            Number,
            Boolean,
        }

        public string Name { get; set; }

        public string Value { get; set; }

        public Types ParameterType { get; set; }

        public bool BooleanValue { get; set; }

        public double NumericValue { get; set; }

        public bool Valid { get; set; }

        public Parameter()
        {

        }

        public Parameter(string name, string value)
        {
            this.Name = name;
            this.Value = value;
            this.ParameterType = Types.String;
            this.Valid = true;
        }

        public Parameter(string rawValue)
        {
            Parameter p = Parameter.Parse(rawValue);
            this.BooleanValue = p.BooleanValue;
            this.Name = p.Name;
            this.NumericValue = p.NumericValue;
            this.ParameterType = p.ParameterType;
            this.Valid = p.Valid;
            this.Value = p.Value;
        }

        public static Parameter GetParameter(string name, bool displayOutput)
        {
            if (Parameter.Parameters.ContainsKey(name))
            {
                return Parameter.Parameters[name];
            }
            else
            {
                if (displayOutput == true)
                {
                    Console.WriteLine("the parameter '" + name + "' was not found");
                }
                Parameter p = new Parameter(name, "");
                p.Valid = false;
                return p;
            }
        }

        public static bool AddParameter(Parameter parameter, bool displayOutput)
        {
            if (parameter.Valid == false)
            { 
                if (displayOutput == true)
                {
                    Console.WriteLine("the passed parameter '" + parameter.Name + "' is invalid");
                }
                return false; 
            }
            if (Parameter.Parameters.ContainsKey(parameter.Name))
            {
                Parameter.Parameters[parameter.Name] = parameter;
                if (displayOutput == true)
                {
                    Console.WriteLine("the passed parameter '" + parameter.Name + "' was overwritten by value '" + parameter.Value + "'");
                }
            }
            else
            {
                Parameter.Parameters.Add(parameter.Name, parameter);
            }
            return true;
        }

        public static Parameter Parse(string rawValue)
        {
            Parameter p = new Parameter();
            if (string.IsNullOrEmpty(rawValue) == true) { p.Valid = false; return p; }
            if (rawValue.StartsWith("-p:") == false && rawValue.StartsWith("--param:") == false) { p.Valid = false; return p; }
            string param;
            if (rawValue.StartsWith("-p:"))
            {
                param = rawValue.Substring(3);
            }
            else
            {
                param = rawValue.Substring(8);
            }
            
            string param2;
            if (param.ToLower().StartsWith("s:") == true)
            { 
                p.ParameterType = Types.String;
                param2 = param.Substring(2);
            }
            else if (param.ToLower().StartsWith("n:") == true)
            {
                p.ParameterType = Types.Number;
                param2 = param.Substring(2);
            }
            else if (param.ToLower().StartsWith("b:") == true)
            {
                p.ParameterType = Types.Boolean;
                param2 = param.Substring(2);
            }
            else
            { 
                p.ParameterType = Types.String;
                param2 = param;
            }
            string[] tuple = param2.Split(':');
            if (tuple.Length < 2)
            {
                p.Valid = false;
                return p;
            }
            p.Name = tuple[0];
            param = param2.Substring(p.Name.Length + 1);
            if ((param.StartsWith("\"") && param.EndsWith("\"")) || (param.StartsWith("'") && param.EndsWith("'")))
            {
                p.Value = param.Substring(1, param.Length - 2);
            }
            else
            {
                p.Value = param;
            }
            if (p.ParameterType == Types.Boolean)
            {
                bool bv;
                if (bool.TryParse(p.Value, out bv) == false)
                {
                    p.Valid = false;
                    return p;
                }
                else
                {
                    p.BooleanValue = bv;
                }
            }
            if (p.ParameterType == Types.Boolean)
            {
                double dv;
                if (double.TryParse(p.Value, out dv) == false)
                {
                    p.Valid = false;
                    return p;
                }
                else
                {
                    p.NumericValue = dv;
                }
            }
            p.Valid = true;
            return p;
        }

    }
}
