using Interpreter.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
    class Parser
    {
        readonly List<Token> tokens = new List<Token>();
        int currentToken = 0;

        public Parser(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        public List<Statement> Parse()
        {

            List<Statement> statements = new List<Statement>();
            while (!Eof())
            {
                statements.Add(Declaration());
            }

            return statements;
        }

        #region Helper Methods
        /// <summary>
        /// Returns current <see cref="Token"/> and increments pointer to point to the next <see cref="Token"/>.
        /// </summary>
        /// <returns>
        /// Previous <see cref="Token"/>.
        /// </returns>
        private Token Next()
        {
            return tokens[currentToken++];
        }
        /// <summary>
        /// Returns previous <see cref="Token"/>.
        /// </summary>
        /// <returns>
        /// Previous <see cref="Token"/> or <see langword="null"/> if current <see cref="Token"/> is the first one.
        /// </returns>
        private Token Previous()
        {
            if (currentToken - 1 >= 0)
                return tokens[currentToken - 1];
            else
                return null;

        }
        /// <summary>
        /// Matches current <see cref="Token"/> type. Doesn't consume <see cref="Token"/>.
        /// </summary>
        /// <returns>
        /// <see langword="True"/> if matches.
        /// </returns>
        private bool MatchToken(TokenType type)
        {
            if (CheckType(type))
            {
                Next();
                return true;
            }
            return false;
        }
        /// <summary>
        /// Matches current <see cref="Token"/> type. Sets pointer to the next <see cref="Token"/> if matches.
        /// </summary>
        /// <returns>
        /// Current <see cref="Token"/> if matches, else throws a exception.
        /// </returns>
        /// <exception cref="ParseError">
        /// Thrown when match is false.
        /// </exception>
        private Token MatchToken(TokenType type, string errorMessage)
        {
            if (CheckType(type))
                return Next();
            throw Error(Peek(), errorMessage);
        }
        /// <summary>
        /// Matches current <see cref="Token"/> type. Doesn't consume <see cref="Token"/>.
        /// </summary>
        /// <returns>
        /// <see langword="True"/> if at least one matches.
        /// </returns>
        private bool MatchTokens(List<TokenType> types)
        {

            foreach (TokenType type in types)
            {
                if (CheckType(type))
                {
                    Next();
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Matches current <see cref="Token"/> type. Sets pointer to the next <see cref="Token"/> if matches.
        /// </summary>
        /// <returns>
        /// Current <see cref="Token"/> if at least one matches, else throws a exception.
        /// </returns>
        /// <exception cref="ParseError">
        /// Thrown when no match is true.
        /// </exception>
        private Token MatchTokens(List<TokenType> types, string errorMessage)
        {

            foreach (TokenType type in types)
            {
                if (CheckType(type))
                    return Next();
            }

            throw Error(Peek(), errorMessage);
        }
        /// <summary>
        /// Returns current <see cref="Token"/> without moving to the next one.
        /// </summary>
        /// <returns>
        /// Current <see cref="Token"/>.
        /// </returns>
        private Token Peek()
        {
            return tokens[currentToken];
        }
        /// <summary>
        /// Checks if current <see cref="Token"/> has type <see langword="EOF"/>.
        /// </summary>
        /// <returns>
        /// <see langword="True"/> if current <see cref="Token"/> has type <see langword="EOF"/>.
        /// </returns>
        private bool Eof()
        {
            return Peek().type == TokenType.EOF;
        }
        /// <summary>
        /// Checks if current <see cref="Token"/> matches type.
        /// </summary>
        /// <returns>
        /// <see langword="True"/> if matches. Returns <see langword="false"/> if current type is <see langword="EOF"/>. 
        /// </returns>
        private bool CheckType(TokenType type)
        {
            if (Eof())
                return false;
            else
                return Peek().type == type;
        }
        /// <summary>
        /// Throws <see cref="ParseError"/> and displays error details.
        /// </summary>
        /// <exception cref="ParseError">
        /// </exception>
        private ParseError Error(Token token, string errorMessage)
        {
            ErrorThrower.SyntaxError(token, errorMessage);
            throw new ParseError();
        }
        /// <summary>
        /// Synchronizes parser. Consumes current <see cref="Token"/> and keeps consuming until first viable <see cref="Token"/> or <see langword="EOF"/>.
        /// </summary>
        private void Synchronize()
        {
            Next();

            while (!Eof())
            {
                if (Previous().type == TokenType.SEMICOLON) return;

                switch (Peek().type)
                {
                    case TokenType.FOR:
                    case TokenType.WHILE:
                    case TokenType.IF:
                    case TokenType.RETURN:
                        return;
                }

                Next();
            }
        }

        #endregion

        #region Statements

        private Statement Declaration()
        {
            try
            {
                if (MatchToken(TokenType.FUN))
                    return Function("function");
                if (MatchTokens(new List<TokenType> { TokenType.VAR }))
                    return VarDeclaration();
                return Statement();
            }
            catch (ParseError)
            {
                Synchronize();
                return null;
            }
        }

        private Statement Statement()
        {
            if (MatchToken(TokenType.IF))
                return IfStatement();
            if (MatchToken(TokenType.FOR))
                return ForStatement();
            if (MatchToken(TokenType.WHILE))
                return WhileStatement();
            if (MatchToken(TokenType.PRINT))
                return PrintStatement();
            else if (MatchToken(TokenType.LEFT_BRACE))
                return new Block(Block());

            return ExpressionStatement();
        }

        private Statement Function(String kind)
        {
            Token name = MatchToken(TokenType.IDENTIFIER, "Expect function name.");
            MatchToken(TokenType.LEFT_PARENTHESIS, "Expect '(' after " + kind + " name.");
            List<Token> parameters = new List<Token>();
            if (!CheckType(TokenType.RIGHT_PARENTHESIS))
            {
                do
                {
                    if (parameters.Count >= 255)
                        ErrorThrower.SyntaxError(Peek(), "Can't have more than 255 parameters.");
                    parameters.Add(MatchToken(TokenType.IDENTIFIER, "Expect parameter name."));

                } while (MatchToken(TokenType.COMMA));
            }

            MatchToken(TokenType.RIGHT_PARENTHESIS, "Expect ')' after parameters.");

            MatchToken(TokenType.LEFT_BRACE, "Expect '{' before " + kind + " body.");
            List<Statement> body = Block();
            return new Function(name, parameters, body);


        }
        private Statement IfStatement()
        {
            MatchToken(TokenType.LEFT_PARENTHESIS, "Expect '(' after 'if'.");
            Expression condition = Expression();
            MatchToken(TokenType.RIGHT_PARENTHESIS, "Expect ')' after if condition.");

            Statement thenBranch = Statement();
            Statement elseBranch = null;
            if (MatchToken(TokenType.ELSE))
            {
                elseBranch = Statement();
            }

            return new If(condition, thenBranch, elseBranch);
        }

        private List<Statement> Block()
        {
            List<Statement> statements = new List<Statement>();

            while (!CheckType(TokenType.RIGHT_BRACE) && !Eof())
            {
                statements.Add(Declaration());
            }

            MatchToken(TokenType.RIGHT_BRACE, "Expect '}' after block.");
            return statements;
        }

        private Statement WhileStatement()
        {
            MatchToken(TokenType.LEFT_PARENTHESIS, "Expect '(' after 'while'.");
            Expression condition = Expression();
            MatchToken(TokenType.RIGHT_PARENTHESIS, "Expect ')' after while condition.");
            Statement body = Statement();

            return new While(condition, body);
        }

        private Statement ForStatement()
        {
            MatchToken(TokenType.LEFT_PARENTHESIS, "Expect '(' after 'for'.");
            Statement initializer;
            if (MatchToken(TokenType.SEMICOLON))
                initializer = null;
            else if (MatchToken(TokenType.VAR))
                initializer = VarDeclaration();
            else
                initializer = ExpressionStatement();

            Expression condition = null;
            if (!CheckType(TokenType.SEMICOLON))
                condition = Expression();
            MatchToken(TokenType.SEMICOLON, "Expect ';' after loop condition.");

            Expression increment = null;
            if (!CheckType(TokenType.RIGHT_PARENTHESIS))
                increment = Expression();
            MatchToken(TokenType.RIGHT_PARENTHESIS, "Expect ')' after loop clauses.");

            Statement body = Statement();

            if (increment != null)
                body = new Block(new List<Statement> { body, new ExpressionStatement(increment) });

            if (condition == null)
                condition = new Literal(true);
            body = new While(condition, body);

            if (initializer != null)
                body = new Block(new List<Statement> { initializer, body });

            return body;
        }

        private Statement PrintStatement()
        {
            Expression expression = Expression();
            MatchToken(TokenType.SEMICOLON, "Expect ';' after value.");
            return new Print(expression);
        }

        private Statement VarDeclaration()
        {
            Token name = MatchToken(TokenType.IDENTIFIER, "Expect variable name.");
            Expression initializer = null;

            if (MatchToken(TokenType.EQUAL))
                initializer = Expression();
            MatchToken(TokenType.SEMICOLON, "Expect ';' after value.");

            return new Var(name, initializer);
        }

        private Statement ExpressionStatement()
        {
            Expression expression = Expression();
            MatchToken(TokenType.SEMICOLON, "Expect ';' after value.");
            return new ExpressionStatement(expression);
        }

        #endregion

        #region Expressions
        private Expression Expression()
        {
            return Assignment();
        }

        private Expression Assignment()
        {
            Expression expression = Or();

            if (MatchToken(TokenType.EQUAL))
            {
                Token equals = Previous();
                Expression value = Assignment();

                if (expression is Variable var)
                    return new Assign(var.Name, value);

                ErrorThrower.SyntaxError(equals, "Invalid assignment target.");
            }

            return expression;
        }

        private Expression Or()
        {
            Expression expression = And();

            while (MatchToken(TokenType.OR))
            {
                Token _operator = Previous();
                Expression right = And();
                expression = new Logical(expression, _operator, right);
            }

            return expression;
        }

        private Expression And()
        {
            Expression expression = Equality();

            while (MatchToken(TokenType.AND))
            {
                Token _operator = Previous();
                Expression right = Equality();
                expression = new Logical(expression, _operator, right);
            }

            return expression;
        }

        private Expression Equality()
        {
            Expression expression = Comparison();

            while (MatchTokens(new List<TokenType> { TokenType.EXCLAMATION_EQUAL, TokenType.EQUAL_EQUAL }))
            {
                Token _operator = Previous();
                Expression rightExpression = Comparison();
                expression = new Binary(expression, _operator, rightExpression);
            }

            return expression;
        }

        private Expression Comparison()
        {
            Expression expression = Term();

            while (MatchTokens(new List<TokenType> { TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL }))
            {
                Token _operator = Previous();
                Expression rightExpression = Term();
                expression = new Binary(expression, _operator, rightExpression);
            }

            return expression;
        }

        private Expression Term()
        {
            Expression expression = Factor();

            while (MatchTokens(new List<TokenType> { TokenType.PLUS, TokenType.MINUS }))
            {
                Token _operator = Previous();
                Expression rightExpression = Factor();
                expression = new Binary(expression, _operator, rightExpression);
            }

            return expression;
        }

        private Expression Factor()
        {
            Expression expression = Unary();

            while (MatchTokens(new List<TokenType> { TokenType.STAR, TokenType.SLASH }))
            {
                Token _operator = Previous();
                Expression rightExpression = Unary();
                expression = new Binary(expression, _operator, rightExpression);
            }

            return expression;
        }

        private Expression Unary()
        {
            if (MatchTokens(new List<TokenType> { TokenType.EXCLAMATION, TokenType.MINUS }))
            {
                Token _operator = Previous();
                Expression rightExpression = Unary();
                return new Unary(_operator, rightExpression);
            }

            return Call();
        }

        private Expression Call()
        {
            Expression expression = Primary();

            while (true)
            {
                if (MatchToken(TokenType.LEFT_PARENTHESIS))
                    expression = FinishCall(expression);
                else
                    break;
            }
            return expression;
        }

        private Expression FinishCall(Expression callee)
        {
            List<Expression> arguments = new List<Expression>();
            if (!CheckType(TokenType.RIGHT_PARENTHESIS))
                do
                {
                    if (arguments.Count >= 255)
                        ErrorThrower.SyntaxError(Peek(), "Can't have more than 255 arguments.");
                    arguments.Add(Expression());
                } while (MatchToken(TokenType.COMMA));

            Token parenthesis = MatchToken(TokenType.RIGHT_PARENTHESIS, "Expect ')' after arguments.");

            return new Call(callee, parenthesis, arguments);
        }

        private Expression Primary()
        {
            if (MatchToken(TokenType.FALSE))
                return new Literal(false);
            if (MatchToken(TokenType.TRUE))
                return new Literal(true);
            if (MatchToken(TokenType.NULL))
                return new Literal(null);

            if (MatchTokens(new List<TokenType> { TokenType.STRING, TokenType.NUMBER }))
                return new Literal(Previous().literalValue);

            if (MatchToken(TokenType.IDENTIFIER))
                return new Variable(Previous());

            if (MatchToken(TokenType.LEFT_PARENTHESIS))
            {
                Expression expression = Expression();
                MatchToken(TokenType.RIGHT_PARENTHESIS, "Expect ')' after expression.");
                return new Grouping(expression);
            }
            throw Error(Peek(), "Expected expression.");
        }

        #endregion



    }
}
