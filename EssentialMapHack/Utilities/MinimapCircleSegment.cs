using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace EssentialMapHack.Utilities
{
    class MinimapCircleSegment
    {
        public readonly Vector2 pos;
        public readonly bool ok;
        public MinimapCircleSegment(Vector2 pos, bool ok)
        {
            this.pos = pos;
            this.ok = ok;
        }

    }
}
