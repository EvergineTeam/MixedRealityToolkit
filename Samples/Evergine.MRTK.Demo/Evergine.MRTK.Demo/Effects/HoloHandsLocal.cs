//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Evergine.MRTK.Demo.Effects
{
    using Evergine.Framework.Graphics;


    public class HoloHandsLocal : Evergine.Framework.Graphics.MaterialDecorator
    {

        public HoloHandsLocal(Evergine.Framework.Graphics.Effects.Effect effect) :
                base(new Material(effect))
        {
        }

        public HoloHandsLocal(Evergine.Framework.Graphics.Material material) :
                base(material)
        {
        }

        public Evergine.Mathematics.Matrix4x4 Base_WorldViewProj
        {
            get
            {
                return this.material.CBuffers[0].GetBufferData<Evergine.Mathematics.Matrix4x4>(0);
            }
            set
            {
                this.material.CBuffers[0].SetBufferData(value, 0);
            }
        }

        public Evergine.Mathematics.Matrix4x4 Base_World
        {
            get
            {
                return this.material.CBuffers[0].GetBufferData<Evergine.Mathematics.Matrix4x4>(64);
            }
            set
            {
                this.material.CBuffers[0].SetBufferData(value, 64);
            }
        }

        public float Base_Time
        {
            get
            {
                return this.material.CBuffers[0].GetBufferData<System.Single>(128);
            }
            set
            {
                this.material.CBuffers[0].SetBufferData(value, 128);
            }
        }

        public Evergine.Mathematics.Vector3 Matrices_EdgeColor
        {
            get
            {
                return this.material.CBuffers[1].GetBufferData<Evergine.Mathematics.Vector3>(0);
            }
            set
            {
                this.material.CBuffers[1].SetBufferData(value, 0);
            }
        }

        public float Matrices_EdgeWidth
        {
            get
            {
                return this.material.CBuffers[1].GetBufferData<System.Single>(12);
            }
            set
            {
                this.material.CBuffers[1].SetBufferData(value, 12);
            }
        }

        public Evergine.Mathematics.Vector3 Matrices_FillColor0
        {
            get
            {
                return this.material.CBuffers[1].GetBufferData<Evergine.Mathematics.Vector3>(16);
            }
            set
            {
                this.material.CBuffers[1].SetBufferData(value, 16);
            }
        }

        public float Matrices_EdgeSmooth
        {
            get
            {
                return this.material.CBuffers[1].GetBufferData<System.Single>(28);
            }
            set
            {
                this.material.CBuffers[1].SetBufferData(value, 28);
            }
        }

        public Evergine.Mathematics.Vector3 Matrices_FillColor1
        {
            get
            {
                return this.material.CBuffers[1].GetBufferData<Evergine.Mathematics.Vector3>(32);
            }
            set
            {
                this.material.CBuffers[1].SetBufferData(value, 32);
            }
        }

        public float Matrices_Displacement
        {
            get
            {
                return this.material.CBuffers[1].GetBufferData<System.Single>(44);
            }
            set
            {
                this.material.CBuffers[1].SetBufferData(value, 44);
            }
        }

        public float Matrices_T
        {
            get
            {
                return this.material.CBuffers[1].GetBufferData<System.Single>(48);
            }
            set
            {
                this.material.CBuffers[1].SetBufferData(value, 48);
            }
        }

        public float Matrices_DistorsionH
        {
            get
            {
                return this.material.CBuffers[1].GetBufferData<System.Single>(52);
            }
            set
            {
                this.material.CBuffers[1].SetBufferData(value, 52);
            }
        }

        public Evergine.Mathematics.Matrix4x4 PerCamera_MultiviewViewProj
        {
            get
            {
                return this.material.CBuffers[2].GetBufferData<Evergine.Mathematics.Matrix4x4>(0);
            }
            set
            {
                this.material.CBuffers[2].SetBufferData(value, 0);
            }
        }

        public int PerCamera_EyeCount
        {
            get
            {
                return this.material.CBuffers[2].GetBufferData<System.Int32>(160);
            }
            set
            {
                this.material.CBuffers[2].SetBufferData(value, 160);
            }
        }
    }
}