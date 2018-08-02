using System.Collections.Generic;
using SenseNet.Diagnostics;

namespace SenseNet.Tools.Diagnostics
{
    /// <summary>
    /// Defines methods for writing audit events.
    /// </summary>
    public interface IAuditEventWriter
    {
        /// <summary>
        /// Writes an audit event to a dedicated destination.
        /// </summary>
        /// <param name="auditEvent">The audit event to write.</param>
        /// <param name="properties">Additional properties.</param>
        void Write(IAuditEvent auditEvent, IDictionary<string, object> properties);
    }
}
