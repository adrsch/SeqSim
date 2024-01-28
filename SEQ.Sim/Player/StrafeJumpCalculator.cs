using System.Collections;
using System.Collections.Generic;
using Stride.Engine;using SEQ.Script;
using SEQ.Script.Core;

namespace SEQ.Sim
{
    public class BhopData
    {
        public float Angle;
        public float AddSpeed;
        public float AccelSpeed;
        public float LastVelocity;
        public float TargetSpeed;
        public bool DidUpdate;
        public bool DidMove;

        public float Speed;
    }
}