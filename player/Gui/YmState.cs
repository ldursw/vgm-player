namespace VgmReader.Gui
{
    // Ported from XGM Player - SGDK
    class YmState
    {
        public YmCh[] Channels { get; } = new YmCh[6];
        public bool Ch3Special { get; private set; }

        private const short MIN_ATT_LEVEL = 0;
        private const int MAX_ATT_LEVEL = 0x7F << 6;

        private const ushort EG_ATT = 4;
        private const ushort EG_DEC = 3;
        private const ushort EG_SUS = 2;
        private const ushort EG_REL = 1;
        private const ushort EG_OFF = 0;

        private static readonly short[] ar_tab = new short[]
        {
            0x0, 0x1B, 0x48, 0xA3, 0x123, 0x222, 0x369, 0x5F9,
            0x91A, 0xF5C, 0x16C1, 0x1FC0, 0x1FC0, 0x1FC0, 0x1FC0, 0x1FC0,
            0x1FC0, 0x1FC0, 0x1FC0, 0x1FC0, 0x1FC0, 0x1FC0, 0x1FC0, 0x1FC0,
            0x1FC0, 0x1FC0, 0x1FC0, 0x1FC0, 0x1FC0, 0x1FC0, 0x1FC0, 0x1FC0
        };

        private static readonly short[] dr_tab = new short[]
        {
            0x0, 0x1, 0x5, 0xB, 0x15, 0x27, 0x3F, 0x6E,
            0xA8, 0x11C, 0x1A5, 0x2B7, 0x3F3, 0x66C, 0x939, 0xED2,
            0x1515, 0x1FC0, 0x1FC0, 0x1FC0, 0x1FC0, 0x1FC0, 0x1FC0, 0x1FC0,
            0x1FC0, 0x1FC0, 0x1FC0, 0x1FC0, 0x1FC0, 0x1FC0, 0x1FC0, 0x1FC0
        };

        public YmState()
        {
            for (var i = 0; i < Channels.Length; i++)
            {
                Channels[i] = new YmCh();
            }
        }

        public void Reset()
        {
            Ch3Special = false;

            for (var c = 0; c < 6; c++)
            {
                Reset(c);
            }
        }

        public void Write(byte port, byte address, byte value)
        {
            if ((address & 0xF0) > 0x20)
            {
                var c = (ushort)(address & 3);
                // invalid channel
                if (c == 3)
                {
                    return;
                }

                // set channel number
                if (port == 1)
                {
                    c += 3;
                }

                var ch = Channels[c];

                YM_write(ch, ch.slots[(address >> 2) & 3], address, value);
            }
            else if (address == 0x27)
            {
                // special mode
                Ch3Special = (value & 0x40) > 0;
            }
            else if (address == 0x28)
            {
                var c = (ushort)(value & 0x03);

                if (c == 3)
                {
                    return;
                }

                if ((value & 0x04) > 0)
                {
                    c += 3;
                }

                var ch = Channels[c];

                YM_Key(ch.slots[0], (value & 0x10) > 0);
                YM_Key(ch.slots[1], (value & 0x20) > 0);
                YM_Key(ch.slots[2], (value & 0x40) > 0);
                YM_Key(ch.slots[3], (value & 0x80) > 0);
            }
            else if (address == 0x2b && (value & 0x80) > 0)
            {
                Reset(5, false);
            }
        }

        public void Update()
        {
            foreach (var ch in Channels)
            {
                foreach (var slot in ch.slots)
                {
                    short env = slot.env;

                    switch (slot.ep)
                    {
                        case EG_ATT:    // attack phase
                            env -= slot.ar;

                            short tmp = (short)slot.tl;

                            // check phase transition
                            if (env <= tmp)
                            {
                                env = tmp;
                                slot.ep = (slot.sl <= tmp) ? EG_SUS : EG_DEC;
                            }
                            break;

                        case EG_DEC:    // decay phase
                            env = (short)(env + slot.d1r);

                            // check phase transition
                            if (env >= MAX_ATT_LEVEL)
                            {
                                env = MAX_ATT_LEVEL;
                                slot.ep = EG_OFF;
                            }
                            else if (env >= slot.sl)
                            {
                                slot.ep = EG_SUS;
                            }
                            break;

                        case EG_SUS:    // sustain phase
                            env = (short)(env + slot.d2r);

                            // check phase transition
                            if (env >= MAX_ATT_LEVEL)
                            {
                                env = MAX_ATT_LEVEL;
                                slot.ep = EG_OFF;
                            }
                            break;

                        case EG_REL:    // release phase
                            env = (short)(env + slot.rr);

                            // check phase transition
                            if (env >= MAX_ATT_LEVEL)
                            {
                                env = MAX_ATT_LEVEL;
                                slot.ep = EG_OFF;
                            }
                            break;
                    }

                    // update envelop
                    slot.env = env;
                }
            }
        }

        private void Reset(int channel, bool resetPan = true)
        {
            var ch = Channels[channel];

            ch.algo = 0;
            ch.freq = 0;
            ch.ext_freq = 0;

            if (resetPan)
            {
                ch.pan = 0;
            }

            for (var s = 0; s < 4; s++)
            {
                var slot = ch.slots[s];

                slot.mul = 0;

                slot.ep = EG_OFF;

                slot.env = MAX_ATT_LEVEL;
                slot.env_step = 0;

                slot.ar = 0;
                slot.d1r = 0;
                slot.d2r = 0;
                slot.rr = 0;

                slot.tl = MAX_ATT_LEVEL;
                slot.sl = MAX_ATT_LEVEL;

                slot.key = false;
                // default for algo 0
                slot.outSlot = (ushort)((s == 3) ? 1 : 0);
            }
        }

        private void YM_write(YmCh ch, YmSlot slot, ushort r, ushort v)
        {
            switch (r & 0xF0)
            {
                case 0x30:  // MUL
                    slot.mul = (ushort)(v & 0xF);
                    break;

                case 0x40:  // TL
                    slot.tl = (ushort)((v & 0x7F) << 6);
                    break;

                case 0x50:  // AR
                    slot.ar = ar_tab[v & 0x1F];
                    break;

                case 0x60:  // DR
                    slot.d1r = dr_tab[v & 0x1F];
                    break;

                case 0x70:  // SR
                    slot.d2r = dr_tab[v & 0x1F];
                    break;

                case 0x80:  // SL, RR
                    slot.sl = (ushort)((v & 0xF0) << 5);
                    slot.rr = dr_tab[((v & 0x0F) << 1) + 1];
                    break;

                case 0xa0:
                    switch ((r >> 2) & 3)
                    {
                        case 0: // FNUM1
                            ch.freq = (ushort)((ch.freq & 0xFF00) | v);
                            break;

                        case 1: // FNUM2
                            ch.freq = (ushort)((ch.freq & 0x00FF) | ((v & 0x3F) << 8));
                            break;

                        case 2: // FNUM1 - CH3
                            ch.ext_freq = (ushort)((ch.freq & 0xFF00) | v);
                            break;

                        case 3: // FNUM2 - CH3
                            ch.ext_freq = (ushort)((ch.freq & 0x00FF) | ((v & 0x3F) << 8));
                            break;
                    }
                    break;

                case 0xb0:
                    switch ((r >> 2) & 3)
                    {
                        case 0: // algo
                            ch.algo = (ushort)(v & 7);

                            switch (v & 7)
                            {
                                case 0:
                                    ch.slots[0].outSlot = 0;
                                    ch.slots[1].outSlot = 1;
                                    ch.slots[2].outSlot = 2;
                                    ch.slots[3].outSlot = 3;
                                    break;

                                case 1:
                                    ch.slots[0].outSlot = 1;
                                    ch.slots[1].outSlot = 1;
                                    ch.slots[2].outSlot = 2;
                                    ch.slots[3].outSlot = 3;
                                    break;

                                case 2:
                                    ch.slots[0].outSlot = 2;
                                    ch.slots[1].outSlot = 1;
                                    ch.slots[2].outSlot = 2;
                                    ch.slots[3].outSlot = 3;
                                    break;

                                case 3:
                                    ch.slots[0].outSlot = 1;
                                    ch.slots[1].outSlot = 2;
                                    ch.slots[2].outSlot = 2;
                                    ch.slots[3].outSlot = 3;
                                    break;

                                case 4:
                                    ch.slots[0].outSlot = 2;
                                    ch.slots[1].outSlot = 3;
                                    ch.slots[2].outSlot = 2;
                                    ch.slots[3].outSlot = 3;
                                    break;

                                case 5:
                                    ch.slots[0].outSlot = 1;
                                    ch.slots[1].outSlot = 3;
                                    ch.slots[2].outSlot = 3;
                                    ch.slots[3].outSlot = 3;
                                    break;

                                case 6:
                                    ch.slots[0].outSlot = 2;
                                    ch.slots[1].outSlot = 3;
                                    ch.slots[2].outSlot = 3;
                                    ch.slots[3].outSlot = 3;
                                    break;

                                case 7:
                                    ch.slots[0].outSlot = 3;
                                    ch.slots[1].outSlot = 3;
                                    ch.slots[2].outSlot = 3;
                                    ch.slots[3].outSlot = 3;
                                    break;
                            }
                            break;

                        case 1: // panning
                            ch.pan = (ushort)(v & 0xC0);
                            break;
                    }
                    break;
            }
        }

        private void YM_Key(YmSlot slot, bool on)
        {
            if (on)
            {
                YM_KeyOn(slot);
            }
            else
            {
                YM_KeyOff(slot);
            }
        }

        private void YM_KeyOn(YmSlot slot)
        {
            // no change
            if (slot.key)
            {
                return;
            }

            if (slot.ar < 94)
            {
                slot.ep = (slot.env <= MIN_ATT_LEVEL) ?
                    ((slot.sl == MIN_ATT_LEVEL) ? EG_SUS : EG_DEC) : EG_ATT;
            }
            else
            {
                // set envelop to MIN ATTENUATION
                slot.env = MIN_ATT_LEVEL;
                // and directly switch to Decay (or Sustain) */
                slot.ep = (slot.sl == MIN_ATT_LEVEL) ? EG_SUS : EG_DEC;
            }

            slot.key = true;
        }

        private void YM_KeyOff(YmSlot slot)
        {
            // no change
            if (!slot.key)
            {
                return;
            }

            if (slot.ep > EG_REL)
            {
                slot.ep = EG_REL;     // pass to release phase
            }

            slot.key = false;
        }

        public class YmSlot
        {
            public ushort mul;        // freq multiplier

            public ushort ep;         // current envelop phase

            public short env;         // current envelop level (comparable to tl and sl)
            public short env_step;    // envelop step (ar, d1r, d2r or rr depending env phase)

            public short ar;          // attack rate
            public short d1r;         // decay rate
            public short d2r;         // substain rate
            public short rr;          // release rate

            public ushort tl;         // total level (min = 0, max = 32768)
            public ushort sl;         // substain level (comparable to tl)

            public bool key;        // key on/off state
            public ushort outSlot;    // out slot type (depending algo)
        }

        public class YmCh
        {
            public readonly YmSlot[] slots = new YmSlot[4];

            public ushort algo;
            public ushort freq;
            public ushort ext_freq;       // (for CH3 special mode)
            public ushort pan;

            public YmCh()
            {
                for (int i = 0; i < slots.Length; i++)
                {
                    slots[i] = new YmSlot();
                }
            }
        }
    }
}
