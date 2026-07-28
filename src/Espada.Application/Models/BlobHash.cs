namespace Espada.Application.Models
{
    public readonly record struct BlobHash
    {
        public BlobHash(string value)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            if (value.Length != 64 || value.Any(character => !Uri.IsHexDigit(character)))
            {
                throw new ArgumentException("Blob hash must be a 64-character SHA-256 hex value.", nameof(value));
            }

            Value = value.ToLowerInvariant();
        }

        public string Value { get; }

        public override string ToString()
        {
            return Value;
        }
    }
}