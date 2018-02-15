using System;
using System.Collections.Generic;

namespace g3
{
    public class InteractiveReducer : Reducer
    {
        int PerFrameCount = 1;

        public InteractiveReducer(DMesh3 mesh) : base(mesh)
        {
        }


        public virtual IEnumerable<int> ReduceToTriangleCount_Interactive(int nCount)
        {

            ReduceMode = TargetModes.TriangleCount;
            TargetCount = Math.Max(1, nCount);
            MinEdgeLength = double.MaxValue;


            if (mesh.TriangleCount == 0)    // badness if we don't catch this...
                yield break;

            begin_pass();

            begin_setup();
            Precompute();
            InitializeVertexQuadrics();
            InitializeQueue();
            end_setup();

            begin_ops();

            begin_collapse();
            int count = PerFrameCount;
            while (EdgeQueue.Count > 0) {

                // termination criteria
                if (ReduceMode == TargetModes.VertexCount) {
                    if (mesh.VertexCount <= TargetCount)
                        break;
                } else {
                    if (mesh.TriangleCount <= TargetCount)
                        break;
                }

                int eid = EdgeQueue.Dequeue();
                if (!mesh.IsEdge(eid))
                    continue;

                int vKept;
                ProcessResult result = CollapseEdge(eid, EdgeQuadrics[eid].collapse_pt, out vKept);
                if (result == ProcessResult.Ok_Collapsed) {
                    vertQuadrics[vKept] = EdgeQuadrics[eid].q;
                    UpdateNeighbours(vKept);
                }
                if (count-- == 0) {
                    count = PerFrameCount;
                    yield return 0;
                }
            }
            end_collapse();
            end_ops();

            Reproject();

            end_pass();
        }



    }
}
