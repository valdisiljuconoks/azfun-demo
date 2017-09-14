namespace Shared.Models
{
    public class AsciiArtResult
    {
        public AsciiArtResult(string blobRef, string description, string[] tags)
        {
            BlobRef = blobRef;
            Description = description;
            Tags = tags;
        }

        public string BlobRef { get; }
        public string Description { get; }
        public string[] Tags { get; }
    }
}
