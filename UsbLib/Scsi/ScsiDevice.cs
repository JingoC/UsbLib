﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsbLib.Scsi
{
    using UsbLib.Scsi.Commands;
        
    public class ScsiDevice
    {
        public Read10 Read10 { get => this.Commands[ScsiCommandCode.Read10] as Read10; }
        public ReadCapacity ReadCapacity { get => this.Commands[ScsiCommandCode.ReadCapacity] as ReadCapacity; }
        public Inquiry Inquiry { get => this.Commands[ScsiCommandCode.Inquiry] as Inquiry; }

        public Dictionary<ScsiCommandCode, ScsiCommand> Commands { get; private set; }

        public ScsiDevice()
        {
            this.Commands = new Dictionary<ScsiCommandCode, ScsiCommand>();
            this.Commands.Add(ScsiCommandCode.ReadCapacity, new ReadCapacity());
            this.Commands.Add(ScsiCommandCode.Read10, new Read10());
            this.Commands.Add(ScsiCommandCode.Inquiry, new Inquiry());
        }

        public ScsiCommand this[ScsiCommandCode code]
        {
            get
            {
                if (this.Commands.ContainsKey(code))
                    return this.Commands[code];
                else
                    throw new Exception($"Scsi command {code} not found!");
            }
        }
    }
}
