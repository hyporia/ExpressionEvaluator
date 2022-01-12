namespace ExpressionEvaluator
{
    public interface IEvaluator
    {
        /// <summary>
        /// Evaluate expression that consists only numbers, operations "* / + -" and brackets
        /// </summary>
        /// <param name="expression">Expression</param>
        /// <returns>Evaluation result as string</returns>
        string EvaluateExpression(string expression);
    }
}
