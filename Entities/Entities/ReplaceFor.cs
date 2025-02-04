using CryptographCredentials.Domain.Enums;

namespace CryptographCredentials.Domain.Entities
{
    public class ReplaceFor
    {
        #region | Properties |
        public bool Secret { get; set; }
        public bool Hash { get; set; }
        public bool Whitespace { get; set; }
        #endregion

        #region | Methods |
        /// <summary>
        /// Gets the replacement options
        /// Throws an exception if more than one option is set to true
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public EReplaceOptions? GetActiveOption()
        {
            // Counts how many options are true
            int trueCount = (Secret ? 1 : 0) + (Hash ? 1 : 0) + (Whitespace ? 1 : 0);

            // If more than one option is true, throws an exception
            if (trueCount > 1)
                throw new InvalidOperationException("More than one replacement option is enabled.");

            if (trueCount == 1)
            {
                if (Secret) return EReplaceOptions.Secret;
                if (Hash) return EReplaceOptions.Hash;
                if (Whitespace) return EReplaceOptions.Whitespace;
            }

            return null;
        }
        #endregion
    }
}
