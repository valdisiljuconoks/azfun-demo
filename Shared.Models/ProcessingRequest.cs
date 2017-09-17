namespace Shared.Models {
    public class ProcessingRequest
    {
        public ProcessingRequest()
        {
            Width = 100;
        }

        public string FileId { get; set; }

        public int Width { get; set; }

        public byte[] Content { get; set; }
    }
}