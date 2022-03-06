namespace EsoAdv.Metadata.Model
{
    public class Issue
    {
        /// <summary>Reference by name to addon</summary>
        public string AddOnRef { get; set; }
        /// <summary>Description of the issue</summary>
        public string Message { get; set; }
        /// <summary>Level of severity</summary>
        public IssueSeverity Severity { get; set; }
    }
}