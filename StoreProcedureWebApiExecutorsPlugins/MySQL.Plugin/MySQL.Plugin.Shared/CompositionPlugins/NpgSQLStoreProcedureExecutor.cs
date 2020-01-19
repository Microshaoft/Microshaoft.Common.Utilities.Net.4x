﻿namespace Microshaoft.CompositionPlugins
{
    using Microshaoft;
    using Npgsql;
    using System.Collections.Concurrent;
    using System.Composition;

    [Export(typeof(IStoreProcedureExecutable))]
    public class NpgSQLStoreProcedureExecutorCompositionPlugin
                        : AbstractStoreProcedureExecutorCompositionPlugin
                            <NpgsqlConnection, NpgsqlCommand, NpgsqlParameter>
    {
        public AbstractStoreProceduresExecutor
                    <NpgsqlConnection, NpgsqlCommand, NpgsqlParameter>
                        _executor;

        public override void InitializeOnDemand
                                    (
                                        ConcurrentDictionary<string, ExecutingInfo>
                                            executingCachingStore
                                    )
        {
            _executor = new NpgSqlStoreProceduresExecutor(executingCachingStore);
        }
        public override AbstractStoreProceduresExecutor<NpgsqlConnection, NpgsqlCommand, NpgsqlParameter> Executor
        {
            get => _executor;
        }

        public override string DataBaseType
        {
            get => "npgsql";
        }
    }
}
