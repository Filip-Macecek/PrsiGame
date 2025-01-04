namespace PrsiGame.Common;

public static class EnumerableExtensions
{
    public static Stack<T> MakeCopy<T>(this Stack<T> stack)
    {
        var newStack = new Stack<T>();

        foreach (var item in stack.Reverse())
        {
            newStack.Push(item);
        }

        return newStack;
    }
}
