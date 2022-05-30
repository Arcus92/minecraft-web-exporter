namespace MinecraftWebExporter.Minecraft.World
{
    /// <summary>
    /// The chunk offset in a <see cref="RegionFile"/>
    /// </summary>
    public readonly struct RegionChunkOffset
    {
        public RegionChunkOffset(int offset, byte size)
        {
            Offset = offset;
            Size = size;
        }

        /// <summary>
        /// Gets the byte offset
        /// </summary>
        public int Offset { get; init; }

        /// <summary>
        /// Gets the chunk size
        /// </summary>
        public byte Size { get; init; }

        public override string ToString()
        {
            return $"{nameof(Offset)}: {Offset}, {nameof(Size)}: {Size}";
        }
    }
}