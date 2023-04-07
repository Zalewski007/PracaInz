namespace Interpreter
{
    public class Token
    {
        public readonly TokenType type;
        public readonly string value; // lexeme
        public readonly Object literalValue; // literal
        public readonly int line;

        public Token(TokenType type, string value, Object literalValue, int line)
        {
            this.type = type;
            this.value = value;
            this.literalValue = literalValue;
            this.line = line;
        }

        public override string ToString()
        {
            return type + " " + value + " " + literalValue + " linia:" + line;
        }


    }

    public enum TokenType
    {
        // Single character tokens
        LEFT_PARENTHESIS, // (
        RIGHT_PARENTHESIS, // )
        LEFT_BRACE, // {
        RIGHT_BRACE, // }
        COMMA, // ,
        DOT, // .
        MINUS, // -
        PLUS, // +
        SEMICOLON, // ;
        SLASH, // /
        STAR, // *

        // One or two character tokens
        EXCLAMATION, // !
        EXCLAMATION_EQUAL, // !=
        EQUAL, // =
        EQUAL_EQUAL, // ==
        GREATER, // >
        GREATER_EQUAL, // >=
        LESS, // <
        LESS_EQUAL, // <=

        // Identifiers
        STRING, // string
        NUMBER, // int, float, double
        BOOLEAN, // bool

        IDENTIFIER,

        // Keywords
        NULL, // null
        VOID, // void
        IF, // if
        ELSE, // else
        TRUE, // true
        FALSE, // false
        FOR, // for
        WHILE, // while
        RETURN,
        PRINT,
        VAR,
        OR,
        AND,
        FUN,

        // Special type for end of file
        EOF
    }
}