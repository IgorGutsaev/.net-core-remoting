using RemotableInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RemotableClient
{
    public class RemoteDecorator<T> : DispatchProxy
    {
        private T _decorated;
        /// <summary>
        /// Proxy object through which the interaction takes place
        /// </summary>
        private IRemotingClient _clientProxy;
        private string _serviceUid = "";

        private void SetProxy(IRemotingClient clientProxy)
        {
            _clientProxy = clientProxy;
            
            // build remote interface
            _serviceUid = _clientProxy.BuildRemoteService(_decorated.GetType().GetInterfaces().FirstOrDefault());// build target interface
            _clientProxy.OnEvent += _proxy_OnEvent;
        }

        private void _proxy_OnEvent(object sender, ServiceEvent e)
        {
            var eventInfoes = this._decorated.GetType().GetEvents();

            foreach (var eventInfo in eventInfoes)
            {
                Type typeEventParam = eventInfo.EventHandlerType;

                if (e.Data != null && typeEventParam.GenericTypeArguments.FirstOrDefault() == e.Data.GetType())
                {
                    var eventDelegate = (MulticastDelegate)this._decorated.GetType().GetField(eventInfo.Name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this._decorated);
                    if (eventDelegate != null)
                    {
                        foreach (var handler in eventDelegate.GetInvocationList())
                        {
                            handler.Method.Invoke(handler.Target, new object[] { null, e.Data });
                        }
                    }
                }
            }
        }

        //public RemoteDecorator<T> InjectProxy(IClientProxy proxy)
        //{
        //    this._clientProxy = proxy;
        //    return this;
        //}

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            try
            {
                LogBefore(targetMethod, args);

                object result = null;

                if (targetMethod.Name.StartsWith("add_"))
                    result = targetMethod.Invoke(_decorated, args); // Original invoke
                else
                    result = this._clientProxy.InvokeMethod(this._serviceUid, targetMethod.Name, PrepareParameters(targetMethod, args)); // Invocation via proxy object

                LogAfter(targetMethod, args, result);
                return result;
            }
            catch (Exception ex) when (ex is TargetInvocationException)
            {
                LogException(ex.InnerException ?? ex, targetMethod);
                throw ex.InnerException ?? ex;
            }
        }

        public static T Create(T decorated, IRemotingClient clientProxy)
        {
            object proxy = Create<T, RemoteDecorator<T>>();
            ((RemoteDecorator<T>)proxy).SetParameters(decorated);
            ((RemoteDecorator<T>)proxy).SetProxy(clientProxy);

            return (T)proxy;
        }


        private void SetParameters(T decorated)
        {
            if (decorated == null)
            {
                throw new ArgumentNullException(nameof(decorated));
            }
            _decorated = decorated;
        }

        private void LogException(Exception exception, MethodInfo methodInfo = null)
        {
            Console.WriteLine($"Class {_decorated.GetType().FullName}, Method {methodInfo.Name} threw exception:\n{exception}");
        }

        private void LogAfter(MethodInfo methodInfo, object[] args, object result)
        {
            Console.WriteLine($"Class {_decorated.GetType().FullName}, Method {methodInfo.Name} executed, Output: {result}");
        }

        private void LogBefore(MethodInfo methodInfo, object[] args)
        {
            Console.WriteLine($"Class {_decorated.GetType().FullName}, Method {methodInfo.Name} is executing");
        }

        private MethodParameter[] PrepareParameters(MethodInfo mInfo, object[] data) // dynamic data
        {
            List<MethodParameter> result = new List<MethodParameter>();

            int index = 0;
            foreach (var p in mInfo.GetParameters())
            {
                var t = data[index];
                index++;

                MethodParameter param = new MethodParameter(p.Name, p.ParameterType, t);
                result.Add(param);
            }

            return result.ToArray();
        }
    }
}
