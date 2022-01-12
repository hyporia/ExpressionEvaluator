namespace ExpressionEvaluator
{
    public interface IEvaluator
    {
        /// <summary>
        /// Evaluate expression that consists only of numbers, operations "* / + -" and brackets
        /// </summary>
        /// <param name="expression">Expression</param>
        /// <exception cref="ArgumentNullException">If expression is null or empty</exception>
        /// <exception cref="InvalidOperationException">If expression is incorrect</exception>
        /// <returns>Evaluation result as a string</returns>
        string EvaluateExpression(string expression = null!);
    }
}
