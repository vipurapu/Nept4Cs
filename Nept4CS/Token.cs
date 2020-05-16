namespace Nept
{
    public class Token
    {
		private string token;
		private string file;
		private int line;

		public Token(string token, string file, int line)
		{
			this.token = token;
			this.file = file;
			this.line = line;
		}
		public string GetToken()
		{
			return token;
		}
		public string GetFile()
		{
			return file;
		}
		public int GetLine()
		{
			return line;
		}
		public override string ToString()
		{
			return token;
		}
	}
}
