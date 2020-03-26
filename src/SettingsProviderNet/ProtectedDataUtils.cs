using System;
using System.Security.Cryptography;
using System.Text;

namespace SimpleSettingsStorage
{
  internal class ProtectedDataUtils
  {
    /// <summary>
    /// Encrypts the specified value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="entropy">The entropy.</param>
    /// <returns></returns>
    static public string Encrypt(string value, string entropy)
    {
      return Encrypt(value, Encoding.UTF8.GetBytes(entropy));
    }

    /// <summary>
    /// Encrypts the specified value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="entropyBytes">The entropy bytes.</param>
    /// <returns></returns>
    public static string Encrypt(string value, byte[] entropyBytes)
    {
      try
      {
        byte[] valueBytes = Encoding.UTF8.GetBytes(value);
        byte[] cipherBytes = ProtectedData.Protect(valueBytes, entropyBytes, DataProtectionScope.LocalMachine);

        return Convert.ToBase64String(cipherBytes);
      }
      catch (FormatException)
      {
        return String.Empty;
      }
      catch (CryptographicException)
      {
        return String.Empty; //for positive result, if entropy incorrect 
      }
    }

    /// <summary>
    /// Decrypts the specified cipher.
    /// </summary>
    /// <param name="cipher">The cipher.</param>
    /// <param name="entropy">The entropy.</param>
    /// <returns></returns>
    static public string Decrypt(string cipher, string entropy)
    {
      return Decrypt(cipher, Encoding.UTF8.GetBytes(entropy));
    }

    /// <summary>
    /// Decrypts the specified cipher.
    /// </summary>
    /// <param name="cipher">The cipher.</param>
    /// <param name="entropyBytes">The entropy bytes.</param>
    /// <returns></returns>
    public static string Decrypt(string cipher, byte[] entropyBytes)
    {
      try
      {
        byte[] cipherBytes = Convert.FromBase64String(cipher);
        byte[] valueBytes = ProtectedData.Unprotect(cipherBytes, entropyBytes, DataProtectionScope.LocalMachine);

        return Encoding.UTF8.GetString(valueBytes);
      }
      catch (FormatException)
      {
        return String.Empty;
      }
      catch (CryptographicException)
      {
        return String.Empty; //for positive result, if entropy incorrect
      }
    }
  }
}
