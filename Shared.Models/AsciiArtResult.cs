namespace Shared.Models
{
    public class AsciiArtResult
    {
        public AsciiArtResult(string resultBlobRef, string origBlobRef, string description, string[] tags)
        {
            ResultBlobRef = resultBlobRef;
            OrigBlobRef = origBlobRef;
            Description = description;
            Tags = tags;
        }

        public string ResultBlobRef { get; }
        public string OrigBlobRef { get; }
        public string Description { get; }
        public string[] Tags { get; }
    }
}
