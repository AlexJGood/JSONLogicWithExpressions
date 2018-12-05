using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ExpressionTest
{
    public class ExpressionTester<TReturnType>
    {
        public Func<Dictionary<string, object>, TReturnType> CompiledFunc { get; set; }

        public void CompileFunction(string functionExpression, Dictionary<string, Type> paramTypeDictionary)
        {
            var ruleNodes = JObject.Parse(functionExpression);

            var paramExpressions = new List<ParameterExpression>();
            var inputDictionaryExpression = Expression.Parameter(typeof(Dictionary<string,object>));
            paramExpressions.Add(inputDictionaryExpression);
            
            var result =  Expression.Parameter(typeof(TReturnType), "result");

            var block = Expression.Block(new[] { result }, GenerateExpression(inputDictionaryExpression, paramTypeDictionary, ruleNodes.First ));

            var lambdaExpression = Expression.Lambda < Func<Dictionary<string, object>, TReturnType> > (block,  paramExpressions);

            CompiledFunc = lambdaExpression.Compile();

        }

        private Expression GenerateExpression(ParameterExpression inputDictionaryExpression, Dictionary<string, Type> paramTypeDictionary, JToken rule)
        {

            if (rule is JProperty)
            {
                var jProp = (Newtonsoft.Json.Linq.JProperty)rule;
                switch (jProp.Name)
                {
                    case "and":
                        return Expression.And(GenerateExpression(inputDictionaryExpression, paramTypeDictionary, UnboxToken(((JArray)rule.First)[0])), GenerateExpression(inputDictionaryExpression, paramTypeDictionary, UnboxToken(((JArray)rule.First)[1])));
                    case "or":
                        return Expression.Or(GenerateExpression(inputDictionaryExpression, paramTypeDictionary, UnboxToken(((JArray)rule.First)[0])), GenerateExpression(inputDictionaryExpression, paramTypeDictionary, UnboxToken(((JArray)rule.First)[1])));
                    case "+":
                        return Expression.Add(GenerateExpression(inputDictionaryExpression, paramTypeDictionary, UnboxToken(((JArray)rule.First)[0])), GenerateExpression(inputDictionaryExpression, paramTypeDictionary, UnboxToken(((JArray)rule.First)[1])));
                    case "<":
                        return Expression.LessThan(GenerateExpression(inputDictionaryExpression, paramTypeDictionary, UnboxToken(((JArray)rule.First)[0])), GenerateExpression(inputDictionaryExpression, paramTypeDictionary, UnboxToken(((JArray)rule.First)[1])));
                    case "==":
                        return Expression.Equal(GenerateExpression(inputDictionaryExpression, paramTypeDictionary, UnboxToken(((JArray)rule.First)[0])), GenerateExpression(inputDictionaryExpression, paramTypeDictionary, UnboxToken(((JArray)rule.First)[1])));
                    case "var":
                        return Expression.Convert(Expression.Property(inputDictionaryExpression, "Item", Expression.Constant(rule.First.ToString().Replace("{", "").Replace("}", "")) ), paramTypeDictionary[rule.First.ToString().Replace("{", "").Replace("}", "")]);
                }
            }

            if (rule is JValue)
            {
                var val = (JValue)rule;
                switch (val.Type)
                {
                    case JTokenType.Integer:
                        return Expression.Constant(Convert.ToInt32(val.Value), typeof(int));
                    case JTokenType.Float:
                        return Expression.Constant(Convert.ToSingle(val.Value), typeof(float));
                    case JTokenType.String:
                        return Expression.Constant(Convert.ToString(val.Value), typeof(string));
                    case JTokenType.Boolean:
                        return Expression.Constant(Convert.ToBoolean(val.Value), typeof(bool));
                    case JTokenType.Date:
                        return Expression.Constant(Convert.ToDateTime(val.Value), typeof(DateTime));
                    case JTokenType.Guid:
                        return Expression.Constant(Guid.Parse(val.Value.ToString()), typeof(Guid));
                    case JTokenType.TimeSpan:
                        return Expression.Constant(Convert.ToDateTime(val.Value), typeof(DateTime));
                }
                return Expression.Constant(Convert.ToString(val.Value), typeof(string)); ;
            }

            return null;
        }
    
        private JToken UnboxToken(JToken token)
        {
            if (token is JObject)
            {
                return token.First;
            }
            else
            {
                return token;
            }
        }
    }
}
