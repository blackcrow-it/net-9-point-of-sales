namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when a business rule is violated
/// </summary>
public class BusinessRuleViolationException : DomainException
{
    public string RuleName { get; }

    public BusinessRuleViolationException(string ruleName, string message)
        : base(message, "BUSINESS_RULE_VIOLATION")
    {
        RuleName = ruleName;
    }

    public BusinessRuleViolationException(string ruleName, string message, Exception innerException)
        : base(message, "BUSINESS_RULE_VIOLATION", innerException)
    {
        RuleName = ruleName;
    }
}
