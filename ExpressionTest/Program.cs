using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ExpressionTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var rule = @" { 'and' : [  {'<' : [ { 'var' : 'temp' }, 110 ]}, {'==' : [ { 'var' : 'pie.filling' }, 'apple' ] }] }";
            // var rule = @" { '+' : [  2, 2] }";
            //var rule = @" { '==' : [  {'+' : [1,1]}, 2] }";
            



            var paramTypeDictionary = new Dictionary<string, Type>();
            paramTypeDictionary.Add("temp", typeof(int));
            paramTypeDictionary.Add("pie.filling", typeof(string));

            var expressionTest = new ExpressionTester<bool>();
            expressionTest.CompileFunction(rule, paramTypeDictionary);

            var paramDictionary = new Dictionary<string,object>();
            paramDictionary.Add("temp",120);
            paramDictionary.Add("pie.filling", "apple");

            

            var test = expressionTest.CompiledFunc(paramDictionary);

            //var test = expressionTest.CompiledFunc.DynamicInvoke();

            Console.WriteLine(test);
            Console.WriteLine("Test");
            Console.ReadKey();
        }
        
    }
}
