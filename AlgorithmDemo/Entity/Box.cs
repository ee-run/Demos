using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgorithmDemo.Entity
{
    public class Box
    {
        public int ID;

        private float m_Length = 2;
        public float Length
        {
            get { return m_Length; }
            set { m_Length = value; }
        }

        private float m_Width = 2;
        public float Width
        {
            get { return m_Width; }
            set { m_Width = value; }
        }

        private float m_Height = 2;
        public float High
        {
            get { return m_Height; }
            set { m_Height = value; }
        }

        public float Volume
        {
            get { return High * Width * Length; }
        }

        public int Status = 0; // 1 装满，0 未装满

        public PackPanel BoxPackPanel;
    }
}
