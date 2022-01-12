using ExpressionEvaluator;

IEvaluator evaluator = new Evaluator();

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