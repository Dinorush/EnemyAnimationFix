using EnemyAnimationFix.Utils;
using Iced.Intel;
using Il2CppInterop.Runtime.Runtime.VersionSpecific.Class;
using Il2CppInterop.Runtime.Runtime.VersionSpecific.MethodInfo;
using Il2CppInterop.Runtime.Runtime;
using Il2CppInterop.Runtime;
using System;
using System.IO;
using System.Runtime.InteropServices;
using EnemyAnimationFix.NativePatches.KernelTools;
using Enemies;

namespace EnemyAnimationFix.NativePatches
{
    internal static class ValidTargetPatches
    {
        const int NOP = 0x90;
        const int AddssLen = 8;

        public static unsafe void ApplyInstructionPatch()
        {
            INativeClassStruct classStruct = UnityVersionHandler.Wrap((Il2CppClass*)Il2CppClassPointerStore<EnemyAgent>.NativeClassPtr);
            for (int i = 0; i < classStruct.MethodCount; ++i)
            {
                INativeMethodInfoStruct methodInfoStruct = UnityVersionHandler.Wrap(classStruct.Methods[i]);
                if (Marshal.PtrToStringAnsi(methodInfoStruct.Name) == nameof(EnemyAgent.HasValidTarget))
                {
                    // Found the method, now find the address of the instruction we want to change.
                    IntPtr methodPointer = methodInfoStruct.MethodPointer;
                    IntPtr instructionIP = FindAdd(methodPointer);
                    if (instructionIP == IntPtr.Zero)
                    {
                        DinoLogger.Error("Unable to find instruction in HasValidTarget. Not applying fix for enemies stuck on spawning.");
                        return;
                    }    

                    // Change that instruction into `NOP`s.
                    using (new MemoryProtectionCookie(instructionIP, Kernel32.MemoryProtectionConstant.ExecuteReadWrite, new IntPtr(16)))
                    {
                        for (int j = 0; j < AddssLen; ++j)
                        {
                            *(byte*)((ulong)instructionIP + (ulong)new IntPtr(j)) = NOP;
                        }
                    }
                    return;
                }
            }
        }

        private static unsafe IntPtr FindAdd(IntPtr methodPointer)
        {            
            // Set up the decoder to go through the instructions.
            StreamCodeReader streamCodeReader = new(new UnmanagedMemoryStream((byte*)methodPointer, 65536L, 65536L, (FileAccess)1));
            Decoder decoder = Decoder.Create(sizeof(void*) * 8, streamCodeReader);
            decoder.IP = (ulong)(long)methodPointer;
            decoder.Decode(out Instruction instruction);

            while (instruction.Mnemonic != Mnemonic.Int3)
            // `Int3` is an opcode that is sometimes used to halt execution for a debugger. We
            // can be reasonably sure that it will appear after our method and never be inside
            // it.
            {
                if (instruction.Mnemonic == Mnemonic.Addss)
                {
                    // Error handling.
                    if ((instruction.NextIP - instruction.IP) != AddssLen)
                    {
                        DinoLogger.Error($"EnemyAnimationFix found an instruction with an unexpected width.");
                        return IntPtr.Zero;
                    }

                    return (IntPtr)(long)instruction.IP;
                }
                decoder.Decode(out instruction);
            }
            streamCodeReader.Stream.Dispose();

            return IntPtr.Zero;
        }
    }
}
