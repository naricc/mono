//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Runtime.DurableInstancing;
    using System.Transactions;
    using System.Xml.Linq;

    sealed class ExtendLockAsyncResult : SqlWorkflowInstanceStoreAsyncResult
    {
        static readonly string commandText = string.Format(CultureInfo.InvariantCulture, "{0}.[ExtendLock]", SqlWorkflowInstanceStoreConstants.DefaultSchema);

        public ExtendLockAsyncResult
            (
            InstancePersistenceContext context,
            InstancePersistenceCommand command,
            SqlWorkflowInstanceStore store,
            SqlWorkflowInstanceStoreLock storeLock,
            Transaction currentTransaction,
            TimeSpan timeout, 
            AsyncCallback callback, 
            object state
            ) :
            base(context, command, store, storeLock, currentTransaction, timeout, int.MaxValue, callback, state)
        {
        }

        protected override string ConnectionString
        {
            get
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(base.Store.CachedConnectionString);
                builder.ApplicationName = SqlWorkflowInstanceStore.CommonConnectionPoolName;
                return builder.ToString();
            }
        }

        protected override void GenerateSqlCommand(SqlCommand sqlCommand)
        {
            long surrogateOwnerId = base.StoreLock.SurrogateLockOwnerId;
            double lockTimeout = base.Store.BufferedHostLockRenewalPeriod.TotalSeconds;

            sqlCommand.Parameters.Add(new SqlParameter { ParameterName = "@surrogateLockOwnerId", SqlDbType = SqlDbType.BigInt, Value = surrogateOwnerId });
            sqlCommand.Parameters.Add(new SqlParameter { ParameterName = "@lockTimeout", SqlDbType = SqlDbType.Int, Value = lockTimeout });
        }

        protected override string GetSqlCommandText()
        {
            return ExtendLockAsyncResult.commandText;
        }

        protected override CommandType GetSqlCommandType()
        {
            return CommandType.StoredProcedure;
        }

        protected override Exception ProcessSqlResult(SqlDataReader reader)
        {
            return StoreUtilities.CheckRemainingResultSetForErrors(base.InstancePersistenceCommand.Name, reader);
        }
    }
}
