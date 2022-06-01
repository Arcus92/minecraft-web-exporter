using SharpNBT;

namespace MinecraftWebExporter.Minecraft.World
{
    /// <summary>
    /// A chunk reader helper
    /// </summary>
    public class ChunkHelper
    {
        /// <summary>
        /// Reads the given long array
        /// </summary>
        /// <param name="dataVersion"></param>
        /// <param name="bits"></param>
        /// <param name="count"></param>
        /// <param name="arrayTag"></param>
        /// <returns></returns>
        public static ushort[] ReadLongArray(int dataVersion, int bits, int count, LongArrayTag arrayTag)
        {
            // Reads the block states
            if (dataVersion >= 2529)
            {
                return ReadLongArraySince2529(bits, count, arrayTag);
            }
            else
            {
                return ReadLongArrayBefore2529(bits, count, arrayTag);
            }
        }
        
        /// <summary>
        /// Reads the long array from data version 2529
        /// </summary>
        /// <param name="bits"></param>
        /// <param name="count"></param>
        /// <param name="arrayTag"></param>
        /// <returns></returns>
        private static ushort[] ReadLongArraySince2529(int bits, int count, LongArrayTag arrayTag)
        {
            var bitMask = (ushort)(1 << bits);
            var components = 64 / bits;
                
            var result = new ushort[count];
            for (var i = 0; i < count; i++)
            {
                var l = i / components;
                var c = i % components;
                var lv = (ulong)arrayTag[l];
                result[i] = (ushort) ((lv >> (c * bits)) % bitMask);
            }

            return result;
        }
        
        /// <summary>
        /// Reads the long array before data version 2529
        /// </summary>
        /// <param name="bits"></param>
        /// <param name="count"></param>
        /// <param name="arrayTag"></param>
        /// <returns></returns>
        private static ushort[] ReadLongArrayBefore2529(int bits, int count, LongArrayTag arrayTag)
        {
            var bitMask = (ushort)(1 << bits);

            var i = 0;
            var longCount = arrayTag.Count;
            var result = new ushort[count];
            for (var l = 0; l < longCount; l++)
            {
                var lv = (ulong)arrayTag[l];
                var overhang = (bits - (l * 64) % bits) % bits;
                if (overhang > 0)
                {
                    result[i - 1] |= (ushort) ((lv % (ushort) (1 << overhang)) << (bits - overhang));
                    lv >>= overhang;
                }
                var remainingBits = 64 - overhang;
                var c = (remainingBits + bits - 1) / bits;
                for (var b = 0; b < c; b++)
                {
                    result[i++] = (ushort)(lv % bitMask);
                    lv >>= bits;
                    if (i >= count)
                        return result;
                }
            }

            return result;
        }
    }
}