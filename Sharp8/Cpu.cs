using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharp8
{
    class Cpu
    {
        private const int memorySize = 0x1000;
        private const int displaySize = 64 * 32;
        private const int stackSize = 16;
        private const int registerCount = 16;

        // The current opcode being read by the CPU
        ushort opcode;

        /* The RAM the CPU has access to
         * 0x000-0x1FF - CHIP-8 interpreter
         * 0x050-0x0A0 - Used for the built-in 4x5 pixel font (0-F)
         * 0x200-0xFFF - Program ROM and work RAM */
        byte[] memory = new byte[memorySize];

        /* 16 8-bit registers.
         * V0-VE are general purpose
         * VF is a carry flag */
        byte[] V = new byte[registerCount];

        // Index register
        ushort I;

        // Program counter
        ushort pc;

        /* The graphics of the CHIP-8 can be
         * represented as either 1 or 0 per pixel */
        byte[] gfx = new byte[displaySize];

        /* Timers that count at 60Hz. Will count down to zero when set above zero.
         * When the sound timer is not zero, a buzzing sound will be made. */
        byte delayTimer;
        byte soundTimer;

        // Stack and stack pointer for keeping track of jumps
        ushort[] stack = new ushort[stackSize];
        ushort sp;

        // Hex-based keypad
        byte[] keys = new byte[16];

        /* A 4x5 pixel font with characters
         * 0-9 and A-F */
        byte[] font =
        {
            0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
            0x20, 0x60, 0x20, 0x20, 0x70, // 1
            0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
            0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
            0x90, 0x90, 0xF0, 0x10, 0x10, // 4
            0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
            0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
            0xF0, 0x10, 0x20, 0x40, 0x40, // 7
            0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
            0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
            0xF0, 0x90, 0xF0, 0x90, 0x90, // A
            0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
            0xF0, 0x80, 0x80, 0x80, 0xF0, // C
            0xE0, 0x90, 0x90, 0x90, 0xE0, // D
            0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
            0xF0, 0x80, 0xF0, 0x80, 0x80, // F
        };

        /// <summary>
        /// Initialises system and sets up memory
        /// </summary>
        public void Init()
        {
            pc = 0x200;
            opcode = 0;
            I = 0;
            sp = 0;

            Array.Clear(gfx, 0, displaySize);
            Array.Clear(stack, 0, stackSize);
            Array.Clear(V, 0, registerCount);
            Array.Clear(memory, 0, memorySize);

            // Copy font in to memory
            Array.Copy(font, 0, memory, 0x50, font.Length);

            delayTimer = 0;
            soundTimer = 0;
        }

        /// <summary>
        /// Reads in the specified game and loads it into memory
        /// </summary>
        /// <param name="gameName"></param>
        /// <returns></returns>
        public bool LoadGame(string gameName)
        {
            return true;
        }

        /// <summary>
        /// Fetches an opcode from memory
        /// </summary>
        public void AdvanceCycle()
        {

        }

        /// <summary>
        /// Decodes the current opcode and executes it
        /// </summary>
        private void DecodeOpcode()
        {

        }
    }
}
