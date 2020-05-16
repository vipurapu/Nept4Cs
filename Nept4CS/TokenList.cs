using System.Collections.Generic;
using System.Text;

namespace Nept
{
    public class TokenList
    {
        private List<Token> tokens;
        private int index;
        public TokenList(List<Token> tokens)
        {
            this.tokens = tokens;
            this.index = 0;
        }
        public void Shift()
        {
            index++;
            if (index >= tokens.Count)
                throw new System.IndexOutOfRangeException();
        }
        public Token Next()
        {
            return tokens[index++];
        }
        public string NextString()
        {
            return tokens[index++].GetToken();
        }
        public Token Seek()
        {
            return tokens[index];
        }
        public void ShiftBack()
        {
            index--;
            if (index < 0)
            {
                throw new System.IndexOutOfRangeException();
            }
        }
        public Token SeekPrevious()
        {
            return tokens[index - 2];
        }
        public string SeekPreviousString()
        {
            return tokens[index - 2].GetToken();
        }
        public Token Seek(int n)
        {
            return tokens[index + n];
        }
        public Token SeekPrevious(int n)
        {
            return tokens[index - n];
        }
        public string SeekPreviousString(int n)
        {
            return tokens[index - n].GetToken();
        }
        public string SeekString()
        {
            return tokens[index].GetToken();
        }
        public bool HasNext()
        {
            return index < tokens.Count;
        }
        public bool Has(int number)
        {
            return index + number - 1 < tokens.Count;
        }
        public bool IsNext(params string[] keyword)
        {
            return HasNext() ? new List<string>(keyword).Contains(Seek().GetToken()) : false;
        }
        public Token Accept(params string[] keyword)
        {
            Token next = Next();
            if (!new List<string>(keyword).Contains(next.GetToken()))
                throw new ParsingException(Expected(keyword), next);
            return next;
        }

        public bool AcceptIfNext(params string[] keyword)
        {
            bool isItNext = IsNext(keyword);
            if (isItNext) Shift();
            return isItNext;
        }
        public static string Expected(params string[] keyword)
        {
            if (keyword.Length == 1)
                return "Expected '" + keyword[0] + "'";
            System.Text.StringBuilder builder = new System.Text.StringBuilder("Expected '");
            int i = 0;
            for (; i < (keyword.Length - 1); i++)
            {
                builder.Append(keyword[i]).Append("', '");
            }
            return builder.ToString() + "or '" + keyword[i] + "'";
        }
        public void Reset()
        {
            index = 0;
        }
        public TokenList Append(Token token)
        {
            tokens.Add(token);
            return this;
        }
        public IReadOnlyCollection<Token> ToList()
        {
            return tokens.AsReadOnly();
        }
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder("[");
            int i = 0;
            for ( ; i < (tokens.Count - 1); i++)
            {
                builder.Append(tokens[i].GetToken() + ", ");
            }
            return builder.Append(tokens[i] + "]").ToString();
        }
    }
}
