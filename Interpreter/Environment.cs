using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
    public class Environment
    {
        public Environment EnclosingEnvironment { get; }
        private Dictionary<string, object> Values = new Dictionary<string, object>();

        public Environment()
        {
            EnclosingEnvironment = null;
        }

        public Environment(Environment enclosingEnvironment)
        {
            EnclosingEnvironment = enclosingEnvironment;
        }

        public void Clear()
        {
            Values = new Dictionary<string, object>();
        }

        public void Define(string name, object value)
        {
            Values.Add(name, value);
        }

        public object Get(Token name)
        {
            if (Values.ContainsKey(name.value))
            {
                return Values[name.value];
            }

            if (EnclosingEnvironment != null)
                return EnclosingEnvironment.Get(name);

            throw new RuntimeError(name,
                "Undefined variable '" + name.value + "'.");
        }

        public void Assign(Token name, object value)
        {
            if (Values.ContainsKey(name.value))
            {
                Values[name.value] = value;
            }
            else if (EnclosingEnvironment != null)
                EnclosingEnvironment.Assign(name, value);
            else
                throw new RuntimeError(name, "Undefined variable '" + name.value + "'.");
        }
    }
}
