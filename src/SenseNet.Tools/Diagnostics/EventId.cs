// ReSharper disable once CheckNamespace
// ReSharper disable UnusedMember.Global
namespace SenseNet.Diagnostics
{
    /// <summary>
    /// Contains well-known event ids in sensenet ECM.
    /// In your custom code please consider defining custom event ids and use these built-in values only 
    /// if the event is truly related to the feature it is defined for.
    /// </summary>
    public static class EventId
    {
        /// <summary>General, not classified event. Value: 1.</summary>
        public static readonly int NotDefined = 1;
        /// <summary>Event if the feature is not supported in the current context. Value: 10.</summary>
        public static readonly int NotSupported = 10;

        /// <summary>Serialization / deserialization related event. Value: 11</summary>
        public static readonly int Serialization = 11;

        /// <summary>General event for the Content Repository. Value: 20</summary>
        public static readonly int RepositoryRuntime = 20;
        /// <summary>Event during starting and stopping the Content Repository. Value:21</summary>
        public static readonly int RepositoryLifecycle = 21;

        /// <summary>Messaging related event. Used when communicating with other app domains. Value: 30</summary>
        public static readonly int Messaging = 30;
        /// <summary>Security related event. Value: 40</summary>
        public static readonly int Security = 40;
        /// <summary>Lucene indexing related event. Value: 50</summary>
        public static readonly int Indexing = 50;
        /// <summary>Content Query related event. Value: 60</summary>
        public static readonly int Querying = 60;
        /// <summary>ActionFramework related event. Value: 70</summary>
        public static readonly int ActionFramework = 70;
        /// <summary>DirectoryServices (AdSync) related event. Value: 80</summary>
        public static readonly int DirectoryServices = 80;
        /// <summary>Packaging (SnAdmin) related event. Value: 90</summary>
        public static readonly int Packaging = 90;
        /// <summary>TreeLock related event. Value: 100</summary>
        public static readonly int TreeLock = 100;
        /// <summary>Transaction related event. Value: 110</summary>
        public static readonly int Transaction = 110;
        /// <summary>Portal component (e.g. Portlet, ContentView etc.) related event. Value: 120</summary>
        public static readonly int Portal = 120;
        /// <summary>Service related event. Value: 130</summary>
        public static readonly int Services = 130;

        /// <summary>Load test event. Value: 140</summary>
        public static readonly int LoadTest = 140;
        /// <summary>SenseNet.Client related event. Value: 150</summary>
        public static readonly int ClientEvent = 150;
        /// <summary>Preview generation related event. Value: 160</summary>
        public static readonly int Preview = 160;

        /// <summary>Configuration related event.</summary>
        public static readonly int Configuration = 180;

        /// <summary>Contains Task Management related event ids.</summary>
        public static class TaskManagement
        {
            /// <summary>General task management event. Value: 500</summary>
            public static readonly int General = 500;
            /// <summary>Events during starting or stopping any task management related component. Value: 510</summary>
            public static readonly int Lifecycle = 510;
            /// <summary>Any communication related event in task management. Value: 520</summary>
            public static readonly int Communication = 520;
        }
    }
}
