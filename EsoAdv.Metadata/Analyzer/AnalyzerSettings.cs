namespace EsoAdv.Metadata.Analyzer
{
    public class AnalyzerSettings
    {
        public bool CheckAddOnVersion = true;
        public bool CheckOutdated = true;
        public bool CheckProvidedFiles = true;
        public bool CheckOptionalDependsOn = true;
        public bool CheckMultipleInstances = true;
        public bool CheckDependsOn = true;
        public bool CheckUnused { get; set; } = true;
    }
}
