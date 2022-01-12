using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ExpressionEvaluator
{
    /// <summary>
    /// Implementation of IEvaluator
    /// </summary>
    public class Evaluator : IEvaluator
    {
        private const string operations = "+-*/";

        /// <inheritdoc/>
        public string EvaluateExpression(string expression = null!)
        {
            if (string.IsNullOrEmpty(expression))
            {
                throw new ArgumentNullException(nameof(expression));
            }

            // remove whitespaces
            expression = Regex.Replace(expression, @"\s+", "");

            var state = 0;
            var tokensStack = new Stack<string>();
            var currentToken = new StringBuilder();
            var i = 0;
            var curChar = default(char);
            while (i < expression?.Length && state != 8)
            {
                curChar = expression[i];
                switch (state)
                {
                    case 0: // Initial state
                        if (char.IsDigit(curChar))
                        {
                            if (curChar == '0')
                            {
                                state = 1; i++;
                            }
                            else
                            {
                                state = 3; i++;
                            }
                            currentToken.Append(curChar);
                        }
                        else if (curChar == '-')
                        {
                            state = 4; i++;
                            currentToken.Append(curChar);
                        }
                        else if (curChar == '(')
                        {
                            state = 5; i++;
                            tokensStack.Push(curChar.ToString());
                        }
                        else
                        {
                            state = 8;
                        }
                        break;
                    case 1: // Met zero at the beginning (=> single zero or 0 < double < 1)
                        if (curChar == '.')
                        {
                            state = 2; i++;
                            currentToken.Append(curChar.ToString());
                        }
                        else if (operations.Contains(curChar))
                        {
                            state = 6; i++;
                            tokensStack.Push(currentToken.ToString());
                            currentToken.Clear();
                            tokensStack.Push(curChar.ToString());
                        }
                        else
                        {
                            state = 8;
                        }
                        break;
                    case 2: // scanning double
                        if (char.IsDigit(curChar))
                        {
                            i++;
                            currentToken.Append(curChar.ToString());
                        }
                        else if (operations.Contains(curChar))
                        {
                            state = 6; i++;
                            tokensStack.Push(currentToken.ToString());
                            currentToken.Clear();
                            tokensStack.Push(curChar.ToString());
                        }
                        else if (curChar == ')')
                        {
                            state = 7; i++;
                            tokensStack.Push(currentToken.ToString());
                            currentToken.Clear();
                            tokensStack.Push(EvaluateExpressionInsideBrackets(tokensStack));
                        }
                        else
                        {
                            state = 8;
                        }
                        break;
                    case 3: // scanning integer
                        if (char.IsDigit(curChar))
                        {
                            i++;
                            currentToken.Append(curChar.ToString());
                        }
                        else if (curChar == '.')
                        {
                            state = 2; i++;
                            currentToken.Append(curChar.ToString());
                        }
                        else if (operations.Contains(curChar))
                        {
                            state = 6; i++;
                            tokensStack.Push(currentToken.ToString());
                            currentToken.Clear();
                            tokensStack.Push(curChar.ToString());
                        }
                        else if (curChar == ')')
                        {
                            state = 7; i++;
                            tokensStack.Push(currentToken.ToString());
                            currentToken.Clear();
                            tokensStack.Push(EvaluateExpressionInsideBrackets(tokensStack));
                        }
                        else
                        {
                            state = 8;
                        }
                        break;
                    case 4: // met minus before digit (=> negative number)
                        if (char.IsDigit(curChar))
                        {
                            if (curChar == '0')
                            {
                                state = 1; i++;
                            }
                            else
                            {
                                state = 3; i++;
                            }
                            currentToken.Append(curChar);
                        }
                        else
                        {
                            state = 8;
                        }
                        break;
                    case 5: // met opening bracket
                        if (char.IsDigit(curChar))
                        {
                            if (curChar == '0')
                            {
                                state = 1; i++;
                            }
                            else
                            {
                                state = 3; i++;
                            }
                            currentToken.Append(curChar);
                        }
                        else if (curChar == '-')
                        {
                            state = 4; i++;
                            currentToken.Append(curChar);
                        }
                        else if (curChar == '(')
                        {
                            i++;
                            tokensStack.Append(curChar.ToString());
                        }
                        else
                        {
                            state = 8;
                        }
                        break;

                    case 6: // met operation character
                        if (char.IsDigit(curChar))
                        {
                            if (curChar == '0')
                            {
                                state = 1; i++;
                            }
                            else
                            {
                                state = 3; i++;
                            }
                            currentToken.Append(curChar);
                        }
                        else if (curChar == '(')
                        {
                            state = 5; i++;
                            tokensStack.Push(curChar.ToString());
                        }
                        else
                        {
                            state = 8;
                        }
                        break;

                    case 7: // met closing bracket => replace expression inside brackets with number
                        if (curChar == ')')
                        {
                            i++;
                            tokensStack.Push(EvaluateExpressionInsideBrackets(tokensStack));
                        }
                        else if (operations.Contains(curChar))
                        {
                            state = 6; i++;
                            tokensStack.Push(curChar.ToString());
                        }
                        else
                        {
                            state = 8;
                        }
                        break;

                    default: break;
                }
                if (i == expression.Length) // expression ended during scanning number => push number to stack
                {
                    if (char.IsDigit(curChar))
                    {
                        tokensStack.Push(currentToken.ToString());
                    }
                }
            }

            if (state == 8)
                throw new InvalidOperationException($"Unexpected character '{curChar}'");

            return EvaluateFinalExpression(tokensStack);
        }


        /// <summary>
        /// Pop expression inside brackets from the stack and evaluate it
        /// </summary>
        /// <param name="tokensStack">Stack of tokens</param>
        /// <returns>Result of evaluation as a string</returns>
        /// <exception cref="InvalidOperationException">If expression inside stack is incorrect</exception>
        private string EvaluateExpressionInsideBrackets(Stack<string> tokensStack)
        {
            var innerExp = new List<string>();
            var curToken = default(string);

            if (tokensStack?.Any(x => x == "(") != true)
                throw new InvalidOperationException("No opening bracket");

            while ((curToken = tokensStack.Pop()) != "(")
            {
                innerExp.Insert(0, curToken);
            }
            return EvaluateSimpleExpression(innerExp);
        }

        /// <summary>
        /// Evaluate final expression inside stack.
        /// Expression must be without brackets
        /// </summary>
        private string EvaluateFinalExpression(Stack<string> tokensStack)
        {
            var innerExp = Enumerable.Reverse(tokensStack).ToList();
            return EvaluateSimpleExpression(innerExp);
        }

        /// <summary>
        /// Evaluate simple expression without brackets
        /// ex. 10+3*5/100
        /// </summary>
        /// <param name="expression">Expression as a list of numbers and operators in the original order</param>
        /// <returns>Result of evaluation as a string</returns>
        /// <exception cref="InvalidOperationException">If expression is incorrect</exception>
        private string EvaluateSimpleExpression(List<string> expression)
        {
            if (!expression.Any() || expression.Count % 2 != 1)
                throw new InvalidOperationException($"Incorrect expression: {string.Join("", expression)}");

            var res = 0.0;
            // First reduce multiplication and division
            while (expression.Contains("/") || expression.Contains("*"))
            {
                var mIndex = expression.IndexOf("*");
                var dIndex = expression.IndexOf("/");
                if (mIndex != -1 && dIndex != -1)
                {
                    if (mIndex < dIndex)
                    {
                        var a = ParseDouble(expression[mIndex - 1]);
                        var b = ParseDouble(expression[mIndex + 1]);
                        res = a * b;
                        expression.RemoveAt(mIndex - 1);
                        expression.RemoveAt(mIndex - 1);
                        expression[mIndex - 1] = res.ToString(CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        var a = ParseDouble(expression[dIndex - 1]);
                        var b = ParseDouble(expression[dIndex + 1]);
                        if (b == 0) throw new InvalidOperationException("Devision by zero");
                        res = a / b;
                        expression.RemoveAt(dIndex - 1);
                        expression.RemoveAt(dIndex - 1);
                        expression[dIndex - 1] = res.ToString(CultureInfo.InvariantCulture);
                    }
                }
                else
                {
                    if (mIndex != -1)
                    {
                        var a = ParseDouble(expression[mIndex - 1]);
                        var b = ParseDouble(expression[mIndex + 1]);
                        res = a * b;
                        expression.RemoveAt(mIndex - 1);
                        expression.RemoveAt(mIndex - 1);
                        expression[mIndex - 1] = res.ToString(CultureInfo.InvariantCulture);
                    }
                    else if (dIndex != -1)
                    {
                        var a = ParseDouble(expression[dIndex - 1]);
                        var b = ParseDouble(expression[dIndex + 1]);
                        if (b == 0) throw new InvalidOperationException("Devision by zero");
                        res = a / b;
                        expression.RemoveAt(dIndex - 1);
                        expression.RemoveAt(dIndex - 1);
                        expression[dIndex - 1] = res.ToString(CultureInfo.InvariantCulture);
                    }
                }
            }

            // while expression is not reduced to single number
            while (expression.Count != 1)
            {
                if (expression.Count % 2 != 1)
                    throw new InvalidOperationException($"Incorrect expression: {string.Join("", expression)}");

                var a = ParseDouble(expression[0]);
                var b = ParseDouble(expression[2]);
                var op = expression[1];
                if (op == "-")
                {
                    res = a - b;
                }
                else if (op == "+")
                {
                    res = a + b;
                }
                else throw new InvalidOperationException($"Incorrect operation character: {op}");
                expression.RemoveAt(0);
                expression.RemoveAt(0);
                expression[0] = res.ToString(CultureInfo.InvariantCulture);
            }

            return expression[0];
        }

        /// <summary>
        /// Parse double from string. Double must be dot-separated
        /// </summary>
        /// <param name="number">dot-separated double as a string</param>
        /// <returns>Result as a double</returns>
        /// <exception cref="InvalidOperationException">If string is incorrect</exception>
        private double ParseDouble(string number)
        {
            if (double.TryParse(number, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                return result;
            throw new InvalidOperationException($"Incorrect number to parse: {number}");
        }
    }
}
