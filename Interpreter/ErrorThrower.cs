using static System.Net.Mime.MediaTypeNames;

namespace Interpreter
{
    public static class ErrorThrower
    {
        public static bool HadRuntimeError;

        public static void SyntaxError(Token token, string message)
        {
            throw new Exception(message + " (line: " + token.line + ")");
        }

        public static void InputError(string message, int line, int column)
        {
            throw new Exception(message + " (line: " + line + ", column: " + column + ")");
        }

        public static void RuntimeError(RuntimeError error)
        {
           
            //Console.WriteLine(error.Message + "\n (line: " + error.token.line + ")");
            //throw new Exception(error.Message + "\n (line: " + error.token.line + ")");
            HadRuntimeError = true;
        }


    }

    public class RuntimeError : SystemException
    {
        public readonly Token token;

        public RuntimeError(Token token, String message) : base(message)
        {
            this.token = token;
        }
    }

    class ParseError : Exception { }
}