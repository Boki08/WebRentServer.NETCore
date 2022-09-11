
namespace WebRentServer.NETCore.Encrypting
{
	public class Formatter
	{
		const int headerLenght = 54;

		/// <summary>
		/// Splits image bytes to the header and body
		/// </summary>
		public static void Decompose(byte[] bytePicture, out byte[] header, out byte[] body)
		{
			header = new byte[headerLenght];
			body = new byte[bytePicture.Length - headerLenght];

			for (int i = 0; i < bytePicture.Length; i++)
			{
				if (i < headerLenght)
				{ 
					header[i] = bytePicture[i];
				}
				else
				{
					body[i - headerLenght] = bytePicture[i];
				}
			}
		}

		/// <summary>
		///  Links the image header and body together into one array
		/// </summary>
		public static void Compose(byte[] header, byte[] body, int outputLenght, string outFile)
		{
			byte[] output = new byte[outputLenght];

			for (int i = 0; i < outputLenght; i++)
			{
				if (i < headerLenght)
				{ 
					output[i] = header[i];
				}
				else
				{
					output[i] = body[i - headerLenght];
				}
			}
			
			BinaryWriter Writer = new BinaryWriter(File.OpenWrite(outFile));
			Writer.Write(output);		// Writer raw data    
			Writer.Flush();
			Writer.Close();
		}

        public static void ComposeBytes(byte[] header, byte[] body, int outputLenght, out byte[] outFile)
        {
            byte[] output = new byte[outputLenght];

            for (int i = 0; i < outputLenght; i++)
            {
                if (i < headerLenght)
                {
                    output[i] = header[i];
                }
                else
                {
                    output[i] = body[i - headerLenght];
                }
            }

            outFile = output;
        }
    }
}
