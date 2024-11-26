using System;
using System.Runtime.InteropServices;

namespace EnemyAnimationFix.NativePatches.KernelTools
{
    internal static class Kernel32
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr VirtualAlloc(
          IntPtr lpAddress,
          IntPtr dwSize,
          MemoryAllocationConstant flAllocationType,
          MemoryProtectionConstant flProtect);

        [DllImport("kernel32.dll")]
        public static extern bool VirtualProtect(
          IntPtr lpAddress,
          IntPtr dwSize,
          MemoryProtectionConstant flNewProtect,
          out MemoryProtectionConstant lpflOldProtect);

        [Flags]
        public enum MemoryAllocationConstant : uint
        {
            Commit = 4096, // 0x00001000
            Reserve = 8192, // 0x00002000
            Reset = 524288, // 0x00080000
            ResetUndo = 16777216, // 0x01000000
            LargePages = 536870912, // 0x20000000
            Physical = 4194304, // 0x00400000
            TopDown = 1048576, // 0x00100000
            WriteWatch = 2097152, // 0x00200000
        }

        [Flags]
        public enum MemoryProtectionConstant : uint
        {
            NoAccess = 1,
            ReadOnly = 2,
            ReadWrite = 4,
            WriteCopy = 8,
            Execute = 16, // 0x00000010
            ExecuteRead = 32, // 0x00000020
            ExecuteReadWrite = 64, // 0x00000040
            ExecuteWriteCopy = 128, // 0x00000080
        }
    }
}