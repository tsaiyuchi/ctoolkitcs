using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;

namespace CToolkit.v1_0.Logging
{
    [Serializable]
    public class CtkLoggerMapper : Dictionary<String, CtkLogger>
    {


        ~CtkLoggerMapper()
        {
            CtkEventUtil.RemoveEventHandlersFromOwningByFilter(this, (dlgt) => true);
        }

        public CtkLogger Get(String id = "")
        {
            lock (this)
            {
                if (!this.ContainsKey(id))
                {
                    var logger = new CtkLogger();
                    this.Add(id, logger);

                    this.OnCreated(new CtkLoggerMapperEventArgs() { LoggerId = id, Logger = logger });
                }
            }
            //不能 override/new this[] 會造成無窮迴圈
            return this[id];
        }





        #region Event
        public event EventHandler<CtkLoggerMapperEventArgs> evtCreated;
        void OnCreated(CtkLoggerMapperEventArgs ea)
        {
            if (this.evtCreated == null)
                return;
            this.evtCreated(this, ea);
        }
        #endregion



        #region Static

        static CtkLoggerMapper m_Singleton;
        public static CtkLoggerMapper Singleton { get { if (m_Singleton == null) { m_Singleton = new CtkLoggerMapper(); } return m_Singleton; } }

        #endregion

    }
}
