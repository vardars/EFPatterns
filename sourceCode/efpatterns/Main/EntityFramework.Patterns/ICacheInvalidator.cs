namespace EntityFramework.Patterns
{
    public interface ICacheInvalidator
    {
        bool IsValid { get; }
    }
}