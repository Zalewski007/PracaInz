using Interpreter.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter.Interfaces
{
    public interface IExpressionVisitor<T>
    {
        T VisitBinary(Binary binary);
        T VisitUnary(Unary unary);
        T VisitGrouping(Grouping grouping);
        T VisitLiteral(Literal literal);
        T VisitVariable(Variable variable);
        T VisitAssign(Assign assign);
        T VisitLogical(Logical logical);
        T VisitCall(Call call);
    }
}
