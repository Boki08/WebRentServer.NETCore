using System.Text;
using System.Security.Cryptography;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System.Dynamic;
using WebRentServer.NETCore.Models;
using Microsoft.Extensions.Configuration.UserSecrets;
using System.Reflection;

namespace WebRentServer.NETCore.Encrypting
{
	public enum AlgorithmType
	{
		DES = 0,
		TripleDES = 1,
		AES = 2
	};

	public static class SecretKey
	{
		# region Generate Secret Key

		/// <summary>
		/// Generate a symmetric key for the specific symmetric algorithm. IV is generated automatically.
		/// </summary>
		/// <param name="algorithmType"> type of symmetric algorith the key is generated for </param>
		/// <returns> string value representing a symmetric key </returns>
		public static string GenerateKey(AlgorithmType algorithmType)
		{
			SymmetricAlgorithm symmAlgorithm = null;

			switch (algorithmType)
			{
				case AlgorithmType.DES: symmAlgorithm = DESCryptoServiceProvider.Create(); break;
				case AlgorithmType.TripleDES: symmAlgorithm = TripleDESCryptoServiceProvider.Create(); break;
				case AlgorithmType.AES: symmAlgorithm = AesCryptoServiceProvider.Create(); break;
				default: Console.WriteLine("SecretKeys.GenerateKey:: Unknown Symmetric Algorithm Type {0}", algorithmType.ToString()); break;
			}

			return symmAlgorithm == null ? String.Empty : ASCIIEncoding.ASCII.GetString(symmAlgorithm.Key);
		}

		#endregion

		#region Store Secret Key

		/// <summary>
		/// Store a secret key as string value in a specified file.
		/// </summary>
		/// <param name="secretKey"> a symmetric key value </param>
		/// <param name="outFile"> file location to store a secret key </param>
		public static bool StoreKey(string secretKey, string itemName)
		{
			try
            { //Get secrets.json
                var secretsId = Assembly.GetExecutingAssembly().GetCustomAttribute<UserSecretsIdAttribute>().UserSecretsId;
                var secretsPath = PathHelper.GetSecretsPathFromSecretsId(secretsId);
                var secretsJson = File.ReadAllText(secretsPath);

                var jsonSettings = new JsonSerializerSettings();
				jsonSettings.Converters.Add(new ExpandoObjectConverter());
				jsonSettings.Converters.Add(new StringEnumConverter());

				dynamic config = JsonConvert.DeserializeObject<ExpandoObject>(secretsJson, jsonSettings);

				var expando = config as IDictionary<string, object>;

				if (!expando.ContainsKey(itemName))
				{
					expando.Add(itemName, new AESConfig() { Key = secretKey });
					var newJson = JsonConvert.SerializeObject(config, Formatting.Indented, jsonSettings);

					File.WriteAllText(secretsPath, newJson);
				}
				return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("SecretKeys.LoadKey:: ERROR {0}", e.Message);
                return false;
			}
		}

		# endregion
	}
}
