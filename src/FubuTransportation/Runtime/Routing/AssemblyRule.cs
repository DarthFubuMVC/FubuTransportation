using System;
using System.IO;
using System.Reflection;

namespace FubuTransportation.Runtime.Routing
{
    public class AssemblyRule : IRoutingRule
    {
        private readonly Assembly _assembly;

        public AssemblyRule(Assembly assembly)
        {
            _assembly = assembly;
        }

        public bool Matches(Type type)
        {
            return _assembly.Equals(type.Assembly);
        }

        public void Describe(IScenarioWriter writer)
        {
            writer.WriteLine("Publishes messages in Assembly " + _assembly.GetName().Name);
        }

        public static AssemblyRule For<T>()
        {
            return new AssemblyRule(typeof(T).Assembly);
        }

        protected bool Equals(AssemblyRule other)
        {
            return Equals(_assembly, other._assembly);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AssemblyRule) obj);
        }

        public override int GetHashCode()
        {
            return (_assembly != null ? _assembly.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return string.Format("Contained in assembly {0}", _assembly.GetName().Name);
        }
    }
}