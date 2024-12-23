﻿using ExpressionEvaluator;

IEvaluator evaluator = new Evaluator();

Console.WriteLine("Enter expression. Decimal numbers must be dot-separated.");
var expression = string.Empty;
while ((expression = Console.ReadLine()) != "exit")
{
    if (string.IsNullOrEmpty(expression))
        Console.WriteLine("Empty expression");
    else
    {
        try
        {
            Console.WriteLine(evaluator.EvaluateExpression(expression));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}