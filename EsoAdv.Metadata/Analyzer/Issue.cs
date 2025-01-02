namespace EsoAdv.Metadata.Analyzer;

public class Issue
{
    /// <summary>Reference by name to addon</summary>
    public string AddOnRef { get; set; }

    /// <summary>Description of the issue</summary>
    public string Message { get; set; }

    /// <summary>Level of severity</summary>
    public IssueSeverity Severity { get; set; }
}