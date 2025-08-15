using System.Text;

namespace System.Management
{
    public enum VideoMemoryType: ushort
    {
        Other = 1,
        Unknown = 2,
        VRAM = 3,
        DRAM = 4,
        SRAM = 5,
        WRAM = 6,
        EDORAM = 7,
        BSDRAM = 8,
        PBSRAM = 9,
        CDRAM = 10,
        ThreeDRAM = 11,
        SDRAM = 12,
        SGRAM = 13,
    }

    public enum VideoArchitecture : ushort
    {
        Other = 1,
        Unknown = 2,
        CGA = 3,
        EGA = 4,
        VGA = 5,
        SVGA = 6,
        MDA = 7,
        HGC = 8,
        MCGA = 9,
        Eight514A = 10,
        XGA = 11,
        LinearFrameBuffer = 12,
        PC98 = 160
    }

}

namespace sysinfo
{
    public class ConsoleInterceptor : TextWriter
    {
        private readonly TextWriter originalOut;
        private int linesWritten = 0;

        public ConsoleInterceptor(TextWriter originalOut)
        {
            this.originalOut = originalOut;
        }

        public override Encoding Encoding => originalOut.Encoding;

        public bool canCount = true;

        public override void Write(char value)
        {
            originalOut.Write(value);

            // Count newlines anywhere
            if (value == '\n')
                CheckForPause();
        }

        public override void Write(string? value)
        {
            if (value != null)
            {
                originalOut.Write(value);

                // Count embedded newlines in the string
                foreach (char c in value)
                    if (c == '\n')
                        CheckForPause();
            }
            else
            {
                originalOut.Write(value);
            }
        }

        private void CheckForPause()
        {
            if (!canCount) return;

            linesWritten++;

            if (linesWritten % 32 == 0)
            {
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                Console.ForegroundColor = ConsoleColor.White;
                canCount = false;
                Console.Write("Press Enter to continue...");
                Console.ResetColor();
                Console.ReadLine();
                canCount = true;
            }
        }
    }
}
