using HarmonyLib;
using Newtonsoft.Json.Linq;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Types = SDG.Unturned.Types;

namespace ThreadsafeBlock
{
    public class BlockV2 : Block
    {
        public byte[] CustomBuffer;

        public BlockV2(byte[] customBuffer, int prefix, byte[] contents) : base(prefix, contents)
        {
            CustomBuffer = customBuffer;
        }

        public BlockV2(byte[] customBuffer, byte[] contents) : base(contents)
        {
            CustomBuffer = customBuffer;
        }

        public BlockV2(byte[] customBuffer, int prefix) : base(prefix)
        {
            CustomBuffer = customBuffer;
        }

        public BlockV2(byte[] customBuffer) : base()
        {
            CustomBuffer = customBuffer;
        }
    }

    [HarmonyPatch(typeof(Block))]
    [HarmonyPatch("WriteBitConverterBytes")]
    class WriteBitConverterBytesPatch
    {
        [HarmonyPrefix]
        internal static bool Prefix(Block __instance, byte[] bytes)
        {
            if (__instance is not BlockV2 block) return true;


            int dstOffset = block.step;
            int count = bytes.Length;
            Buffer.BlockCopy(bytes, 0, block.CustomBuffer, dstOffset, count);


            return false;
        }
    }

    [HarmonyPatch(typeof(Block))]
    [HarmonyPatch("writeString")]
    class WriteStringPatch
    {
        [HarmonyPrefix]
        internal static bool Prefix(Block __instance, string value)
        {
            if (__instance is not BlockV2 block) return true;


            byte[] bytes = Encoding.UTF8.GetBytes(value);
            byte b = (byte)bytes.Length;
            block.CustomBuffer[block.step] = b;
            block.step++;
            Buffer.BlockCopy(bytes, 0, block.CustomBuffer, block.step, b);
            block.step += b;


            return false;
        }
    }

    [HarmonyPatch(typeof(Block))]
    [HarmonyPatch("writeBoolean")]
    class WriteBooleanPatch
    {
        [HarmonyPrefix]
        internal static bool Prefix(Block __instance, bool value)
        {
            if (__instance is not BlockV2 block) return true;


            byte[] bytes = BitConverter.GetBytes(value);
            block.CustomBuffer[block.step] = bytes[0];
            block.step++;


            return false;
        }
    }

    [HarmonyPatch(typeof(Block))]
    [HarmonyPatch("writeBooleanArray")]
    class WriteBooleanArrayPatch
    {
        [HarmonyPrefix]
        internal static bool Prefix(Block __instance, bool[] values)
        {
            if (__instance is not BlockV2 block) return true;


            block.writeUInt16((ushort)values.Length);
            ushort num = (ushort)Mathf.CeilToInt((float)values.Length / 8f);
            for (ushort num2 = 0; num2 < num; num2 = (ushort)(num2 + 1))
            {
                block.CustomBuffer[block.step + num2] = 0;
                byte b = 0;
                while (b < 8 && num2 * 8 + b < values.Length)
                {
                    if (values[num2 * 8 + b])
                    {
                        block.CustomBuffer[block.step + num2] |= Types.SHIFTS[b];
                    }
                    b = (byte)(b + 1);
                }
            }
            block.step += num;


            return false;
        }
    }

    [HarmonyPatch(typeof(Block))]
    [HarmonyPatch("writeByte")]
    class WriteBytePatch
    {
        [HarmonyPrefix]
        internal static bool Prefix(Block __instance, byte value)
        {
            if (__instance is not BlockV2 block) return true;


            block.CustomBuffer[block.step] = value;
            block.step++;


            return false;
        }
    }

    [HarmonyPatch(typeof(Block))]
    [HarmonyPatch("writeByteArray")]
    class WriteByteArrayPatch
    {
        [HarmonyPrefix]
        internal static bool Prefix(Block __instance, byte[] values)
        {
            if (__instance is not BlockV2 block) return true;


            if (values.Length < 30000)
            {
                if (block.longBinaryData)
                {
                    block.writeInt32(values.Length);
                    Buffer.BlockCopy(values, 0, block.CustomBuffer, block.step, values.Length);
                    block.step += values.Length;
                }
                else
                {
                    byte b = (byte)values.Length;
                    block.CustomBuffer[block.step] = b;
                    block.step++;
                    Buffer.BlockCopy(values, 0, block.CustomBuffer, block.step, b);
                    block.step += b;
                }
            }


            return false;
        }
    }

    [HarmonyPatch(typeof(Block))]
    [HarmonyPatch("getBytes")]
    class GetBytesPatch
    {
        [HarmonyPrefix]
        internal static bool Prefix(Block __instance, ref byte[] __result, out int size)
        {
            size = 0;
            if (__instance is not BlockV2 block) return true;


            if (block.block == null)
            {
                size = block.step;
                __result = block.CustomBuffer;
                return false;
            }
            size = block.block.Length;
            __result = block.block;


            return false;
        }
    }

    [HarmonyPatch(typeof(Block))]
    [HarmonyPatch("getHash")]
    class GetHashPatch
    {
        [HarmonyPrefix]
        internal static bool Prefix(Block __instance, ref byte[] __result)
        {
            if (__instance is not BlockV2 block) return true;


            if (block.block == null)
            {
                __result = Hash.SHA1(block.CustomBuffer);
                return false;
            }
            __result = Hash.SHA1(block.block);


            return false;
        }
    }

    [HarmonyPatch(typeof(Block))]
    [HarmonyPatch("")]
    class Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(Block __instance)
        {
            if (__instance is not BlockV2 block) return true;





            return false;
        }
    }
}
