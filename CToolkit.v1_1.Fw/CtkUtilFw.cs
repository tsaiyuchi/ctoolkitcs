using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace CToolkit.v1_1
{
    public class CtkUtilFw
    {

        public static bool EnableUnsafeHeaderParsing()
        {
            //Get the assembly that contains the internal class
            Assembly aNetAssembly = Assembly.GetAssembly(typeof(System.Net.Configuration.SettingsSection));
            if (aNetAssembly != null)
            {
                //Use the assembly in order to get the internal type for the internal class
                System.Type aSettingsType = aNetAssembly.GetType("System.Net.Configuration.SettingsSectionInternal");
                if (aSettingsType != null)
                {
                    //Use the internal static property to get an instance of the internal settings class.
                    //If the static instance isn't created allready the property will create it for us.
                    object anInstance = aSettingsType.InvokeMember("Section",
                      BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.NonPublic, null, null, new object[] { });

                    if (anInstance != null)
                    {
                        //Locate the private bool field that tells the framework is unsafe header parsing should be allowed or not
                        FieldInfo aUseUnsafeHeaderParsing = aSettingsType.GetField("useUnsafeHeaderParsing", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (aUseUnsafeHeaderParsing != null)
                        {
                            aUseUnsafeHeaderParsing.SetValue(anInstance, true);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetCurrentMethod()
        {
            var st = new StackTrace();
            var sf = st.GetFrame(1);

            return sf.GetMethod().Name;
        }

        public static string GetExecutingVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            return assembly.GetName().Version.ToString();
        }
        public static string GetEntryVersion()
        {
            var assembly = Assembly.GetEntryAssembly();
            return assembly.GetName().Version.ToString();
        }


        #region Process
        public static long MemorySize(string processName)
        {
            var procs = Process.GetProcessesByName(processName);
            var sum = 0L;
            foreach (var p in procs)
                sum += p.WorkingSet64;
            return sum;
        }

        public static void TaskkillByName(string name, bool isWaitForExit = true)
        {
            var arguments = string.Format("/IM \"{0}\" /F", name);
            var myproc = Process.Start("taskkill", arguments);
            if (isWaitForExit) myproc.WaitForExit();
        }

        #endregion 



        #region Serialization

        public static T LoadXml<T>(String fn)
        {
            var seri = new System.Xml.Serialization.XmlSerializer(typeof(T));
            var fi = new FileInfo(fn);
            if (!fi.Exists) return default(T);


            using (var stm = fi.OpenRead())
            {
                return (T)seri.Deserialize(stm);
            }
        }

        public static T LoadXmlOrNew<T>(String fn) where T : class, new()
        {
            var seri = new System.Xml.Serialization.XmlSerializer(typeof(T));
            var fi = new FileInfo(fn);
            if (!fi.Exists)
            {
                var config = new T();
                return config;
            }


            using (var stm = fi.OpenRead())
            {
                return seri.Deserialize(stm) as T;
            }
        }

        public static void SaveToXmlFile(object obj, String fn)
        {
            var seri = new System.Xml.Serialization.XmlSerializer(obj.GetType());
            var fi = new FileInfo(fn);

            if (!fi.Directory.Exists) fi.Directory.Create();

            using (var stm = fi.Open(FileMode.Create))
            {
                seri.Serialize(stm, obj);
            }
        }

        public static void SaveToXmlFile(System.Type type, object obj, String fn)
        {
            var seri = new System.Xml.Serialization.XmlSerializer(type);
            var fi = new FileInfo(fn);

            if (!fi.Directory.Exists) fi.Directory.Create();

            using (var stm = fi.Open(FileMode.Create))
            {
                seri.Serialize(stm, obj);
            }
        }

        public static void SaveToXmlFileT<T>(T obj, String fn)
        {
            var seri = new System.Xml.Serialization.XmlSerializer(typeof(T));
            var fi = new FileInfo(fn);

            if (!fi.Directory.Exists) fi.Directory.Create();

            using (var stm = fi.Open(FileMode.Create))
            {
                seri.Serialize(stm, obj);
            }
        }

        public static object TryCatch(Action theMethod, params object[] parameters)
        {
            try
            {
                return theMethod.DynamicInvoke(parameters);
            }
            catch (Exception ex)
            {
                CtkLog.Write(ex);
                return ex;
            }
        }


        public static byte[] SerializeBinary(object obj)
        {
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                ms.Flush();
                return ms.ToArray();
            }
        }

        public static T DeserializeBinary<T>(byte[] dataArray)
        {
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream(dataArray))
            {
                var obj = bf.Deserialize(ms);
                return (T)obj;
            }
        }

        #endregion

        #region Dispose

        public static void DisposeObjTry(IDisposable obj, Action<Exception> exceptionHandler = null)
        {
            try
            {
                if (obj == null) return;
                obj.Dispose();
            }
            catch (Exception ex)
            {
                if (exceptionHandler == null) exceptionHandler(ex);
                else CtkLog.Write(ex);
            }

        }
        public static void DisposeObjTry(IEnumerable<IDisposable> objs, Action<Exception> exceptionHandler = null)
        {
            foreach (var obj in objs) DisposeObjTry(obj, exceptionHandler);
        }

        #endregion

        #region Foreach

        public static void ForeachTry<T>(IEnumerable<T> list, Action<T> act, Action<Exception> exceptionHandler = null)
        {
            foreach (var obj in list)
            {
                try { act(obj); }
                catch (Exception ex)
                {
                    if (exceptionHandler == null) exceptionHandler(ex);
                    else CtkLog.Write(ex);
                }
            }
        }

        #endregion



    }
}
