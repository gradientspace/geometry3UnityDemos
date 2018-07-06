using System;
using System.Collections.Generic;

namespace g3
{
    public class InteractiveRemesher : Remesher
    {
        int PerFrameCount = 1;

        public InteractiveRemesher(DMesh3 mesh) : base(mesh)
        {
        }


        public virtual IEnumerable<int> InteractiveRemesh(int nPasses)
        {
            for (int k = 0; k < nPasses; ++k) {
                BasicRemeshPass();
                yield return k;
            }
        }



        public static IEnumerable<int> InteractiveRemesh(Remesher r, int nPasses)
        {
            for (int k = 0; k < nPasses; ++k) {
                r.BasicRemeshPass();
                yield return k;
            }
        }



    }
}
