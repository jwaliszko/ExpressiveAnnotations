using System.Collections.Generic;
using ExpressiveAnnotations.Analysis;

namespace ExpressiveAnnotations.Tests
{
    public class LocationComparer: IEqualityComparer<Location>
    {
        public bool Equals(Location x, Location y)
        {
            return (x.Line == y.Line && x.Column == y.Column);
        }

        public int GetHashCode(Location obj)
        {
            return obj.GetHashCode();
        }
    }
}
