using System.IO;

namespace CapRecAnimConv
{
	public static class ReaderExtender
	{
		public static string ReadStringToNull(this BinaryReader br, int maxsize = -1)
		{
			string output = "";
			char c;

			int maximum = maxsize;

			if (maximum == -1)
				maximum = (int)br.BaseStream.Length - (int)br.BaseStream.Position;

			for (int j = 0; j < maximum; j++)
			{
				c = br.ReadChar();
				if (c == 0)
				{
					if (maxsize != -1)
						br.BaseStream.Position += maximum - 1 - j;
					break;
				}

				output += c.ToString();
			}

			return output;
		}
	}
}
