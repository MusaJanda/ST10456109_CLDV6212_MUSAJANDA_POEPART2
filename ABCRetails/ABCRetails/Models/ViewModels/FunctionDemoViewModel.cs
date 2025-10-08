namespace ABCRetails.Models
{
    // Model for displaying results from the function calls in the demo view.
    public class FunctionDemoViewModel
    {
        public string CustomerIdForLookup { get; set; } = "1234"; // Mock ID for lookup
        public string TableResult { get; set; } = "Not Run";
        public string BlobResult { get; set; } = "Not Run";
        public string FileShareResult { get; set; } = "Not Run";
    }
}
