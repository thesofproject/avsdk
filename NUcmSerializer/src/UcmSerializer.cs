//
// Copyright (c) 2018, Intel Corporation. All rights reserved.
//
// Author: Cezary Rojewski <cezary.rojewski@intel.com>
//
// SPDX-License-Identifier: Apache-2.0 OR MIT
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NUcmSerializer
{
    public class UcmSerializer
    {
        public UcmSerializer()
        {
            // TODO: check semantics of type to be serialized/ deserialized
        }

        public void Serialize(Stream stream, IEnumerable<Section> sections, Encoding encoding)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (sections == null)
                throw new ArgumentNullException("sections");

            var writer = new UcmWriter(stream, encoding);

            foreach (var section in sections)
            {
                if (section == null)
                    throw new ArgumentNullException("section");
                writer.WriteToken(section, null);
            }

            writer.Flush();
            writer.Dispose();
        }

        public void Serialize(Stream stream, IEnumerable<Section> topology)
        {
            Serialize(stream, topology, null);
        }

        public IEnumerable<Section> Deserialize(Stream stream, Encoding encoding)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            List<Section> result = new List<Section>();
            Section section;

            using (var reader = new UcmReader(stream, encoding))
            {
                while ((section = reader.ReadToken()) != null)
                    result.Add(section);
            }

            return result;
        }

        public IEnumerable<Section> Deserialize(Stream stream)
        {
            return Deserialize(stream, null);
        }
    }
}
