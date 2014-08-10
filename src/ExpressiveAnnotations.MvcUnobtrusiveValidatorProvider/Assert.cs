using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpressiveAnnotations.MvcUnobtrusiveValidatorProvider
{
    internal static class Assert
    {
       public static void NoNamingCollisionsAtCorrespondingSegments(IEnumerable<string> listA, IEnumerable<string> listB)
        {
            var segmentsA = listA.Select(x => x.Split('.')).ToList();
            var segmentsB = listB.Select(x => x.Split('.')).ToList();

            foreach (var segA in segmentsA)
            {
                foreach (var segB in segmentsB)
                {
                    var boundary = new[] {segA.Count(), segB.Count()}.Min();
                    for (var i = 0; i < boundary; i++)
                    {
                        if (segA[i] == segB[i])
                            throw new InvalidOperationException(string.Format(
                                "Any naming collisions cannot be accepted at client side - {0} part at level {1} is ambiguous.", segA[i], i));
                    }
                }
            }
        }
    }
}
