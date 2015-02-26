using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Linq;
using System.Linq.Expressions;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExpressionMunging
{

    public static class ExpressionHydration
    {
        public static TR Invoke<T1, TR>(this Expression<Func<T1, TR>> expr, T1 p1)
        {
            throw new NotImplementedException("Invoke() used without call to Hydrate().");
        }

        public static Expression<TDelegate> Hydrate<TDelegate>(this Expression<TDelegate> expr)
        {
            return (Expression<TDelegate>)new SearchVisitor().Visit(expr);
        }

        private class SearchVisitor : ExpressionVisitor
        {
            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.DeclaringType == typeof(ExpressionHydration) && node.Method.Name == "Invoke")
                {
                    var subEx = (LambdaExpression)((Func<Expression>)Expression.Lambda(node.Arguments[0]).Compile())();
                    var parameters = subEx.Parameters.ToArray();
                    var arguments = node.Arguments.Skip(1).ToArray();
                    var replacedEx = (LambdaExpression)new ReplaceParametersVisitor(parameters, arguments).Visit(subEx);
                    return replacedEx.Body;
                }

                return base.VisitMethodCall(node);
            }
        }

        private class ReplaceParametersVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression[] parameters;
            private readonly Expression[] args;

            public ReplaceParametersVisitor(ParameterExpression[] parameters, Expression[] args)
            {
                this.parameters = parameters;
                this.args = args;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                var index = Array.IndexOf(parameters, node);
                return index >= 0 ? args[index] : base.VisitParameter(node);
            }
        }
    }



    [TestClass]
    public class ExpressionHydrationTests
    {
        [TestMethod]
        public void CanIncludeSubexpressionInExpression()
        {
            Expression<Func<string, int>> subex = v => v.Length;

            Expression<Func<string, string>> ex = s => "The length of '" + s + "' is " + subex.Invoke(s);

            var hydrated = ex.Hydrate();
            Assert.AreEqual(hydrated.Compile()("Tim"), "The length of 'Tim' is 3");
        }
    }


}
