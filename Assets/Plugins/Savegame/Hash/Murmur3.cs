using System;
using System.Drawing;

public static class Murmur3
{
    private static uint Murmur32_Scramble(uint k)
    {
        k = (k * 0x16A88000) | ((k * 0xCC9E2D51) >> 17);
        k *= 0x1B873593;
        return k;
    }

    /// <summary>
    /// Finds a hash region which fits the hash at the beginning of the data
    /// </summary>
    /// <param name="data">The data to search in</param>
    /// <param name="offset">The offset, starting at the hash</param>
    /// <param name="minSize">Min size to look for</param>
    /// <param name="maxSize">Max size to look for</param>
    /// <param name="seed">Seed</param>
    /// <returns></returns>
    public static uint FindBlock(in byte[] data, int offset, uint minSize, uint maxSize, uint seed = 0)
    {
        UnityEngine.Debug.Log("Offset: " + offset.ToString("X8"));
        uint originalHash = BitConverter.ToUInt32(data, offset);
        uint checksum = seed;
        offset += 4;
        var end = data.Length - offset;
        if (maxSize > end) maxSize = (uint) end;
        for (var i = 0; i < end / 4; i++)
        {
            var iOffset = i * 4;
            var val = BitConverter.ToUInt32(data, offset + iOffset);
            checksum ^= Murmur32_Scramble(val);
            checksum = (checksum >> 19) | (checksum << 13);
            checksum = checksum * 5 + 0xE6546B64;
            if (iOffset >= minSize)
            {
                var tempHash = checksum;
                tempHash ^= (uint) (iOffset + 4);
                tempHash ^= tempHash >> 16;
                tempHash *= 0x85EBCA6B;
                tempHash ^= tempHash >> 13;
                tempHash *= 0xC2B2AE35;
                tempHash ^= tempHash >> 16;
                if (tempHash == originalHash)
                {
                    return (uint) (i * 4 + 4);
                }
            }
        }
        return 0;
    }

    public static uint GetMurmur3Hash(in byte[] data, int offset, uint size, uint seed = 0)
    {
        uint checksum = seed;
        if (size > 3)
        {
            for (var i = 0; i < (size / sizeof(uint)); i++)
            {
                var val = BitConverter.ToUInt32(data, offset);
                checksum ^= Murmur32_Scramble(val);
                checksum = (checksum >> 19) | (checksum << 13);
                checksum = checksum * 5 + 0xE6546B64;
                offset += 4;
            }
        }

        var remainder = size % sizeof(uint);
        if (remainder != 0)
        {
            uint val = BitConverter.ToUInt32(data, (int)((offset + size) - remainder));
            for (var i = 0; i < (sizeof(uint) - remainder); i++)
                val >>= 8;
            checksum ^= Murmur32_Scramble(val);
        }

        checksum ^= size;
        checksum ^= checksum >> 16;
        checksum *= 0x85EBCA6B;
        checksum ^= checksum >> 13;
        checksum *= 0xC2B2AE35;
        checksum ^= checksum >> 16;
        return checksum;
    }

    public static uint UpdateMurmur32(in byte[] data, int hashOffset, int readOffset, uint readSize)
    {
        var newHash = GetMurmur3Hash(data, readOffset, readSize);
        Array.Copy(BitConverter.GetBytes(newHash), 0, data, hashOffset, 4);
        return newHash;
    }

    public static bool VerifyMurmur32(in byte[] data, int hashOffset, int readOffset, uint readSize)
        => BitConverter.ToUInt32(data, hashOffset) == GetMurmur3Hash(data, readOffset, readSize);
}
