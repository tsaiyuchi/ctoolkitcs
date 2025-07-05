using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace CToolkitCs.v1_2Core
{

    public class CtkEventUtil
    {

        public static bool IsSubscriberIncludedEvent(EventInfo eventInfo, Delegate targetSubscriber
            , BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
        { return IsSubscriberIncludedEvent(null, eventInfo, targetSubscriber, flags); }
        public static bool IsSubscriberIncludedEvent(Object eventObject, EventInfo eventInfo, Delegate targetSubscriber
            , BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
        {
            if (eventInfo == null || targetSubscriber == null) return false;


            var field = eventInfo.DeclaringType.GetField(eventInfo.Name, flags);
            var eventDelegate = field.GetValue(eventObject) as Delegate;
            if (eventDelegate == null) return false;//沒人訂閱也會回傳null

            foreach (var subscriber in eventDelegate.GetInvocationList().ToList())
            {//每個訂閱者
                if (subscriber == targetSubscriber) return true;
            }
            return false;
        }


        public static bool IsSubscriberIncludedObject(Object eventObject, Delegate targetSubscriber
            , BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
        {
            if (eventObject == null || targetSubscriber == null) return false;
            foreach (DelegateInfo dlgtInfo in GetDelegates(eventObject, null, flags, true))
            {//每個delegate/event
                foreach (Delegate subscriber in dlgtInfo.GetInvocationList())
                {//每個訂閱者
                    if (subscriber == targetSubscriber) return true;
                }
            }
            return false;
        }
        public static bool IsSubscriberIncludedType(Type eventType, Delegate targetSubscriber
            , BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
        {
            if (eventType == null || targetSubscriber == null) return false;

            foreach (DelegateInfo eventOfOwning in GetDelegates(null, eventType, flags, true))
            {//每個delegate/event
                foreach (Delegate subscriber in eventOfOwning.GetInvocationList())
                {//每個訂閱者
                    if (subscriber == targetSubscriber) return true;
                }
            }
            return false;
        }






        /// <summary> 移除物件裡所有delegate/event 屬於target的訂閱者 </summary>
        public static void RemoveSubscriberOfObjectBelongTarget(Object eventObject, Object target
            , BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
        {
            if (eventObject == null || target == null) return;

            foreach (DelegateInfo dlgtInfo in GetDelegates(eventObject, null, flags, true))
            {//每個delegate/event
                foreach (Delegate subscriber in dlgtInfo.GetInvocationList())
                {//每個訂閱者
                    if (subscriber.Target == target)
                    {
                        EventInfo theEvent = dlgtInfo.GetEventInfo(flags);
                        RemoveSubscriberEvenIfItsPrivate(theEvent, eventObject, subscriber, flags);
                    }
                }
            }
        }

        /// <summary> 移除物件裡所有delegate/event 經過filter的訂閱者 </summary>
        public static void RemoveSubscriberOfObjectByFilter(Object eventObject, Func<Delegate, bool> filterFunc = null,
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
        {
            if (eventObject == null) return;
            if (filterFunc == null) filterFunc = dlgt => true;

            foreach (DelegateInfo eventFromOwningObject in GetDelegates(eventObject, null, flags, true))
            {
                foreach (Delegate subscriber in eventFromOwningObject.GetInvocationList())
                {
                    if (filterFunc(subscriber))
                    {
                        EventInfo theEvent = eventFromOwningObject.GetEventInfo(flags);
                        RemoveSubscriberEvenIfItsPrivate(theEvent, eventObject, subscriber, flags);
                    }
                }
            }
        }

        /// <summary> 移除型別裡所有delegate/event 屬於target訂閱者 </summary>
        public static void RemoveSubscriberOfTypeBelongTarget(Type eventType, Object target
            , BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
        {
            foreach (DelegateInfo eventOfOwning in GetDelegates(null, eventType, flags, true))
            {//每個delegate/event
                foreach (Delegate subscriber in eventOfOwning.GetInvocationList())
                {//每個訂閱者
                    if (subscriber.Target == target)
                    {
                        EventInfo theEvent = eventOfOwning.GetEventInfo(flags);
                        RemoveSubscriberEvenIfItsPrivate(theEvent, target, subscriber, flags);
                    }
                }
            }
        }




        /// <summary>
        /// Getting delegates/events what from a type.
        /// 取得型別中 宣告為 delegate/event 的成員
        /// </summary>
        private static DelegateInfo[] GetDelegates(object ownObject, Type ownType, BindingFlags flags, bool isIncldueBaseType)
        {
            var delegates = new List<DelegateInfo>();
            var potentialEvetns = new List<FieldInfo>();

            if (ownObject != null) ownType = ownObject.GetType();
            if (ownType == null) throw new CtkException("Cannot get type");

            var type = ownType;
            do
            {
                potentialEvetns.AddRange(type.GetFields(flags));
                type = type.BaseType;
            } while (type != null && isIncldueBaseType);


            foreach (FieldInfo privateFieldInfo in potentialEvetns)
            {
                Delegate eventFromOwningObject = privateFieldInfo.GetValue(ownObject) as Delegate;
                //可以成功轉為Delegate的, 記錄下來
                if (eventFromOwningObject != null)
                {
                    delegates.Add(new DelegateInfo(eventFromOwningObject, privateFieldInfo, ownObject, ownType));
                }
            }

            return delegates.ToArray();
        }



        private static void RemoveSubscriberEvenIfItsPrivate(
          EventInfo eventInfo, object ownObject, Delegate subscriber, BindingFlags flags)
        {
            // You can use eventInfo.RemoveEventHandler(owningObject, subscriber) 
            // unless it's a private delegate if (eventInfo == null) return;
            MethodInfo privateRemoveMethod = eventInfo.GetRemoveMethod(true);
            //移除指定 訂閱者
            privateRemoveMethod.Invoke(ownObject, flags, null, new object[] { subscriber }, CultureInfo.CurrentCulture);
        }




        private class DelegateInfo
        {
            public readonly Delegate aDelegate;
            public readonly FieldInfo fieldInfo;
            public readonly object ownObject;
            public readonly Type ownType;

            public DelegateInfo(Delegate delegateInformation, FieldInfo fieldInfo, object ownObject, Type ownType)
            {
                this.aDelegate = delegateInformation;
                this.fieldInfo = fieldInfo;
                this.ownObject = ownObject;
                this.ownType = ownType;
            }

            public EventInfo GetEventInfo(BindingFlags flags)
            {
                if (this.ownType == null)
                    return ownObject.GetType().GetEvent(fieldInfo.Name, flags);

                return this.ownType.GetEvent(fieldInfo.Name, flags);
            }

            public Delegate[] GetInvocationList()
            {
                return aDelegate.GetInvocationList();
            }
        }
    }
}
