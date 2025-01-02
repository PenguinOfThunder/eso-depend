namespace EsoAdv.Metadata.Analyzer;

public enum IssueSeverity
{
    /// <summary>Information level issue (probably fine, but could break in the future)</summary>
    Info,

    /// <summary>Warning level issue (should fix or it will break eventually)</summary>
    Warning,

    /// <summary>Error level issue (must fix or something is broken)</summary>
    Error
}