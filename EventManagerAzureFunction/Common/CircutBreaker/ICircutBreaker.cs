namespace Common.CircutBreaker
{
    public interface ICircutBreaker
    {
        void BreakFlow(int exceptionsAllowed, string functionName);
    }
}