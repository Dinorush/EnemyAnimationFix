using System;

namespace EnemyAnimationFix.NativePatches.KernelTools
{
    internal struct MemoryProtectionCookie : IDisposable
    {
        private Kernel32.MemoryProtectionConstant m_OldProtection;
        private readonly IntPtr m_Address;
        private readonly IntPtr m_Size;

        public MemoryProtectionCookie(IntPtr address, Kernel32.MemoryProtectionConstant newProtection, IntPtr size)
        {
            this.m_Address = address;
            this.m_Size = size;
            if (!Kernel32.VirtualProtect(this.m_Address, this.m_Size, newProtection, out this.m_OldProtection))
            {
                Environment.FailFast($"Failed to protect address 0x{this.m_Address:X2} with protection {newProtection}");
            }
        }

        public void Dispose()
        {
            if (!Kernel32.VirtualProtect(this.m_Address, this.m_Size, this.m_OldProtection, out this.m_OldProtection))
            {
                Environment.FailFast($"Failed to revert memory protection of address 0x{this.m_Address:X2}");
            }
        }
    }
}
