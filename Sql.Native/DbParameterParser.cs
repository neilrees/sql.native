using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sql.Native
{
    public class DbParameterParser
    {
        #region Fields

        private const char SingleQuote = '\'';
        private readonly StringReader reader;
        private int current = -2;

        #endregion

        private DbParameterParser(string sql)
        {
            reader = new StringReader(sql);
        }

        private char Current
        {
            get
            {
                if (current == -1)
                    throw new InvalidOperationException();
                return (char)current;
            }
        }

        private bool IsEof
        {
            get
            {
                return current == -1;
            }
        }

        private void SkipString()
        {
            var previous = SingleQuote;
            while (Read())
            {
                // check for '' in string
                if (previous == SingleQuote && Current == SingleQuote)
                {
                    if ((char)reader.Peek() != SingleQuote)
                    {
                        return;
                    }

                    // skip next character
                    if (!Read())
                    {
                        return;
                    }

                    previous = (char)0;
                }
                else if (Current == SingleQuote)
                {
                    return;
                }
                else
                {
                    previous = Current;
                }
            }
        }

        private string ReadParameter()
        {
            // must be at least one character length
            if (!Read())
            {
                throw new FormatException("Invalid parameter name.");
            }

            // must start with a letter
            if (!Char.IsLetter(Current))
            {
                throw new FormatException("Invalid parameter name.");
            }

            // collect name
            var output = new StringBuilder();
            do
            {
                output.Append(Current);
                Read();
            } while (!IsEof && (Char.IsLetterOrDigit(Current) || Current == '_'));

            return output.ToString();
        }

        private bool Read()
        {
            current = reader.Read();
            return current != -1;
        }

        public static IEnumerable<string> ParseParameters(string sql)
        {
            var parser = new DbParameterParser(sql);
            return parser.Parse();
        }

        private IEnumerable<string> Parse()
        {
            while (Read())
            {
                switch (Current)
                {
                    case '\'':
                        SkipString();
                        break;
                    case '@':
                        yield return ReadParameter();
                        break;
                }
            }
        }
    }
}