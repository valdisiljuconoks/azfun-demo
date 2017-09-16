namespace Shared.Models {
    public class ProcessingRequest
    {
        public string FileId { get; set; }
        public byte[] Content { get; set; }
    }
}