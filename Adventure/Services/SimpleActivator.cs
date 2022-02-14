using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Services
{
    public interface ISimpleActivator
    {
        T CreateInstance<T>(string name);
    }

    public class SimpleActivator : ISimpleActivator
    {
        public T CreateInstance<T>(String name)
        {
            var type = Type.GetType(name);
            var instance = (T)Activator.CreateInstance(type);
            return instance;
        }
    }
}
