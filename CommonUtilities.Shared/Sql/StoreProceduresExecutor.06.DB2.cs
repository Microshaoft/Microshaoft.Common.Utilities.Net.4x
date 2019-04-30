﻿//#if !XAMARIN && NETFRAMEWORK4_X
namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Data;
    using IBM.Data.DB2.Core;
    public class DB2StoreProceduresExecutor
                    : AbstractStoreProceduresExecutor
                        <DB2Connection, DB2Command, DB2Parameter>
    {
        protected override DB2Parameter
                        OnQueryDefinitionsSetInputParameterProcess
                            (
                                DB2Parameter parameter
                            )
        {
            parameter.DB2Type = DB2Type.VarChar;
            return parameter;
        }
        protected override DB2Parameter
                        OnQueryDefinitionsSetReturnParameterProcess
                            (
                                DB2Parameter parameter
                            )
        {
            parameter.DB2Type = DB2Type.Integer;
            return parameter;
        }
        protected override DB2Parameter
                       OnQueryDefinitionsReadOneDbParameterProcess
                           (
                               IDataReader reader
                               , DB2Parameter parameter
                               , string connectionString
                           )
        {
            var dbTypeName = (string)(reader["DATA_TYPE"]);
            DB2Type dbType = (DB2Type) Enum.Parse(typeof(DB2Type), dbTypeName, true);
            parameter
                .DB2Type = dbType;
            if (parameter.DB2Type == DB2Type.Decimal)
            {
                var o = reader["NUMERIC_SCALE"];
                if (o != DBNull.Value)
                {
                    parameter
                        .Scale =
                            (
                                (byte)
                                    (
                                        (
                                            (short)
                                                (
                                                    (int)o
                                                )
                                        )
                                    //& 255
                                    )
                            );
                }
                o = reader["NUMERIC_PRECISION"];
                if (o != DBNull.Value)
                {
                    parameter.Precision = ((byte)o);
                }
            }
            return parameter;
        }
        protected override DB2Parameter
                    OnExecutingSetDbParameterTypeProcess
                        (
                            DB2Parameter definitionParameter
                            , DB2Parameter cloneParameter
                        )
        {
            cloneParameter.DB2Type = definitionParameter.DB2Type;
            return cloneParameter;
        }
        protected override object
               OnExecutingSetDbParameterValueProcess
                    (
                        DB2Parameter parameter
                        , JToken jValue
                    )
        {
            return
                parameter
                    .SetGetValueAsObject(jValue);
        }
    }
}
//#endif