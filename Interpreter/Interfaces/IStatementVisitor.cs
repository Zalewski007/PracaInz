using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interpreter.AST;
namespace Interpreter.Interfaces
{
    public interface IStatementVisitor<T>
    {
        T VisitExpressionStatement(ExpressionStatement expressionStatement);
        T VisitPrint(Print print);
        T VisitVar(Var var);
        T VisitBlock(Block block);
        T VisitIf(If _if);
        T VisitWhile(While @while);
        T VisitFunction(Function function);
    }
}
