namespace UbiqSecurity.Internals.Structured.Ciphers
{
    internal interface ICipher
    {
        void Prf(byte[] key, byte[] src, byte[] dest);

        void Prf(byte[] key, byte[] src, int srcOffset, byte[] dest, int destOffset, int length);

        void Ciph(byte[] key, byte[] src, int srcOffset, byte[] dest, int destOffset);
    }
}
