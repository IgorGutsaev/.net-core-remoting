using RemotableInterfaces;
using RemoteCommunication.RemotableProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RemotableObjects
{
    public class ServerProxy<TService> : IServerProxy
    {
        public string Uid { get; private set; } = Guid.NewGuid().ToString();
        private TService _Service;

        public bool isCreated
        {
            get { return !String.IsNullOrWhiteSpace(this.Uid) && _Service != null; }
        }

        public ServerProxy(TService service)
        {
            this._Service = service;

            var eventInfoes = _Service.GetType().GetEvents();

            foreach (var eventInfo in eventInfoes)
            {

                Type typeEventParam = eventInfo.EventHandlerType;
                MethodInfo miHandler = typeof(ServerProxy<TService>).GetMethod("EventHandler");


                Delegate d = Delegate.CreateDelegate(typeEventParam, this, miHandler);

                MethodInfo addHandler = eventInfo.GetAddMethod();
                Object[] addHandlerArgs = { d };
                addHandler.Invoke(_Service, addHandlerArgs);
            }
        }

        public void EventHandler(object sender, object someEvent)
        {
            
        }

        public string GetUid()
        {
            return this.Uid;
        }

        public object InvokeMethod(string methodName, List<MethodParameter> parameters)
        {
            Type serviceType = this._Service.GetType();
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

            return compatibleMethod.Invoke(this._Service, parameters.Select(x => x.Value).ToArray());
        }
    }
}
