using System.Globalization;
using System.Text.RegularExpressions;

namespace Interpreter
{
    public class TokenStream
    {

        public delegate bool ConditionDelegate(char ch);
        InputStream inputStream;

        Token currentToken;
        public List<Token> tokens = new List<Token>();

        private static readonly Dictionary<String, TokenType> keywords = new Dictionary<string, TokenType>
        {
            { "null", TokenType.NULL},
            { "void", TokenType.VOID},
            { "if", TokenType.IF},
            { "else", TokenType.ELSE},
            { "true", TokenType.TRUE},
            { "false", TokenType.FALSE},
            { "for", TokenType.FOR},
            { "while", TokenType.WHILE},
            { "print", TokenType.PRINT},
            { "var", TokenType.VAR },
            { "or", TokenType.OR },
            { "fun", TokenType.FUN },
            { "and", TokenType.AND }
        };

        public TokenStream(InputStream inputStream)
        {
            this.inputStream = inputStream;
        }

        #region Condtions Methods
        public bool IsWhitespace(char ch)
        {
            return " \t\n\r".Contains(ch);
        }

        public bool IsComment(char ch)
        {
            return !("\n".Contains(ch));
        }

        public bool IsDigit(char ch)
        {
            Regex digitRegex = new Regex("[0-9]");
            return digitRegex.IsMatch(ch.ToString());
        }

        public bool IsStartOfIndentifier(char ch)
        {
            Regex digitRegex = new Regex("[a-zA-Z_]");
            return digitRegex.IsMatch(ch.ToString());
        }

        public bool IsIndentifier(char ch)
        {
            return IsStartOfIndentifier(ch) || "0123456789".Contains(ch);
        }

        public bool IsOperator(char ch)
        {
            return "+-*/%=&|<>!".Contains(ch);
        }

        public bool IsPunctuation(char ch)
        {
            return ",;(){}[]".Contains(ch);
        }

        #endregion 

        #region Read Methods
        public string ReadWhile(ConditionDelegate condition)
        {
            string str = "";
            while (!inputStream.Eof() && condition(inputStream.Peek()))
                str += inputStream.Next();
            return str;
        }

        public void ReadStringWithEscape(char endChar)
        {
            bool escape = false;
            string str = "";

            str += inputStream.Next(); //Consumes the starting " character 

            while (inputStream.Peek() != '"' && !inputStream.Eof())
            {
                var ch = inputStream.Next();
                if (escape)
                {
                    str += ch;
                    escape = false;
                }
                else if (ch.Equals("\\"))
                {
                    escape = true;
                }
                else
                {
                    str += ch;
                }
            }

            if (inputStream.Eof())
            {
                ErrorThrower.InputError("Unterminated string! ", inputStream.Line, inputStream.Column);
            }

            str += inputStream.Next(); //Consumes the closing " character 

            CreateToken(TokenType.STRING, str, str.Substring(1, str.Length - 2));

        }

        public void ReadNumber()
        {
            string str = "";

            str += ReadWhile(IsDigit);

            if (!inputStream.Eof() && inputStream.Peek() == '.' && IsDigit(inputStream.PeekNext()))
            {
                str += inputStream.Next(); // Consume .
                str += ReadWhile(IsDigit);
            }

            CreateToken(TokenType.NUMBER, str, Double.Parse(str, CultureInfo.InvariantCulture));

        }

        public void ReadIdentifier()
        {
            string word = ReadWhile(IsIndentifier);


            TokenType type;
            if (!keywords.TryGetValue(word, out type))
                type = TokenType.IDENTIFIER;
            //keywords.TryGetValue(word, out type);
            //= keywords.GetValueOrDefault(word, TokenType.IDENTIFIER);

            CreateToken(type, word);

        }

        public string ReadTwoCharacterOperator(char expected)
        {
            string str = "";
            str += inputStream.Next();

            if (inputStream.Peek().Equals(expected))
                return (str += inputStream.Next());

            return str;
        }

        #endregion

        public void ReadTokens()
        {

            while (!inputStream.Eof())
            {
                ReadWhile(IsWhitespace);
                char ch = inputStream.Peek();

                switch (ch)
                {
                    case '\"':
                        ReadStringWithEscape('"');
                        break;
                    case '(': CreateToken(TokenType.LEFT_PARENTHESIS, inputStream.Next().ToString()); break;
                    case ')': CreateToken(TokenType.RIGHT_PARENTHESIS, inputStream.Next().ToString()); break;
                    case '{': CreateToken(TokenType.LEFT_BRACE, inputStream.Next().ToString()); break;
                    case '}': CreateToken(TokenType.RIGHT_BRACE, inputStream.Next().ToString()); break;
                    case ',': CreateToken(TokenType.COMMA, inputStream.Next().ToString()); break;
                    case '.': CreateToken(TokenType.DOT, inputStream.Next().ToString()); break;
                    case '-': CreateToken(TokenType.MINUS, inputStream.Next().ToString()); break;
                    case '+': CreateToken(TokenType.PLUS, inputStream.Next().ToString()); break;
                    case ';': CreateToken(TokenType.SEMICOLON, inputStream.Next().ToString()); break;
                    case '*': CreateToken(TokenType.STAR, inputStream.Next().ToString()); break;
                    case '!':
                        CreateToken(inputStream.PeekNext().Equals('=') ? TokenType.EXCLAMATION_EQUAL : TokenType.EXCLAMATION, ReadTwoCharacterOperator('='));
                        break;
                    case '=':
                        CreateToken(inputStream.PeekNext().Equals('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL, ReadTwoCharacterOperator('='));
                        break;
                    case '<':
                        CreateToken(inputStream.PeekNext().Equals('=') ? TokenType.LESS_EQUAL : TokenType.LESS, ReadTwoCharacterOperator('='));
                        break;
                    case '>':
                        CreateToken(inputStream.PeekNext().Equals('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER, ReadTwoCharacterOperator('='));
                        break;
                    case '/':
                        if (inputStream.PeekNext().Equals('/'))
                            ReadWhile(IsComment);
                        else
                            CreateToken(TokenType.SLASH, inputStream.Next().ToString());
                        break;
                    default:
                        if (IsDigit(ch))
                            ReadNumber();
                        else if (IsStartOfIndentifier(ch))
                            ReadIdentifier();
                        else
                            ErrorThrower.InputError("Can't handle character: " + ch, inputStream.Line, inputStream.Column);
                        break;
                }

            }

            CreateToken(TokenType.EOF, "", null);

        }

        private void CreateToken(TokenType type, string value)
        {
            CreateToken(type, value, null);
        }

        private void CreateToken(TokenType type, string value, Object literalValue)
        {
            currentToken = new Token(type, value, literalValue, inputStream.Line);
            tokens.Add(currentToken);
        }


    }
}