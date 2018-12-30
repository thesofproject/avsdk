using System;
using System.Collections.Generic;
using System.IO;

namespace NUmcSerializer
{
    public class UmcSerializer
    {
        public UmcSerializer()
        {
            // TODO: check semantics of type to be serialized/ deserialized
        }

        public void Serialize(Stream stream, IEnumerable<Section> topology)
        {
            if (topology == null)
                throw new ArgumentNullException("topology");

            var writer = new UmcTextWriter(stream, null);

            foreach (var section in topology)
                writer.WriteToken(section, null);
            writer.Flush();
            writer.Close();
        }

        public IEnumerable<Section> Deserialize(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
