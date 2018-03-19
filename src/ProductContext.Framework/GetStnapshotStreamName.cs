using System;

namespace ProductContext.Framework
{
    public delegate string GetSnapshotStreamName(Type type, string aggregateId);
}
