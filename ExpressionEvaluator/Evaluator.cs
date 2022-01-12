using System.Globalization;
using System.Text;

namespace ExpressionEvaluator
{
    public class Evaluator : IEvaluator
    {
        private const string operations = "+-*/";

        /// <inheritdoc/>
        public string EvaluateExpression(string expression)
        {
            if (string.IsNullOrEmpty(expression))
            {
                throw new ArgumentNullException(nameof(expression));
            }

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
                            tokensStack.Push(CalculateExpInsideBrackets(tokensStack));
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
                            tokensStack.Push(CalculateExpInsideBrackets(tokensStack));
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
                            tokensStack.Push(CalculateExpInsideBrackets(tokensStack));
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

            return CalculateFinalExp(tokensStack);
        }


        /// <summary>
        /// Replace expression with brackets inside stack with number
        /// </summary>
        string CalculateExpInsideBrackets(Stack<string> tokensStack)
        {
            var innerExp = new List<string>();
            var curToken = default(string);

            if (tokensStack?.Any(x => x == "(") != true)
                throw new InvalidOperationException("No opening bracket");

            while ((curToken = tokensStack.Pop()) != "(")
            {
                innerExp.Insert(0, curToken);
            }
            return CalculateExp(innerExp);
        }

        /// <summary>
        /// Calculate final expression inside stack which must be without brackets
        /// </summary>
        private string CalculateFinalExp(Stack<string> tokensStack)
        {
            var innerExp = Enumerable.Reverse(tokensStack).ToList();
            return CalculateExp(innerExp);
        }

        /// <summary>
        /// Calculate simple expression without brackets
        /// ex. 10+3*5/100
        /// </summary>
        private string CalculateExp(List<string> exp)
        {
            if (!exp.Any() || exp.Count % 2 != 1)
                throw new InvalidOperationException($"Incorrect expression: {string.Join("", exp)}");

            var res = 0.0;
            // First reduce multiplication and division
            while (exp.Contains("/") || exp.Contains("*"))
            {
                var mIndex = exp.IndexOf("*");
                var dIndex = exp.IndexOf("/");
                if (mIndex != -1 && dIndex != -1)
                {
                    if (mIndex < dIndex)
                    {
                        var a = ParseDouble(exp[mIndex - 1]);
                        var b = ParseDouble(exp[mIndex + 1]);
                        res = a * b;
                        exp.RemoveAt(mIndex - 1);
                        exp.RemoveAt(mIndex - 1);
                        exp[mIndex - 1] = res.ToString(CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        var a = ParseDouble(exp[dIndex - 1]);
                        var b = ParseDouble(exp[dIndex + 1]);
                        if (b == 0) throw new InvalidOperationException("Devision by zero");
                        res = a / b;
                        exp.RemoveAt(dIndex - 1);
                        exp.RemoveAt(dIndex - 1);
                        exp[dIndex - 1] = res.ToString(CultureInfo.InvariantCulture);
                    }
                }
                else
                {
                    if (mIndex != -1)
                    {
                        var a = ParseDouble(exp[mIndex - 1]);
                        var b = ParseDouble(exp[mIndex + 1]);
                        res = a * b;
                        exp.RemoveAt(mIndex - 1);
                        exp.RemoveAt(mIndex - 1);
                        exp[mIndex - 1] = res.ToString(CultureInfo.InvariantCulture);
                    }
                    else if (dIndex != -1)
                    {
                        var a = ParseDouble(exp[dIndex - 1]);
                        var b = ParseDouble(exp[dIndex + 1]);
                        if (b == 0) throw new InvalidOperationException("Devision by zero");
                        res = a / b;
                        exp.RemoveAt(dIndex - 1);
                        exp.RemoveAt(dIndex - 1);
                        exp[dIndex - 1] = res.ToString(CultureInfo.InvariantCulture);
                    }
                }
            }

            // while expression is not reduced to single number
            while (exp.Count != 1)
            {
                if (exp.Count % 2 != 1)
                    throw new InvalidOperationException($"Incorrect expression: {string.Join("", exp)}");

                var a = ParseDouble(exp[0]);
                var b = ParseDouble(exp[2]);
                var op = exp[1];
                if (op == "-")
                {
                    res = a - b;
                }
                else if (op == "+")
                {
                    res = a + b;
                }
                else throw new InvalidOperationException($"Incorrect operation character: {op}");
                exp.RemoveAt(0);
                exp.RemoveAt(0);
                exp[0] = res.ToString(CultureInfo.InvariantCulture);
            }

            return exp[0];
        }

        private double ParseDouble(string number)
        {
            if (double.TryParse(number, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                return result;
            throw new ArgumentException($"Incorrect number to parse: {number}");
        }
    }
}
