namespace NAPS2.Scan.Experimental.Internal
{
    public interface IScanEvents
    {
        // This only includes events that can't be otherwise inferred.
        void PageStart();
        void PageProgress(double progress);
    }
}
