using System;
using System.Collections.Generic;
using System.Text;
using SenseNet.Diagnostics;

namespace SenseNet.Tools.Diagnostics
{
    public interface IAuditEventWriter
    {
        void Write(IAuditEvent auditEvent, IDictionary<string, object> properties);
    }
}
