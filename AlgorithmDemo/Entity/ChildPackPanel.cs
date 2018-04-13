using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgorithmDemo.Entity
{
    public class ChildPackPanel : PackPanel
    {
        public new float Volume { get { return Length * Width * High; } }
    }
}
