using System;
using System.Collections.Generic;
using System.IO;
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

        public bool ShouldDraw { get; private set; }

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
            Array.Copy(font, memory, font.Length);

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
            var buffer = File.ReadAllBytes(gameName);
            Array.Copy(buffer, 0, memory, 0x200, buffer.Length);
            return true;
        }

        /// <summary>
        /// Fetches and decodes an opcode
        /// </summary>
        public void AdvanceCycle()
        {
            FetchOpcode();
            DecodeOpcode();
        }

        /// <summary>
        /// Fetches an opcode from memory
        /// </summary>
        public void FetchOpcode()
        {
            /* Each opcode is 2 bytes, so load the first byte
             * then shift left by 8, then load the second byte */
            opcode = (ushort)(memory[pc] << 8 | memory[pc + 1]);
        }

        /// <summary>
        /// Decodes the current opcode and executes it
        /// </summary>
        private void DecodeOpcode()
        {
            switch (opcode & 0xF000)
            {
                case 0x0000:
                {
                    switch (opcode & 0x00FF)
                    {
                        default:
                        Console.WriteLine("Unknown opcode [0x0000]: 0x{0:X4}", opcode);
                        break;
                    }
                }
                break;

                case 0x1000:
                {
                    JumpToAddress();
                }
                break;

                case 0x2000:
                {
                    JumpToSubroutine();
                }
                break;

                case 0x3000:
                {
                    SkipIfRegisterEquals();
                }
                break;

                case 0x6000:
                {
                    SetRegister();
                }
                break;

                case 0x7000:
                {
                    AddToRegister();
                }
                break;

                case 0x8000:
                {
                    switch (opcode & 0x000F)
                    {
                        case 0x0000:
                        {
                            AssignRegisterToRegister();
                        }
                        break;

                        default:
                        Console.WriteLine("Unknown opcode [0x8000]: 0x{0:X4}", opcode);
                        break;
                    }
                }
                break;

                case 0xA000:
                {
                    SetIndex();
                }
                break;

                case 0xD000:
                {
                    Draw();
                }
                break;

                case 0xE000:
                {
                    switch (opcode & 0x00FF)
                    {
                        case 0x00A1:
                        {
                            SkipInstructionIfKeyUp();
                        }
                        break;

                        default:
                        Console.WriteLine("Unknown opcode [0xE000]: 0x{0:X4}", opcode);
                        break;
                    }
                }
                break;

                case 0xF000:
                {
                    switch (opcode & 0x00FF)
                    {
                        case 0x001E:
                        {
                            AddRegisterToIndex();
                        }
                        break;

                        default:
                        Console.WriteLine("Unknown opcode [0xF000]: 0x{0:X4}", opcode);
                        break;
                    }
                }
                break;

                default:
                Console.WriteLine("Unknown opcode: 0x{0:X4}", opcode);
                break;
            }
        }

        private void JumpToAddress()
        {
            pc = (ushort)(opcode & 0x0FFF);
            Console.WriteLine("0x{0:X4}: Jumped to address 0x{1:X4}", opcode, opcode & 0x0FFF);
        }

        private void JumpToSubroutine()
        {
            stack[sp] = pc;
            sp++;
            pc = (ushort)(opcode & 0x0FFF);
            Console.WriteLine("0x{0:X4}: Jumped to subroutine 0x{1:X4}", opcode, opcode & 0x0FFF);
        }

        private void SkipIfRegisterEquals()
        {
            if (V[(opcode & 0x0F00) >> 8] == (opcode & 0x00FF))
            {
                pc += 4;
                Console.WriteLine("0x{0:X4}: Register {1} equalled 0x{2:X4}, skipped next instruction", opcode, (opcode & 0x0F00) >> 8, opcode & 0x00FF);
                return;
            }

            pc += 2;
            Console.WriteLine("0x{0:X4}: Register {1} did not equal 0x{2:X4}, did not skip instruction", opcode, (opcode & 0x0F00) >> 8, opcode & 0x00FF);
        }

        private void SetRegister()
        {
            V[(opcode & 0x0F00) >> 8] = (byte)(opcode & 0x00FF);
            pc += 2;
            Console.WriteLine("0x{0:X4}: Set register {1} to 0x{2:X4}", opcode, (opcode & 0x0F00) >> 8, opcode & 0x00FF);
        }

        private void AddToRegister()
        {
            V[(opcode & 0x0F00) >> 8] += (byte)(opcode & 0x00FF);
            pc += 2;
            Console.WriteLine("0x{0:X4}: Added 0x{1:X4} to register {2}", opcode, opcode & 0x00FF, (opcode & 0x0F00) >> 8);
        }

        private void AssignRegisterToRegister()
        {
            V[(opcode & 0x0F00) >> 8] = V[(opcode & 0x00F0) >> 4];
            pc += 2;
            Console.WriteLine("0x{0:X4}: Set register {1} to register {2}", opcode, (opcode & 0x0F00) >> 8, (opcode & 0x00F0) >> 4);
        }

        private void SetIndex()
        {
            I = (ushort)(opcode & 0x0FFF);
            pc += 2;
            Console.WriteLine("0x{0:X4}: Set Index to 0x{1:X4}", opcode, opcode & 0x0FFF);
        }

        private void Draw()
        {
            ushort x = V[(opcode & 0x0F00) >> 8];
            ushort y = V[(opcode & 0x00F0) >> 4];
            ushort height = (ushort)(opcode & 0x000F);
            ushort pixel;

            V[0xF] = 0;
            for (var yline = 0; yline < height; yline++)
            {
                pixel = memory[I + yline];
                for (var xline = 0; xline < 8; xline++)
                {
                    if ((pixel & (0x80 >> xline)) != 0)
                    {
                        if (gfx[(x + xline + ((y + yline) * 64))] == 1)
                        {
                            V[0xF] = 1;
                        }

                        gfx[x + xline + ((y + yline) * 64)] ^= 1;
                    }
                }
            }

            ShouldDraw = true;
            pc += 2;
            Console.WriteLine("0x{0:X4}: Drawing", opcode);
        }

        private void SkipInstructionIfKeyUp()
        {
            if (keys[V[(opcode & 0x0F00) >> 8]] == 0)
            {
                pc += 4;
                Console.WriteLine("0x{0:X4}: Key {1} was not pressed, instruction was skipped", opcode, V[(opcode & 0x0F00) >> 8]);
                return;
            }

            pc += 2;
            Console.WriteLine("0x{0:X4}: Key {1} was pressed, instruction was not skipped", opcode, V[(opcode & 0x0F00) >> 8]);
        }

        private void AddRegisterToIndex()
        {
            I += V[(opcode & 0x0F00) >> 8];
            pc += 2;
            Console.WriteLine("0x{0:X4}: Added 0x{1:X4} to Index", opcode, V[(opcode & 0x0F00) >> 8]);
        }
    }
}
