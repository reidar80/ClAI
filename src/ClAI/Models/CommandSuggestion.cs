namespace ClAI.Models;

/// <summary>
/// Represents a command suggestion from the AI.
/// </summary>
public class CommandSuggestion
{
    /// <summary>
    /// The suggested command to execute.
    /// </summary>
    public required string Command { get; set; }

    /// <summary>
    /// Explanation of what the command does.
    /// </summary>
    public required string Explanation { get; set; }

    /// <summary>
    /// Whether the command is potentially dangerous.
    /// </summary>
    public bool IsDangerous { get; set; }

    /// <summary>
    /// Warning message if the command is dangerous.
    /// </summary>
    public string? Warning { get; set; }
}
