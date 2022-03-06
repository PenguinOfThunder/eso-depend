namespace EsoAdv.Metadata.Model
{
    public class AnalyzerSettings
    {
        public bool CheckAddOnVersion = true;
        public bool CheckOutdated = true;
        public bool CheckProvidedFiles = true;
        public bool CheckOptionalDependsOn = true;
        public bool CheckMultipleInstances = true;
        internal bool CheckDependsOn = true;
    }
}
