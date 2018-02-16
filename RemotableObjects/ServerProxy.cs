﻿using RemotableInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;

namespace RemotableObjects
{
    public class ServerProxy<TService> : IServerProxy
    {
        public delegate void onEventRaised(string serviceUid, object someEvent, IPEndPoint callbackEndpoint);
        public event onEventRaised OnEventRaised;

        public string Uid { get; private set; } = Guid.NewGuid().ToString();
        public IPEndPoint _callbackEndpoint;
        private TService _service;

        public bool isCreated
        {
            get { return !String.IsNullOrWhiteSpace(this.Uid) && _service != null; }
        }

        public ServerProxy(TService service, IPEndPoint callbackEndpoint)
        {
            _service = service;
            _callbackEndpoint = callbackEndpoint;

            var eventInfoes = _service.GetType().GetEvents();

            foreach (var eventInfo in eventInfoes)
            {
                Type typeEventParam = eventInfo.EventHandlerType;
                MethodInfo miHandler = typeof(ServerProxy<TService>).GetMethod("EventHandler");

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
                if (methodParams.Any(x => !parameters.Any(p => p.Name == x.Name && p.Type == x.ParameterType)) ||
                    parameters.Any(x => !methodParams.Any(p => p.Name == x.Name && p.ParameterType == x.Type)))
                    continue;

                // check mismatch by params order
                bool failOrder = false;
                for (int i = 0; i < methodParams.Length; i++)
                {
                    if (!String.Equals(methodParams[i].Name, parameters[i].Name, StringComparison.InvariantCultureIgnoreCase)
                        || !Type.Equals(methodParams[i].ParameterType, parameters[i].Type))
                    { failOrder = true; break; }
                }

                if (failOrder)
                    continue;

                compatibleMethod = method;
                break;
            }

            if (compatibleMethod == null)
                throw new CommunicationException($"Cannot find compatible method {methodName} in remote interface.");

            return compatibleMethod.Invoke(this._service, parameters.Select(x => x.Value).ToArray());
        }

        public void Dispose()
        {
            
        }
    }
}