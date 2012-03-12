﻿using Composite.C1Console.Events;


namespace Composite.Plugins.Data.DataProviders.MSSqlServerDataProvider.Sql
{
    /// <summary>    
    /// </summary>
    /// <exclude />
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)] 
    public static class SqlTableInformationStore
    {
        private static ISqlTableInformationStore _implementation = new SqlTableInformationStoreImpl();

        internal static ISqlTableInformationStore Implementation { get { return _implementation; } set { _implementation = value; } }


        static SqlTableInformationStore()
        {
            GlobalEventSystemFacade.SubscribeToFlushEvent(OnFlush);
        }



        /// <exclude />
        public static ISqlTableInformation GetTableInformation(string connectionString, string tableName)
        {
            return _implementation.GetTableInformation(connectionString, tableName);
        }



        internal static void Flush()
        {
            _implementation.OnFlush();
        }



        private static void OnFlush(FlushEventArgs flushEventArgs)
        {
            _implementation.OnFlush();
        }
    }
}