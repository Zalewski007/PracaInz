using Interpreter.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter.AST
{
    internal class FunctionObject : Callable
    {
        private readonly Function Declaration;

        public FunctionObject(Function declaration)
        {
            Declaration = declaration;
        }

        public int Arity()
        {
            return Declaration.Parameters.Count;
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            Environment environment = new Environment(interpreter.GlobalEnvironment);
            for (int i = 0; i < Declaration.Parameters.Count; i++)
            {
                environment.Define(Declaration.Parameters[i].value, arguments[i]);
            }

            interpreter.ExecuteBlock(Declaration.Body, environment);
            return null;
        }
    }
}
