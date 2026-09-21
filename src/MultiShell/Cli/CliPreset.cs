using MultiShell.Models;

namespace MultiShell.Cli;

public sealed record CliPreset(
    string Id,
    string Name,
    string Category,
    IReadOnlyList<string> ExecutableCandidates,
    string StandardStartArguments,
    string StandardResumeArguments,
    string? FullStartArguments = null,
    string? FullResumeArguments = null,
    bool ResumeSupported = true,
    bool IsCustom = false)
{
    public bool SupportsFullApproval =>
        FullStartArguments is not null && FullResumeArguments is not null;

    public string GetArguments(bool resume, ApprovalMode mode)
    {
        if (mode == ApprovalMode.Full && !SupportsFullApproval)
        {
            throw new InvalidOperationException($"{Name} does not expose a session-local Full Approval mode.");
        }

        if (mode == ApprovalMode.Full)
        {
            return resume && ResumeSupported
                ? FullResumeArguments!
                : FullStartArguments!;
        }

        return resume && ResumeSupported
            ? StandardResumeArguments
            : StandardStartArguments;
    }
}
