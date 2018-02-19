using RemotableInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;

namespace RemotableObjects
{
    public class ServerProxy : IServerProxy
    {
        public delegate void onEventRaised(string serviceUid, object someEvent, IPEndPoint callbackEndpoint);
        public event onEventRaised OnEventRaised;

        public string Uid { get; private set; } = Guid.NewGuid().ToString();
        public IPEndPoint _callbackEndpoint;
        private object _service;

        public ServerProxy(object service, IPEndPoint callbackEndpoint)
        {
            _service = service;
            _callbackEndpoint = callbackEndpoint;

            var eventInfoes = _service.GetType().GetEvents();

            foreach (var eventInfo in eventInfoes)
            {
                Type typeEventParam = eventInfo.EventHandlerType;
                MethodInfo miHandler = typeof(ServerProxy).GetMethod("EventHandler");

                Delegate d = Delegate.CreateDelegate(typeEventParam, this, miHandler);
 
                MethodInfo addHandler = eventInfo.GetAddMethod();
                Object[] addHandlerArgs = { d };
                addHandler.Invoke(_service, addHandlerArgs);
            }
        }

        public void EventHandler(object sender, object someEvent)
        {
            if (this.OnEventRaised != null)
                this.OnEventRaised(this.Uid, someEvent, this._callbackEndpoint);
        }

        public string GetUid()
        {
            return this.Uid.Trim();
        }

        public object InvokeMethod(string methodName, List<MethodParameter> parameters)
        {
            Type serviceType = this._service.GetType();
            List<MethodInfo> compatibleMethods = serviceType.GetMethods().Where(m => String.Equals(m.Name, methodName) /*&& m.GetParameters().Length == parameters.Count*/).ToList();

            MethodInfo compatibleMethod = null;

            // Check signature compatibility
            foreach (var method in compatibleMethods)
            {
                var methodParams = method.GetParameters();

                // params mismatch by count
                if (methodParams.Length != parameters.Count)
                    continue;

                // params mismatch by name and type
                if (methodParams.Any(x => !parameters.Any(p => p.Type == x.ParameterType)) ||
                    parameters.Any(x => !methodParams.Any(p => p.ParameterType == x.Type)))
                    continue;

                // check mismatch by params order
                bool failOrder = false;
                for (int i = 0; i < methodParams.Length; i++)
                {
                    if (!Type.Equals(methodParams[i].ParameterType, parameters[i].Type))
                    { failOrder = true; break; }
                }

                if (failOrder)
                    continue;

                compatibleMethod = method;
                break;
            }

            if (compatibleMethod == null)
                throw new CommunicationException($"Cannot find compatible method {methodName} in remote interface.");

            try
            {
                return compatibleMethod.Invoke(this._service, parameters.Select(x => x.Value).ToArray());
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    throw ex.InnerException;
                else throw ex;
            }
        }

        public void Dispose()
        {
            
        }
    }
}