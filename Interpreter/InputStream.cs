namespace Interpreter
{
    public class InputStream
    {
        public int Position = 0;
        public int Line = 1;
        public int Column = 0;

        string input;

        public InputStream(string input)
        {
            this.input = input;
        }

        public char Next()
        {
            char ch = input[Position++];
            if (ch.Equals('\n'))
            {
                Line++;
                Column = 0;
            }
            else
                Column++;
            return ch;

        }

        public char Peek()
        {
            return input[Position];
        }

        public char PeekNext()
        {
            if (Position + 1 >= input.Length)
                return '\0';

            return input[(Position + 1)];
        }

        public bool Eof()
        {
            return Position >= input.Length;
        }

    }
}