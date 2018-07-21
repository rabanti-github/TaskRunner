using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TaskRunner
{
        /// <summary>
        /// Class to handle conditions
        /// </summary>
        [XmlRoot("condition")]
        public class Condition
        {

            /// <summary>
            /// Enum of actions for conditions
            /// </summary>
            public enum ConditionAction
            {
                /// <summary>
                /// Task or Sub-Task will be executed if condition is met
                /// </summary>
                run,
                /// <summary>
                /// Task will be restarted if condition is met
                /// </summary>
                restart_task,
                /// <summary>
                /// Last Sub-Task will be restarted if condition is met
                /// </summary>
                restart_last_subtask,
                /// <summary>
                /// Current Task or Sub-Task will be skipped if condition is met
                /// </summary>
                skip,
                /// <summary>
                /// Program will be terminated if condition is met
                /// </summary>
                exit,
                /// <summary>
                /// Default - No action / Error
                /// </summary>
                none,
            }

            /// <summary>
            /// Enum of the evaluation type / timing
            /// </summary>
            public enum ConditionType
            {
                /// <summary>
                /// Condition is checked before the execution of the task or sub task
                /// </summary>
                pre,
                /// <summary>
                /// Condition is checked after the execution of the task or sub task
                /// </summary>
                post,
                /// <summary>
                /// Default - No check / Error
                /// </summary>
                none,
            }

            /// <summary>
            /// Logic expression as string
            /// </summary>
            [XmlAttribute("expression")]
            public string Expression { get; set; }

            /// <summary>
            /// Action as string. Available are: run (default), restart_last_subtast, skip, exit, restart_task
            /// </summary>
            [XmlAttribute("action")]
            public string Action { get; set; }

            /// <summary>
            /// Default action as string if the condition is not met. Available are: run, restart_last_subtast, skip (default), exit, restart_task
            /// </summary>
            [XmlAttribute("default")]
            public string Default { get; set; }

            /// <summary>
            /// Type of the conditional check. Available are: pre (default: Check before execution of task or Sub-Task) and post (check after execution)
            /// </summary>
            [XmlAttribute("type")]
            public string Type { get; set; }

            /// <summary>
            /// Default constructor
            /// </summary>
            public Condition()
            {
                this.Expression = "true";
                this.Action = "run";
                this.Default = "skip";
                this.Type = "pre";
            }

            /// <summary>
            /// Constructor with all parameters
            /// </summary>
            /// <param name="expression">Expression to evaluate</param>
            /// <param name="action">Action to execute if expression is true</param>
            /// <param name="defaultAction">Action to execute if expression is false</param>
            /// <param name="type">Type of evaluation</param>
            public Condition(string expression, string action, string defaultAction, string type)
            {
                this.Expression = expression;
                this.Action = action;
                this.Default = defaultAction;
                this.Type = type;
            }

            /// <summary>
            /// Evaluates the expression and returns whether the evaluation is true or false
            /// </summary>
            /// <param name="displayOutput">If true, errors will be displayed</param>
            /// <returns>Evaluation state of the expression</returns>
            public bool Evaluate(bool displayOutput)
            {
                return Evaluation.ParseCondition(this.Expression, displayOutput);
            }

            /// <summary>
            /// Checks the action and default
            /// </summary>
            /// <param name="checkAction">If true the action will be checked otherwise the default action will be returned</param>
            /// <returns>Returns the appropriate value</returns>
            public ConditionAction CheckOperation(bool checkAction)
            {
                if (string.IsNullOrEmpty(this.Action)) { return ConditionAction.none; }
                string test;
                if (checkAction == true) { test = this.Action.ToLower(); }
                else { test = this.Default.ToLower(); }
                if (test == "run") { return ConditionAction.run; }
                else if (test == "skip") { return ConditionAction.skip; }
                else if (test == "exit") { return ConditionAction.exit; }
                else if (test == "restart_last_subtask") { return ConditionAction.restart_last_subtask; }
                else if (test == "restart_task") { return ConditionAction.restart_task; }
                else { return ConditionAction.none; }
            }

            /// <summary>
            /// Checks the type of the condition
            /// </summary>
            /// <returns>Returns the appropriate value</returns>
            public ConditionType CheckType()
            {
                if (string.IsNullOrEmpty(this.Type)) { return ConditionType.none; }
                string test = this.Type.ToLower();
                if (test == "pre") { return ConditionType.pre; }
                else if (test == "post") { return ConditionType.post; }
                else { return ConditionType.none; }
            }

        }
}
