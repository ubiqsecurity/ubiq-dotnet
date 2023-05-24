using Org.BouncyCastle.OpenSsl;

namespace UbiqSecurity.Model
{
    // model-related BouncyCastle helper class
    internal class PasswordFinder : IPasswordFinder
    {
        private readonly string _password;

        internal PasswordFinder(string password)
        {
            _password = password;
        }

        public char[] GetPassword() => _password.ToCharArray();
    }
}
