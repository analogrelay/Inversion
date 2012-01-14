using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Inversion.Core.Facts
{
    public static class ExceptionAssertMixin
    {
        public static T WithMessage<T>(this T self, string message, params object[] args) where T : Exception
        {
            string actual = self.Message;
            if (typeof(ArgumentException).IsAssignableFrom(typeof(T)))
            {
                // Argument Exceptions have a trailing message we don't want to consider
                actual = Regex.Replace(actual, "Parameter name: .*", String.Empty).Trim();
            }

            Assert.Equal(args.Length == 0 ? message : String.Format(message, args), actual);
            return self;
        }

        public static T WithParamName<T>(this T self, string paramName) where T : ArgumentException
        {
            Assert.Equal(paramName, self.ParamName);
            return self;
        }

        public static T ThrownBy<T>(this T self, Expression<Action> expr) where T : Exception
        {
            Expression body = expr.Body;
            if (body.NodeType == ExpressionType.Call)
            {
                Assert.Equal(self.TargetSite, ((MethodCallExpression)body).Method);
            }
            else if (body.NodeType == ExpressionType.New)
            {
                Assert.Equal(self.TargetSite, ((NewExpression)body).Constructor);
            }
            return self;
        }
    }
}
