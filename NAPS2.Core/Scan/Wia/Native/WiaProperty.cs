using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Scan.Wia.Native
{
    public class WiaProperty
    {
        private object value;

        protected internal WiaProperty(WiaItem owner, int id)
        {
            Owner = owner;
            Id = id;
        }

        private WiaItem Owner { get; }

        public int Id { get; set; }

        // TODO: Full R/W impl
        public object Value
        {
            get => value;
            set
            {
                if (value is int valueInt)
                {
                    WiaException.Check(NativeWiaMethods.SetItemProperty(Owner.Handle, Id, valueInt));
                    // TODO: Get the value from the backing in case it changes
                    this.value = value;
                }
                else
                {
                    throw new NotImplementedException("Not implemented property type");
                }
            }
        }

        public SubTypes SubType { get; set; }

        public int SubTypeMax { get; set; }

        public int SubTypeMin { get; set; }

        public int SubTypeStep { get; set; }

        public object[] SubTypeValues { get; set; }

        public enum SubTypes
        {
            None,
            Range,
            List,
            Flag
        }
    }
}