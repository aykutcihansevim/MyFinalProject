using Castle.DynamicProxy;
using Core.CrossCuttingConcerns.Caching;
using Core.Utilities.Interceptors;
using Core.Utilities.IoC;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Core.Aspects.Autofac.Caching
{
    public class CacheAspect : MethodInterception
    {
        private int _duration;
        private ICacheManager _cacheManager;

        public CacheAspect(int duration = 60)
        {
            _duration = duration;
            _cacheManager = ServiceTool.ServiceProvider.GetService<ICacheManager>();
        }

        public override void Intercept(IInvocation invocation)
        {
            //namespace ve method name e göre key oluşturuluyor. Varsa cache'ten alıyorsun yoksa veritabanından alıyorsun fakat cache'e ekliyorsun.
            //namespace + method name
            var methodName = string.Format($"{invocation.Method.ReflectedType.FullName}.{invocation.Method.Name}");
            //Methodun parametrelerini listeye çevir.
            var arguments = invocation.Arguments.ToList();
            //string.Join her parametre arasına "," konuluyor.
            var key = $"{methodName}({string.Join(",", arguments.Select(x => x?.ToString() ?? "<Null>"))})";
            
            if (_cacheManager.IsAdd(key))
            {
                //methodu çalıştırmadan geri dön.
                invocation.ReturnValue = _cacheManager.Get(key);
                return;
            }
            //methodu devam ettir.
            invocation.Proceed();
            _cacheManager.Add(key, invocation.ReturnValue, _duration);
        }
    }
}
