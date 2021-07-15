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
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace CToolkit.v1_1
{
    public class CtkUtil
    {



        public static bool MonitorTryEnter(object obj, int millisecond, Action act)
        {
            try
            {
                if (!Monitor.TryEnter(obj, millisecond)) return false;
                act();
                return true;
            }
            finally { Monitor.Exit(obj); }
        }




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





        #region Enum


        public static Enum EnumParse(String val, Type type) { return (Enum)Enum.Parse(type, val, true); }
        public static T EnumParse<T>(String val) { return (T)Enum.Parse(typeof(T), val, true); }
        public static List<T> EnumList<T>()
        {
            var ary = Enum.GetValues(typeof(T));
            var list = new List<T>();
            foreach (var e in ary) list.Add((T)e);
            return list;
        }

        #endregion



        #region Member Name

        public static string GetMemberName<TType, TValue>(Expression<Func<TType, TValue>> memberAccess)
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



        public static string GetMethodName<TType, TDelegate>(Expression<Func<TType, TDelegate>> expression)
        {
            LambdaExpression lambda = expression;
            return GetMethodName(lambda);
        }
        public static string GetMethodName<T>(Expression<Func<T, Delegate>> expression)
        {
            LambdaExpression lambda = expression;
            return GetMethodName(lambda);
        }
        public static string GetMethodName(LambdaExpression expression)
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

        public static string GetMethodNameAct<T>(Expression<Func<T, Action>> expression)
        {
            LambdaExpression lambda = expression;
            return GetMethodName(lambda);
        }
        #endregion




        #region Type Guid

        public static Guid? TypeGuid(System.Type type)
        {
            var attrs = type.GetTypeInfo().GetCustomAttributes(typeof(GuidAttribute), false);
            var attr = attrs.FirstOrDefault() as GuidAttribute;
            if (attr == null) return null;
            return Guid.Parse(attr.Value);
        }
        public static Guid? TypeGuid<T>()
        {
            var type = typeof(T);
            return TypeGuid(type);
        }

        public static Guid? TypeGuiInst(object inst)
        {
            var type = inst.GetType();
            return TypeGuid(type);
        }

        #endregion



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


        #region Dispose
        public static void DisposeObj(IDisposable obj)
        {
            if (obj == null) return;
            obj.Dispose();
        }
        public static void DisposeObj(IEnumerable<IDisposable> objs)
        {
            foreach (var obj in objs) DisposeObj(obj);
        }
        public static void DisposeObjN(ref IDisposable obj)
        {
            if (obj == null) return;
            obj.Dispose();
            obj = null;
        }
        #endregion

        #region Foreach

        public static void Foreach<T>(IEnumerable<T> list, Action<T> act)
        {
            foreach (var obj in list) act(obj);
        }

        #endregion







    }




}
