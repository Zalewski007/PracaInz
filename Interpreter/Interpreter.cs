using Interpreter.AST;
using Interpreter.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Interpreter
{
    public class Interpreter : IExpressionVisitor<object>, IStatementVisitor<object>
    {


        public readonly Environment GlobalEnvironment = new Environment();
        private Environment Environment = new Environment();

        public Interpreter()
        {
            Environment = GlobalEnvironment;
            GlobalEnvironment.Define("clock", new ClockFunction());
            GlobalEnvironment.Define("write", new Write());
            GlobalEnvironment.Define("writeLine", new WriteLine());
        }

        public void RestartIntepreter()
        {
            GlobalEnvironment.Clear();
            Environment = GlobalEnvironment;
            GlobalEnvironment.Define("clock", new ClockFunction());
            GlobalEnvironment.Define("write", new Write());
            GlobalEnvironment.Define("writeLine", new WriteLine());
        }

        private class ClockFunction : Callable
        {
            public int Arity()
            {
                return 0;
            }

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }
        }
        private class WriteLine : Callable
        {
            public int Arity()
            {
                return 1;
            }

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                return null;
            }
        }
        private class Write : Callable
        {
            public int Arity()
            {
                return 1;
            }

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                return null;
            }
        }

        public void Interpret(List<Statement> statements)
        {
            try
            {
                foreach (Statement statement in statements)
                    Execute(statement);
            }
            catch (RuntimeError error)
            {
                ErrorThrower.RuntimeError(error);
            }
        }
        private void Execute(Statement statement)
        {
            statement.Accept(this);
        }

        private String Stringify(Object targetObject)
        {
            if (targetObject == null)
                return "null";

            if (targetObject is double)
            {
                String text = targetObject.ToString();
                if (text.EndsWith(".0"))
                {
                    text = text.Substring(0, text.Length - 2);
                }
                return text;
            }

            return targetObject.ToString();
        }

        public void ExecuteBlock(List<Statement> statements, Environment environment)
        {
            Environment previous = this.Environment;
            try
            {
                this.Environment = environment;

                foreach (Statement statement in statements)
                {
                    Execute(statement);
                }
            }
            finally
            {
                this.Environment = previous;
            }
        }

        #region Statements
        public object VisitExpressionStatement(ExpressionStatement expressionStatement)
        {
            Evaluate(expressionStatement.Expression);
            return null;
        }
        public object VisitWhile(While @while)
        {
            while (IsTruth(Evaluate(@while.Condition)))
            {
                Execute(@while.Body);
            }
            return null;
        }
        public object VisitPrint(Print print)
        {
            object value = Evaluate(print.Expression);
            //Form1 f1 = (Form1)Application.OpenForms["Form1"];
            //TextBox tb = (TextBox)f1.Controls["OutputBox"];
            //tb.Text += Stringify(value);
            //Console.WriteLine(Stringify(value));
            return null;
        }
        public object VisitVar(Var var)
        {
            object value = null;
            if (var.Initializer != null)
                value = Evaluate(var.Initializer);
            Environment.Define(var.Indentifier.value, value);
            return null;
        }

        public object VisitBlock(Block block)
        {
            ExecuteBlock(block.Statements, new Environment(Environment));
            return null;
        }
        public object VisitIf(If _if)
        {
            if (IsTruth(Evaluate(_if.Condition)))
            {
                Execute(_if.ThenBranch);
            }
            else if (_if.ElseBranch != null)
            {
                Execute(_if.ElseBranch);
            }
            return null;
        }

        public object VisitFunction(Function function)
        {
            FunctionObject func = new FunctionObject(function);
            Environment.Define(function.Name.value, func);
            return null;
        }

        #endregion

        #region Expression
        public object VisitBinary(Binary binary)
        {
            object left = Evaluate(binary.LeftExpression);
            object right = Evaluate(binary.RightExpression);

            switch (binary.Operator.type)
            {
                case TokenType.GREATER:
                    CheckNumberOperands(binary.Operator, left, right);
                    return (double)left > (double)right;
                case TokenType.GREATER_EQUAL:
                    CheckNumberOperands(binary.Operator, left, right);
                    return (double)left >= (double)right;
                case TokenType.LESS:
                    CheckNumberOperands(binary.Operator, left, right);
                    return (double)left < (double)right;
                case TokenType.LESS_EQUAL:
                    CheckNumberOperands(binary.Operator, left, right);
                    return (double)left <= (double)right;
                case TokenType.EXCLAMATION_EQUAL:
                    return !IsEqual(left, right);
                case TokenType.EQUAL_EQUAL:
                    return IsEqual(left, right);
                case TokenType.MINUS:
                    CheckNumberOperands(binary.Operator, left, right);
                    return (double)left - (double)right;
                case TokenType.SLASH:
                    CheckNumberOperands(binary.Operator, left, right);
                    return (double)left / (double)right;
                case TokenType.STAR:
                    CheckNumberOperands(binary.Operator, left, right);
                    return (double)left * (double)right;
                case TokenType.PLUS:
                    if (left is double && right is double)
                        return (double)left + (double)right;
                    if (left is string && right is string)
                        return (string)left + (string)right;
                    throw new RuntimeError(binary.Operator,
                        "Operands must be two numbers or two strings.");
            }

            return null;
        }

        public object VisitGrouping(Grouping grouping)
        {
            return Evaluate(grouping.Expression);
        }

        public object VisitLiteral(Literal literal)
        {
            return literal.Value;
        }

        public object VisitUnary(Unary unary)
        {
            object right = Evaluate(unary.RightExpression);

            switch (unary.Operator.type)
            {
                case TokenType.MINUS:
                    CheckNumberOperand(unary.Operator, right);
                    return -(double)right;
                case TokenType.EXCLAMATION:
                    return !IsTruth(right);
            }

            return null;
        }

        public object VisitCall(Call call)
        {
            Object callee = Evaluate(call.Calee);
            List<object> arguments = new List<object>();
            foreach (Expression argument in call.Arguments)
            {
                arguments.Add(Evaluate(argument));
            }

            if (!(callee is Callable))
                throw new RuntimeError(call.Parenthesis, "Can only call functions.");

            Callable function = (Callable)callee;

            if (arguments.Count != function.Arity())
                throw new RuntimeError(call.Parenthesis, "Expected " + function.Arity() + " arguments but got " + arguments.Count);

            return function.Call(this, arguments);

        }

        public object VisitVariable(Variable variable)
        {
            return Environment.Get(variable.Name);
        }

        public object VisitAssign(Assign assign)
        {
            object value = Evaluate(assign.Value);
            Environment.Assign(assign.Name, value);
            return value;
        }

        public object VisitLogical(Logical logical)
        {
            object left = Evaluate(logical.LeftExpression);

            if (logical.Operator.type == TokenType.OR)
            {
                if (IsTruth(left)) return left;
            }
            else
            {
                if (!IsTruth(left)) return left;
            }

            return Evaluate(logical.RightExpression);
        }

        private object Evaluate(Expression Expression)
        {
            return Expression.Accept(this);
        }

        private bool IsTruth(object targetObject)
        {
            if (targetObject == null)
                return false;
            if (targetObject is bool)
                return (bool)targetObject;
            return true;
        }

        private bool IsEqual(object a, object b)
        {
            if (a == null && b == null)
                return true;
            if (a == null)
                return false;

            return a.Equals(b);
        }

        private void CheckNumberOperand(Token _operator, object operand)
        {
            if (operand is double)
                return;
            throw new RuntimeError(_operator, "Operand must be a number.");
        }

        private void CheckNumberOperands(Token _operator, object left, object right)
        {
            if (left is double && right is double) return;

            throw new RuntimeError(_operator, "Operands must be numbers.");
        }





        #endregion


    }
}
