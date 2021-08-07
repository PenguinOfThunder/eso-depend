namespace EsoAdv.Metadata.Model
{

    public enum IssueSeverity
    {
        Info,
        Warning,

        Error
    }

    public class Issue
    {
        public string AddOnRef { get; set; }
        public string Message { get; set; }
        public IssueSeverity Severity { get; set; }
    }
}