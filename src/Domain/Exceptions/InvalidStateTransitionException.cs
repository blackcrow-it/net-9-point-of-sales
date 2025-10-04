namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when an invalid state transition is attempted
/// </summary>
public class InvalidStateTransitionException : DomainException
{
    public string CurrentState { get; }
    public string AttemptedState { get; }

    public InvalidStateTransitionException(string currentState, string attemptedState)
        : base($"Cannot transition from '{currentState}' to '{attemptedState}'.", "INVALID_STATE_TRANSITION")
    {
        CurrentState = currentState;
        AttemptedState = attemptedState;
    }

    public InvalidStateTransitionException(string currentState, string attemptedState, string reason)
        : base($"Cannot transition from '{currentState}' to '{attemptedState}'. Reason: {reason}", "INVALID_STATE_TRANSITION")
    {
        CurrentState = currentState;
        AttemptedState = attemptedState;
    }
}
