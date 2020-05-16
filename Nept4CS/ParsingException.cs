using System;
using System.Runtime.Serialization;

namespace Nept
{
	public class ParsingException : Exception, ISerializable
	{
		string msg;
		public override string Message { get; }
		Token token;
		public ParsingException(string msg, Token token)
		{
			this.msg = msg;
			this.token = token;
			this.Message = "Syntax error on token '" + token.GetToken() + "' in " + token.GetFile() + ":" + token.GetLine() + ": " + msg;
		}
		public override string ToString()
		{
			return "Syntax error on token '" + token.GetToken() + "' in " + token.GetFile() + ":" + token.GetLine() + ": " + msg;
		}
	}
}
