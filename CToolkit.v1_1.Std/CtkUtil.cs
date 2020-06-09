using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CToolkit.v1_1
{
    public class CtkUtil
    {





        public static string GetMemberName<T, TValue>(Expression<Func<T, TValue>> memberAccess)
        {
            var body = memberAccess.Body;
            var member = body as MemberExpression;
            if (member != null) return member.Member.Name;

            var unary = body as UnaryExpression;
            if (unary != null)
            {
                if (unary.Method != null) return unary.Method.Name;
            }
            throw new ArgumentException();

        }

        public static string GetMethodName<T>(Expression<Func<T, Delegate>> expression)
        {
            var unaryExpression = (UnaryExpression)expression.Body;
            var methodCallExpression = (MethodCallExpression)unaryExpression.Operand;

            var IsNET45 = Type.GetType("System.Reflection.ReflectionContext", false) != null;
            if (IsNET45)
            {
                var methodCallObject = (ConstantExpression)methodCallExpression.Object;
                var methodInfo = (MethodInfo)methodCallObject.Value;
                return methodInfo.Name;
            }
            else
            {
                var methodInfoExpression = (ConstantExpression)methodCallExpression.Arguments.Last();
                var methodInfo = (MemberInfo)methodInfoExpression.Value;
                return methodInfo.Name;
            }
        }


        public static T ParseEnum<T>(String val) { return (T)Enum.Parse(typeof(T), val); }


        #region Serialize

        public static T XmlDeserialize<T>(String xml) where T : class, new()
        {
            var seri = new XmlSerializer(typeof(T));
            using (var xr = XmlReader.Create(new StringReader(xml)))
                return seri.Deserialize(xr) as T;
        }

        public static string XmlSerialize(object obj)
        {
            var seri = new XmlSerializer(obj.GetType());
            using (var sw = new StringWriter())
            using (var xw = XmlWriter.Create(sw))
            {
                seri.Serialize(xw, obj);
                return sw.ToString();
            }
        }



        #endregion


        public static int RandomInt()
        {
            var rnd = new Random((int)DateTime.Now.Ticks);
            var cnt = rnd.Next(32);
            for (var idx = 0; idx < cnt; idx++) rnd.Next();

            return rnd.Next();
        }
        public static int RandomInt(int max)
        {
            var rnd = new Random((int)DateTime.Now.Ticks);
            var cnt = rnd.Next(32);
            for (var idx = 0; idx < cnt; idx++) rnd.Next();

            return rnd.Next(max);
        }
        public static int RandomInt(int min, int max)
        {
            var rnd = new Random((int)DateTime.Now.Ticks);
            var cnt = rnd.Next(32);
            for (var idx = 0; idx < cnt; idx++) rnd.Next();

            return rnd.Next(min, max);
        }


    }




}
