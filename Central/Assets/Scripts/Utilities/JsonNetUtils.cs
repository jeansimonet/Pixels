using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json
{
    public class IgnoreThisConverter
        : System.IDisposable
    {
        JsonSerializer serializer;
        JsonConverter saveConverter;
        ReferenceLoopHandling saveLoophandling;
        int saveIndex;

        public IgnoreThisConverter(JsonSerializer serializer, JsonConverter converter)
        {
            this.serializer = serializer;
            this.saveConverter = converter;
            this.saveLoophandling = serializer.ReferenceLoopHandling;
            serializer.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
            this.saveIndex = serializer.Converters.IndexOf(converter);
            serializer.Converters.RemoveAt(saveIndex);
        }

        public void Dispose()
        {
            serializer.Converters.Insert(saveIndex, saveConverter);
            serializer.ReferenceLoopHandling = saveLoophandling;
        }
    }
}
