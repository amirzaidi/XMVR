namespace LibMesh.Data
{
    internal struct FaceEntry
    {
        internal int VId, VtId, VnId, KHId;

        internal FaceEntry(int vId, int vtId, int vnId, int khId)
        {
            VId = vId;
            VtId = vtId;
            VnId = vnId;
            KHId = khId;
        }

        internal readonly void Deconstruct(out int vId, out int vtId, out int vnId, out int khId)
        {
            vId = VId;
            vtId = VtId;
            vnId = VnId;
            khId = KHId;
        }
    }
}
