using Interpreter.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter.AST
{
    #region Expessions
    public abstract class Expression
    {
        public abstract T Accept<T>(IExpressionVisitor<T> visitor);

    }

    public class Assign : Expression
    {
        public Token Name { get; }
        public Expression Value { get; }

        public Assign(Token name, Expression value)
        {
            Name = name;
            Value = value;
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitAssign(this);
        }
    }

    public class Binary : Expression
    {
        public readonly Expression LeftExpression;
        public readonly Token Operator;
        public readonly Expression RightExpression;

        public Binary(Expression leftExpression, Token @operator, Expression rightExpression)
        {
            LeftExpression = leftExpression;
            Operator = @operator;
            RightExpression = rightExpression;
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitBinary(this);
        }
    }

    public class Unary : Expression
    {
        public readonly Token Operator;
        public readonly Expression RightExpression;

        public Unary(Token @operator, Expression rightExpression)
        {
            Operator = @operator;
            RightExpression = rightExpression;
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitUnary(this);
        }
    }

    public class Call : Expression
    {
        public Expression Calee { get; }
        public Token Parenthesis { get; }
        public List<Expression> Arguments { get; }

        public Call(Expression calee, Token parenthesis, List<Expression> arguments)
        {
            Calee = calee;
            Parenthesis = parenthesis;
            Arguments = arguments;
        }
        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitCall(this);
        }
    }
    public class Grouping : Expression
    {
        public readonly Expression Expression;

        public Grouping(Expression expression)
        {
            Expression = expression;
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitGrouping(this);
        }
    }

    public class Literal : Expression
    {
        public readonly object Value;

        public Literal(object value)
        {
            Value = value;
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitLiteral(this);
        }
    }

    public class Variable : Expression
    {
        public Token Name { get; }

        public Variable(Token name)
        {
            Name = name;
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitVariable(this);
        }
    }

    public class Logical : Expression
    {
        public Expression LeftExpression { get; }
        public Token Operator { get; }
        public Expression RightExpression { get; }

        public Logical(Expression leftExpression, Token @operator, Expression rightExpression)
        {
            LeftExpression = leftExpression;
            Operator = @operator;
            RightExpression = rightExpression;
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitLogical(this);
        }
    }

    #endregion

    #region Statements

    public abstract class Statement
    {
        public abstract T Accept<T>(IStatementVisitor<T> visitor);
    }

    public class Block : Statement
    {
        public List<Statement> Statements { get; }
        public Block(List<Statement> statements)
        {
            Statements = statements;
        }

        public override T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitBlock(this);
        }
    }

    public class ExpressionStatement : Statement
    {
        public Expression Expression { get; }

        public ExpressionStatement(Expression expression)
        {
            Expression = expression;
        }

        public override T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitExpressionStatement(this);
        }
    }

    public class Print : Statement
    {

        public Expression Expression { get; }

        public Print(Expression expression)
        {
            Expression = expression;
        }

        public override T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitPrint(this);
        }
    }

    public class Function : Statement
    {
        public Token Name { get; }
        public List<Token> Parameters { get; }
        public List<Statement> Body { get; }

        public Function(Token name, List<Token> parameters, List<Statement> body)
        {
            Name = name;
            Parameters = parameters;
            Body = body;
        }

        public override T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitFunction(this);
        }
    }

    public class If : Statement
    {
        public Expression Condition { get; }
        public Statement ThenBranch { get; }
        public Statement ElseBranch { get; }
        public If(Expression condition, Statement thenBranch, Statement elseBranch)
        {
            Condition = condition;
            ThenBranch = thenBranch;
            ElseBranch = elseBranch;
        }

        public override T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitIf(this);
        }
    }

    public class Var : Statement
    {
        public Token Indentifier { get; }
        public Expression Initializer { get; }

        public Var(Token indentifier, Expression initializer)
        {
            Indentifier = indentifier;
            Initializer = initializer;
        }

        public override T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitVar(this);
        }
    }

    public class While : Statement
    {
        public Expression Condition { get; }
        public Statement Body { get; }
        public While(Expression condition, Statement body)
        {
            Condition = condition;
            Body = body;
        }

        public override T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitWhile(this);
        }
    }

    #endregion
}
