using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace Nept
{
    public class TokenScanner
    {
        private class Pair<T, U>
        {
            private readonly T a;
            private readonly U b;

            public Pair(T a, U b)
            {
                this.a = a;
                this.b = b;
            }

            public T getA()
            {
                return a;
            }

            public U getB()
            {
                return b;
            }
        }
        private class Trair<T, U, V>
        {
            private readonly T a;
            private readonly U b;
            private readonly V c;

            public Trair(T a, U b, V c)
            {
                this.a = a;
                this.b = b;
                this.c = c;
            }

            public T getA()
            {
                return a;
            }

            public U getB()
            {
                return b;
            }

            public V getC()
            {
                return c;
            }
        }
        private List<string>[] ignore = new List<string>[256];
        private string dontIgnore = "";
        private List<Pair<string, string>>[] ignoreBlocks = new List<Pair<string, string>>[256];
        private bool ignoreWhitespace = true;
        private List<Regex>[] patterns = new List<Regex>[256];
        private List<string>[] operators = new List<string>[256];
        private string oneCharOperators = "";
        private List<Trair<string, string, char>>[] stringBlocks = new List<Trair<string, string, char>>[256];
        private List<Pair<char, string>> escapeCodes = new List<Pair<char, string>>();
        private List<Trair<char, int, int>> charEscapeCodes = new List<Trair<char, int, int>>();
        private bool allPunctuation = true;
        private string EOF = null;
        public TokenScanner()
        {
            { for (int i = 0; i < operators.Length; i++) operators[i] = new List<string>(); }
            { for (int i = 0; i < ignoreBlocks.Length; i++) ignoreBlocks[i] = new List<Pair<string, string>>(); }
            { for (int i = 0; i < ignore.Length; i++) ignore[i] = new List<string>(); }
            { for (int i = 0; i < stringBlocks.Length; i++) stringBlocks[i] = new List<Trair<string, string, char>>(); }
            { for (int i = 0; i < patterns.Length; i++) patterns[i] = new List<Regex>(); }
        }
        public TokenScanner Ignore(char chr)
        {
            Ignore("" + chr);
            return this;
        }
        public TokenScanner Ignore(String seq)
        {
            if (seq.Length == 1 && dontIgnore.IndexOf(seq[0]) >= 0)
                throw new ArgumentException("The character being ignored is already marked not to be ignored");

            ignore[seq[0]].Add(seq);
            return this;
        }
        public TokenScanner DontIgnore(char chr)
        {
            if (ignore[chr].Contains("" + chr))
                throw new ArgumentException("The character marked to not being ignored is already marked to be ignored");

            dontIgnore += chr;
            return this;
        }
        public TokenScanner IgnoreWhitespace(bool value)
        {
            ignoreWhitespace = value;
            return this;
        }
        public TokenScanner AppendOnEOF(string text)
        {
            EOF = text;
            return this;
        }
        public TokenScanner AddPatternRule(Regex p, params char[] startsWith)
        {
            Array.Sort(startsWith);
            foreach (char sw in startsWith)
                patterns[sw].Add(p);
            return this;
        }
        public TokenScanner AddOperatorRule(string op)
        {
            operators[op[0]].Add(op);
            return this;
        }
        public TokenScanner AddOperators(string operatorString)
        {
            oneCharOperators += operatorString;
            return this;
        }
        public TokenScanner AddStringRule(char start, char end, char escape)
        {
            stringBlocks[start].Add(new Trair<string, string, char>("" + start, "" + end, escape));
            return this;
        }
        public TokenScanner AddStringRule(string start, string end, char escape)
        {
            stringBlocks[start[0]].Add(new Trair<string, string, char>(start, end, escape));
            return this;
        }
        public TokenScanner AddEscapeCode(char code, string replacement)
        {
            escapeCodes.Add(new Pair<char, string>(code, replacement));
            return this;
        }
        public TokenScanner AddCharacterEscapeCode(char code, int numberOfDigits, int radix)
        {
            charEscapeCodes.Add(new Trair<char, int, int>(code, numberOfDigits, radix));
            return this;
        }
        public TokenScanner AddCommentRule(string start, string end)
        {
            ignoreBlocks[start[0]].Add(new Pair<string, string>(start, end));
            return this;
        }
        public TokenScanner SeparateIdentifiersAndPunctuation(bool value)
        {
            allPunctuation = value;
            return this;
        }
        public List<string> GetOperators()
        {
            List<string> newOperators = new List<string>();
            foreach (List<string> operatorList in operators)
                newOperators.AddRange(operatorList);
            foreach (char chr in oneCharOperators.ToCharArray())
                newOperators.Add(chr.ToString());
            return newOperators;
        }
        public TokenList Tokenize(string path)
        {
            string content = "";

            foreach (string s in File.ReadAllLines(path, Encoding.Default))
            {
                content += s + "\n";
            }
            return Tokenize(content, path);
        }
        public TokenList Tokenize(string source, string file)
        {
            return Tokenize(source, file, 1);
        }
        public TokenList Tokenize(string source, string file, int firstLine)
        {
            List<Token> tokens = new List<Token>();

            int line = firstLine;
            StringBuilder currToken = new StringBuilder();

            char[] oneCharOperatorsArr = oneCharOperators.ToCharArray();
            Array.Sort(oneCharOperatorsArr);

            char[] dontIgnoreArr = dontIgnore.ToCharArray();
            Array.Sort(dontIgnoreArr);

            int i = -1;
            outer: 
            while (i < source.Length - 1)
            {
                i++;

                if (source[i] == '\n') line++;
                if (ignoreWhitespace
                        && Char.IsWhiteSpace(source[i])
                        && Array.BinarySearch(dontIgnoreArr, source[i]) < 0)
                {
                    if (currToken.Length > 0) tokens.Add(new Token(currToken.ToString(), file, line));
                    currToken.Length = 0;
                    continue;
                }
                bool isBelow256 = source[i] < 256;

                if (isBelow256)
                {
                    foreach (string seq in ignore[source[i]])
                    {
                        if (source.Substring(i).StartsWith(seq))
                        {
                            if (currToken.Length > 0) tokens.Add(new Token(currToken.ToString(), file, line));
                            currToken.Length = 0;
                            i += seq.Length - 1;
                            goto outer;
                        }
                    }
                }
                if (isBelow256)
                {
                    foreach (Pair<string, string> block in ignoreBlocks[source[i]])
                    {
                        string startSeq = block.getA();
                        string endSeq = block.getB();
                        if (source.Substring(i).StartsWith(startSeq))
                        {
                            if (currToken.Length > 0) tokens.Add(new Token(currToken.ToString(), file, line));
                            currToken.Length = 0;

                            i += startSeq.Length - 1;

                            while (true)
                            {
                                i++;
                                if (source.Substring(i).StartsWith(endSeq))
                                {
                                    i += endSeq.Length - 1;
                                    goto outer;
                                } else
                                {
                                    if (source.Length <= i)
                                        throw new ParsingException("Unexpected EOF in the middle of a comment",
                                                new Token(EOF, file, line));
                                    if (source[i] == '\n')
                                        line++;
                                }
                            }
                        }
                    }
                }
                if (isBelow256)
                {
                    foreach (Regex p in patterns[source[i]])
                    {
                        MatchCollection m = p.Matches(source.Substring(i));
                        if (m.Count != 0)
                        {
                            if (currToken.Length > 0) tokens.Add(new Token(currToken.ToString(), file, line));
                            currToken.Length = 0;
                            tokens.Add(new Token(source.Substring(i, i + m.Count), file, line));
                            i = i + m.Count - 1;
                            goto outer;
                        }
                    }
                }
                if (isBelow256)
                {
                    foreach (Trair<string, string, char> block in stringBlocks[source[i]])
                    {
                        String startSeq = block.getA();
                        String endSeq = block.getB();
                        char escapeChar = block.getC();
                        if (source.Substring(i).StartsWith(startSeq))
                        {
                            if (currToken.Length > 0) tokens.Add(new Token(currToken.ToString(), file, line));
                            currToken.Length = 0;

                            StringBuilder str = new StringBuilder();
                            tokens.Add(new Token(startSeq, file, line));

                            i += startSeq.Length - 1;

                            stringLoop:
                            while (true)
                            {
                                i++;

                                if (source.Length > i && source[i] == '\n') line++;

                                if (escapeChar != '\0' && source.Length > i && source[i] == escapeChar)
                                {
                                    if (source.Substring(i + 1).StartsWith(endSeq))
                                    {
                                        str.Append(endSeq);
                                        i += endSeq.Length;
                                        continue;
                                    } else if (source.Length >= i + 2)
                                    {
                                        foreach (Pair<char, string> escapeCode in escapeCodes)
                                        {
                                            if (source[i + 1] == escapeCode.getA())
                                            {
                                                str.Append(escapeCode.getB());
                                                i++;
                                                goto stringLoop;
                                            }
                                        }
                                        foreach (Trair<char, int, int> escapeCode in charEscapeCodes)
                                        {
                                            if (source[i + 1] == escapeCode.getA())
                                            {
                                                String characterCode = "";
                                                for (int j = 2; j < escapeCode.getB() + 2; j++)
                                                {
                                                    characterCode += source[i + 2];
                                                    i++;
                                                }
                                                str.Append((char) Convert.ToInt32(characterCode, escapeCode.getC()));
                                                i++;
                                                goto stringLoop;
                                            }
                                        }
                                        throw new ParsingException("Invalid escape sequence '"
                                                + escapeChar + source[i + 1] + "'",
                                                new Token(block.getA() + str, file, line));
                                    }
                                }
                                if (source.Substring(i).StartsWith(endSeq))
                                {
                                    tokens.Add(new Token(str.ToString(), file, line));
                                    tokens.Add(new Token(endSeq, file, line));
                                    i += endSeq.Length - 1;
                                    goto outer;
                                } else if (source.Length > i)
                                {
                                    str.Append(source[i]);
                                } else
                                {
                                    throw new ParsingException("Unexpected EOF in the middle of a string constant",
                                            new Token(EOF, file, line));
                                }
                            }
                        }
                    }
                }
                if (isBelow256)
                {
                    foreach (String op in operators[source[i]])
                    {
                        if (source.Substring(i).StartsWith(op))
                        {
                            if (currToken.Length > 0) tokens.Add(new Token(currToken.ToString(), file, line));
                            currToken.Length = 0;
                            tokens.Add(new Token(op, file, line));
                            i = i + op.Length - 1;
                            goto outer;
                        }
                    }
                }
                if (Array.BinarySearch(oneCharOperatorsArr, source[i]) >= 0)
                {
                    if (currToken.Length > 0) tokens.Add(new Token(currToken.ToString(), file, line));
                    currToken.Length = 0;
                    tokens.Add(new Token(source[i].ToString(), file, line));
                    goto outer;
                }
                if (allPunctuation && !Char.IsLetter(source[i]) && !Char.IsDigit(source[i]))
                {
                    if (currToken.Length > 0) tokens.Add(new Token(currToken.ToString(), file, line));
                    currToken.Length = 0;
                    tokens.Add(new Token("" + source[i], file, line));
                    i += 0;
                    goto outer; 
                }
                currToken.Append(source[i]);
            }
            if (currToken.Length > 0) tokens.Add(new Token(currToken.ToString(), file, line));

            if (!string.IsNullOrEmpty(EOF))
                tokens.Add(new Token(EOF, file, line));
            return new TokenList(tokens);
        }
    }
}