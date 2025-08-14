using System;
using System.Threading.Tasks;

namespace UbiqSecurity.Internals.Structured.Pipeline.Operations
{
    internal class EncryptOperation : IOperation
    {
        public async Task<string> InvokeAsync(OperationContext context)
        {
            if (!context.IsEncrypt)
            {
                throw new InvalidOperationException("EncryptOperation not allowed in a decryption pipeline");
            }

            if (context.CurrentValue.Length < context.Dataset.MinInputLength)
            {
                throw new ArgumentException("Input length is less than the dataset's minimum input length.");
            }

            if (context.CurrentValue.Length > context.Dataset.MaxInputLength)
            {
                throw new ArgumentException("Input length is greater than the dataset's maximum input length.");
            }

            foreach (var c in context.CurrentValue)
            {
                if (context.Dataset.InputCharacters.IndexOf(c) == -1)
                {
                    throw new ArgumentOutOfRangeException($"Trimmed input string has invalid character:  '{c}'");
                }
            }

            var ffx = await context.FfxCache.GetAsync(context.Dataset, context.KeyNumber);

            context.KeyNumber = ffx.KeyNumber;

            return ffx.Cipher(context.Dataset.EncryptionAlgorithm, context.CurrentValue, context.UserSuppliedTweak, true);
        }
    }
}
