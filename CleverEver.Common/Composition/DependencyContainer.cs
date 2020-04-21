using DryIoc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Composition
{
    public class DependencyContainer
    {
        private static DryIoc.Container container;

        public static Container Container
        {
            get
            {
                if (container == null)
                    throw new InvalidOperationException("Unable to resolve to a container because this is not set");
                return container;
            }
            set
            {
                container = value;
            }
        }

        public static T Resolve<T>()
        {
            return (T)((IResolver)container).Resolve(typeof(T));
        }

        public static T Resolve<T>(string name)
        {
            return (T)((IResolver)container).Resolve(typeof(T), name);
        }

        public static IEnumerable<T> ResolveAll<T>()
        {
            return ((IResolver)container).ResolveMany(typeof(T)).OfType<T>();
        }

        public static void Register<T>(IReuse reuse = null)
        {
            container.Register<T>(reuse: reuse);
        }


        public static void Register<TInterface, TImplementation>()
            where TImplementation : TInterface
        {
            container.Register<TInterface, TImplementation>();
        }

        public static void Register<TInterface, TImplementation>(string name)
             where TImplementation : TInterface
        {
            container.Register<TInterface, TImplementation>(serviceKey: name);
        }
    }
}
